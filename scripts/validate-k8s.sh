#!/usr/bin/env bash
set -euo pipefail

NAMESPACE="${NAMESPACE:-sharpchess-validation}"
APP_DEPLOYMENT="${APP_DEPLOYMENT:-sharpchess-api}"
DB_DEPLOYMENT="${DB_DEPLOYMENT:-sharpchess-db}"
SERVICE_NAME="${SERVICE_NAME:-sharpchess-api}"
CLIENT_POD="${CLIENT_POD:-validation-client}"
OLD_VERSION="${OLD_VERSION:-validation-v1}"
NEW_VERSION="${NEW_VERSION:-validation-v2}"

log() {
  printf '[k8s-validation] %s\n' "$*" >&2
}

cleanup() {
  kubectl -n "${NAMESPACE}" delete pod "${CLIENT_POD}" --ignore-not-found >/dev/null 2>&1 || true
}

service_request() {
  local path="$1"
  kubectl -n "${NAMESPACE}" exec "${CLIENT_POD}" -- wget -qO- -T 2 "http://${SERVICE_NAME}:8080${path}"
}

wait_for_service() {
  local path="$1"
  local description="$2"
  local attempts="${3:-60}"
  local sleep_seconds="${4:-2}"
  local response=""
  local attempt=0

  for ((attempt = 1; attempt <= attempts; attempt++)); do
    if response="$(service_request "${path}" 2>/dev/null)"; then
      log "${description} is reachable."
      printf '%s' "${response}"
      return 0
    fi
    sleep "${sleep_seconds}"
  done

  log "Timed out waiting for ${description}."
  return 1
}

wait_for_restart() {
  local pod_name="$1"
  local initial_restarts="$2"
  local current_restarts=0
  local attempt=0

  for ((attempt = 1; attempt <= 60; attempt++)); do
    current_restarts="$(kubectl -n "${NAMESPACE}" get pod "${pod_name}" -o jsonpath='{.status.containerStatuses[0].restartCount}' 2>/dev/null || echo 0)"
    if [[ "${current_restarts}" -gt "${initial_restarts}" ]]; then
      log "Observed restart count increase on ${pod_name}: ${initial_restarts} -> ${current_restarts}."
      return 0
    fi
    sleep 2
  done

  log "Timed out waiting for restart count to increase on ${pod_name}."
  return 1
}

wait_for_deployment_ready() {
  local deployment_name="$1"
  local attempts="${2:-90}"
  local sleep_seconds="${3:-2}"
  local desired_replicas=0
  local updated_replicas=0
  local ready_replicas=0
  local available_replicas=0
  local unavailable_replicas=0
  local attempt=0

  desired_replicas="$(kubectl -n "${NAMESPACE}" get "deployment/${deployment_name}" -o jsonpath='{.spec.replicas}')"

  for ((attempt = 1; attempt <= attempts; attempt++)); do
    updated_replicas="$(kubectl -n "${NAMESPACE}" get "deployment/${deployment_name}" -o jsonpath='{.status.updatedReplicas}')"
    ready_replicas="$(kubectl -n "${NAMESPACE}" get "deployment/${deployment_name}" -o jsonpath='{.status.readyReplicas}')"
    available_replicas="$(kubectl -n "${NAMESPACE}" get "deployment/${deployment_name}" -o jsonpath='{.status.availableReplicas}')"
    unavailable_replicas="$(kubectl -n "${NAMESPACE}" get "deployment/${deployment_name}" -o jsonpath='{.status.unavailableReplicas}')"

    updated_replicas="${updated_replicas:-0}"
    ready_replicas="${ready_replicas:-0}"
    available_replicas="${available_replicas:-0}"
    unavailable_replicas="${unavailable_replicas:-0}"

    if [[ "${updated_replicas}" == "${desired_replicas}" &&
      "${ready_replicas}" == "${desired_replicas}" &&
      "${available_replicas}" == "${desired_replicas}" &&
      "${unavailable_replicas}" == "0" ]]; then
      return 0
    fi

    sleep "${sleep_seconds}"
  done

  log "Timed out waiting for deployment/${deployment_name} to become fully ready."
  return 1
}

trap cleanup EXIT

log "Waiting for postgres rollout."
kubectl -n "${NAMESPACE}" rollout status "deployment/${DB_DEPLOYMENT}" --timeout=180s

log "Waiting for API rollout."
kubectl -n "${NAMESPACE}" rollout status "deployment/${APP_DEPLOYMENT}" --timeout=180s
wait_for_deployment_ready "${APP_DEPLOYMENT}"

log "Creating an in-cluster validation client."
kubectl -n "${NAMESPACE}" run "${CLIENT_POD}" \
  --image=busybox:1.36 \
  --restart=Never \
  --labels=app="${CLIENT_POD}" \
  --command -- sh -c "sleep 3600"
kubectl -n "${NAMESPACE}" wait --for=condition=Ready "pod/${CLIENT_POD}" --timeout=180s

initial_version="$(wait_for_service "/version" "version endpoint")"
if [[ "${initial_version}" != "${OLD_VERSION}" ]]; then
  log "Expected initial version ${OLD_VERSION}, received ${initial_version}."
  exit 1
fi

wait_for_service "/health/ready" "readiness endpoint" >/dev/null

target_pod="$(kubectl -n "${NAMESPACE}" get pods -l app="${APP_DEPLOYMENT}" -o jsonpath='{.items[0].metadata.name}')"
initial_restarts="$(kubectl -n "${NAMESPACE}" get pod "${target_pod}" -o jsonpath='{.status.containerStatuses[0].restartCount}')"

log "Triggering self-healing by stopping PID 1 in ${target_pod}."
kubectl -n "${NAMESPACE}" exec "${target_pod}" -- /bin/sh -c 'kill 1' >/dev/null 2>&1 || true

wait_for_restart "${target_pod}" "${initial_restarts}"
kubectl -n "${NAMESPACE}" wait --for=condition=Ready "pod/${target_pod}" --timeout=180s
wait_for_deployment_ready "${APP_DEPLOYMENT}"

post_heal_version="$(wait_for_service "/version" "version endpoint after self-healing")"
if [[ "${post_heal_version}" != "${OLD_VERSION}" ]]; then
  log "Expected version ${OLD_VERSION} after self-healing, received ${post_heal_version}."
  exit 1
fi

responses_file="$(mktemp)"
failures_file="$(mktemp)"

log "Triggering rolling update to ${NEW_VERSION}."
(
  for _ in $(seq 1 45); do
    if service_request "/version" >>"${responses_file}" 2>>"${failures_file}"; then
      printf '\n' >>"${responses_file}"
    else
      printf 'request_failed\n' >>"${failures_file}"
    fi
    sleep 1
  done
) &
poller_pid=$!

kubectl -n "${NAMESPACE}" set image "deployment/${APP_DEPLOYMENT}" "${APP_DEPLOYMENT}=sharpchess-api:${NEW_VERSION}"
kubectl -n "${NAMESPACE}" rollout status "deployment/${APP_DEPLOYMENT}" --timeout=180s
wait_for_deployment_ready "${APP_DEPLOYMENT}"

sleep 5
wait "${poller_pid}"

if [[ -s "${failures_file}" ]]; then
  log "Detected request failures during the rolling update:"
  cat "${failures_file}"
  exit 1
fi

if ! grep -qx "${OLD_VERSION}" "${responses_file}"; then
  log "Did not observe ${OLD_VERSION} responses before the rollout completed."
  exit 1
fi

if ! grep -qx "${NEW_VERSION}" "${responses_file}"; then
  log "Did not observe ${NEW_VERSION} responses after the rollout completed."
  exit 1
fi

final_version="$(wait_for_service "/version" "version endpoint after rolling update")"
if [[ "${final_version}" != "${NEW_VERSION}" ]]; then
  log "Expected final version ${NEW_VERSION}, received ${final_version}."
  exit 1
fi

log "Kubernetes validation completed successfully."
