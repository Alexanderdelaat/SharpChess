#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
output_path="$repo_root/docs/kubernetes-pod-health.md"
generated_at="$(date -u '+%Y-%m-%d %H:%M UTC')"
namespace="${K8S_NAMESPACE:-${K3S_NAMESPACE:-default}}"
app_label_input="${APP_LABEL:-${K3S_LABEL_SELECTOR:-}}"
kubectl_bin="${KUBECTL_BIN:-kubectl}"
warning_limit="${K8S_WARNING_EVENT_LIMIT:-10}"

normalize_selector() {
  local value="$1"

  if [[ -z "$value" ]]; then
    printf '%s' ""
    return
  fi

  if [[ "$value" == *"="* ]]; then
    printf '%s' "$value"
    return
  fi

  printf 'app=%s' "$value"
}

label_selector="$(normalize_selector "$app_label_input")"

scope_args=()
scope_label='all namespaces'
if [[ "$namespace" == "all" ]]; then
  scope_args=(-A)
else
  scope_args=(-n "$namespace")
  scope_label="\`$namespace\`"
fi

selector_label='not set'
if [[ -n "$label_selector" ]]; then
  selector_label="\`$label_selector\`"
fi

escape_pipes() {
  printf '%s' "$1" | sed 's/|/\\|/g'
}

timestamp_to_epoch() {
  local value="$1"
  local normalized="$value"

  if [[ "$normalized" == *.*Z ]]; then
    normalized="${normalized%%.*}Z"
  fi

  if date -u -d "$normalized" '+%s' >/dev/null 2>&1; then
    date -u -d "$normalized" '+%s'
    return
  fi

  date -ju -f '%Y-%m-%dT%H:%M:%SZ' "$normalized" '+%s'
}

format_age() {
  local timestamp="$1"
  local now_epoch created_epoch diff days hours minutes

  if [[ -z "$timestamp" ]]; then
    printf '%s' '-'
    return
  fi

  now_epoch="$(date -u '+%s')"
  created_epoch="$(timestamp_to_epoch "$timestamp")"
  diff=$(( now_epoch - created_epoch ))

  if (( diff < 0 )); then
    diff=0
  fi

  days=$(( diff / 86400 ))
  hours=$(( (diff % 86400) / 3600 ))
  minutes=$(( (diff % 3600) / 60 ))

  if (( days > 0 )); then
    printf '%sd %sh' "$days" "$hours"
    return
  fi

  if (( hours > 0 )); then
    printf '%sh %sm' "$hours" "$minutes"
    return
  fi

  printf '%sm' "$minutes"
}

write_page() {
  local availability_message="$1"
  local pod_rows="$2"
  local deployment_rows="$3"
  local warning_lines="$4"
  local local_regen_command

  local_regen_command="K8S_NAMESPACE=$namespace"
  if [[ -n "$app_label_input" ]]; then
    local_regen_command="$local_regen_command APP_LABEL=$app_label_input"
  fi
  local_regen_command="$local_regen_command bash scripts/generate-kubernetes-pod-health.sh"

  cat >"$output_path" <<EOF
---
title: Kubernetes Pod Health
---

# Kubernetes Pod Health

This page shows the last committed snapshot of the SharpChess Kubernetes workload. It is generated locally with \`kubectl\` and then published through DocFX, so the page does not expose direct cluster access, kubeconfig content, or Kubernetes credentials.

_Generated at $generated_at._

- Namespace scope: $scope_label
- App label selector: $selector_label

$availability_message

## Pod Snapshot

| Namespace | Deployment | Pod | Ready | Status | Restarts | Age | Node |
| --- | --- | --- | --- | --- | --- | --- | --- |
$pod_rows

## Deployment Snapshot

| Namespace | Deployment | Ready | Updated | Available | Age |
| --- | --- | --- | --- | --- | --- |
$deployment_rows

## Recent Warning Events

$warning_lines

## What This Means

### What is a Kubernetes Pod?

A Pod is the smallest deployable unit in Kubernetes. It contains one or more containers that run together on the same node and share networking and storage context.

### Why readiness matters

Readiness tells Kubernetes whether a Pod is ready to receive traffic. A Pod can be running but still not be ready. For a deployment, that usually means the application started but is not healthy enough yet to serve requests.

### Basic state meanings

- **Running** means the Pod has been scheduled and the containers are running.
- **Pending** means the Pod has not started properly yet, often because of scheduling, image pull, or storage issues.
- **CrashLoopBackOff** means a container keeps starting and crashing.
- **Ready** means the Pod passed its readiness checks and should be able to handle traffic.

## Regenerate Locally

Run the snapshot script from the repository root:

\`\`\`bash
$local_regen_command
\`\`\`

The script uses \`KUBECONFIG\` if it is set, or falls back to \`~/.kube/config\` on your machine.

## Why This Page Exists

This page gives deployment monitoring evidence without giving the static DocFX site direct cluster access. The snapshot is generated on a machine that can reach the cluster, committed to the repository, and then published with the rest of the documentation.

## Troubleshooting

- **Pending**: check node capacity, image pull access, PVC binding, and whether the namespace exists.
- **ImagePullBackOff**: check the image name, tag, registry access, and any pull secrets.
- **CrashLoopBackOff**: check container logs, startup configuration, environment variables, and database connectivity.
- **NotReady**: check readiness probes, dependency health, migrations, and whether the service is listening on the expected port.
EOF
}

write_fallback_page() {
  local availability_message="$1"

  write_page \
    "$availability_message" \
    '| - | - | - | - | Snapshot unavailable | - | - | - |' \
    '| - | - | Snapshot unavailable | - | - | - |' \
    'No warning event snapshot was available.'
}

if ! command -v "$kubectl_bin" >/dev/null 2>&1; then
  write_fallback_page '> Cluster access was not available when this page was generated, so no live pod snapshot could be collected.'
  exit 0
fi

if [[ -n "${KUBECONFIG:-}" ]]; then
  if [[ ! -f "$KUBECONFIG" ]]; then
    write_fallback_page '> Cluster access was not available when this page was generated, so no live pod snapshot could be collected.'
    exit 0
  fi
elif [[ ! -f "$HOME/.kube/config" ]]; then
  write_fallback_page '> Cluster access was not available when this page was generated, so no live pod snapshot could be collected.'
  exit 0
fi

replicaset_jsonpath='{range .items[*]}{.metadata.namespace}{"\t"}{.metadata.name}{"\t"}{.metadata.ownerReferences[0].kind}{"\t"}{.metadata.ownerReferences[0].name}{"\n"}{end}'
pod_jsonpath='{range .items[*]}{.metadata.namespace}{"\t"}{.metadata.name}{"\t"}{.status.phase}{"\t"}{.spec.nodeName}{"\t"}{.metadata.creationTimestamp}{"\t"}{.metadata.ownerReferences[0].kind}{"\t"}{.metadata.ownerReferences[0].name}{"\t"}{range .status.containerStatuses[*]}{.ready}{","}{end}{"\t"}{range .status.containerStatuses[*]}{.restartCount}{","}{end}{"\t"}{range .status.containerStatuses[*]}{.state.waiting.reason}{","}{end}{"\t"}{range .status.conditions[?(@.type=="Ready")]}{.status}{end}{"\n"}{end}'
deployment_jsonpath='{range .items[*]}{.metadata.namespace}{"\t"}{.metadata.name}{"\t"}{.spec.replicas}{"\t"}{.status.readyReplicas}{"\t"}{.status.updatedReplicas}{"\t"}{.status.availableReplicas}{"\t"}{.metadata.creationTimestamp}{"\n"}{end}'
event_jsonpath='{range .items[*]}{.lastTimestamp}{"\t"}{.involvedObject.kind}{"\t"}{.involvedObject.name}{"\t"}{.reason}{"\t"}{.message}{"\n"}{end}'

if [[ -n "$label_selector" ]]; then
  if ! replicaset_lines="$("$kubectl_bin" get rs "${scope_args[@]}" -l "$label_selector" -o "jsonpath=${replicaset_jsonpath}" 2>/dev/null || true)"; then
    replicaset_lines=""
  fi

  if ! pod_lines="$("$kubectl_bin" get pods "${scope_args[@]}" -l "$label_selector" -o "jsonpath=${pod_jsonpath}" 2>/dev/null)"; then
    write_fallback_page '> Cluster access was configured, but the snapshot could not be collected during generation.'
    exit 0
  fi

  if ! deployment_lines="$("$kubectl_bin" get deployments "${scope_args[@]}" -l "$label_selector" -o "jsonpath=${deployment_jsonpath}" 2>/dev/null || true)"; then
    deployment_lines=""
  fi
else
  if ! replicaset_lines="$("$kubectl_bin" get rs "${scope_args[@]}" -o "jsonpath=${replicaset_jsonpath}" 2>/dev/null || true)"; then
    replicaset_lines=""
  fi

  if ! pod_lines="$("$kubectl_bin" get pods "${scope_args[@]}" -o "jsonpath=${pod_jsonpath}" 2>/dev/null)"; then
    write_fallback_page '> Cluster access was configured, but the snapshot could not be collected during generation.'
    exit 0
  fi

  if ! deployment_lines="$("$kubectl_bin" get deployments "${scope_args[@]}" -o "jsonpath=${deployment_jsonpath}" 2>/dev/null || true)"; then
    deployment_lines=""
  fi
fi

if ! warning_event_lines="$("$kubectl_bin" get events "${scope_args[@]}" --field-selector type=Warning --sort-by=.lastTimestamp -o "jsonpath=${event_jsonpath}" 2>/dev/null || true)"; then
  warning_event_lines=""
fi

resolve_deployment_name() {
  local pod_namespace="$1"
  local owner_kind="$2"
  local owner_name="$3"
  local resolved_name

  case "$owner_kind" in
    Deployment|StatefulSet|DaemonSet|Job)
      if [[ -n "$owner_name" ]]; then
        printf '%s' "$owner_name"
      else
        printf '%s' '-'
      fi
      return
      ;;
    ReplicaSet)
      resolved_name="$(
        printf '%s\n' "$replicaset_lines" |
          awk -F '\t' -v namespace="$pod_namespace" -v replica_set="$owner_name" '
            $1 == namespace && $2 == replica_set && $3 == "Deployment" { print $4; exit }
          '
      )"
      if [[ -n "$resolved_name" ]]; then
        printf '%s' "$resolved_name"
      elif [[ -n "$owner_name" ]]; then
        printf '%s' "$owner_name"
      else
        printf '%s' '-'
      fi
      return
      ;;
  esac

  printf '%s' '-'
}

pod_rows=""
while IFS=$'\t' read -r pod_namespace pod_name phase node_name created_at owner_kind owner_name ready_values restart_values waiting_reasons ready_condition; do
  [[ -z "$pod_name" ]] && continue

  ready_count=0
  container_count=0
  IFS=',' read -r -a ready_parts <<< "$ready_values"
  for value in "${ready_parts[@]}"; do
    [[ -z "$value" ]] && continue
    container_count=$(( container_count + 1 ))
    if [[ "$value" == "true" ]]; then
      ready_count=$(( ready_count + 1 ))
    fi
  done
  ready_display="$ready_count/$container_count"
  if [[ -z "$ready_condition" ]]; then
    ready_condition="False"
  fi
  ready_display="${ready_display} (${ready_condition})"

  restart_total=0
  IFS=',' read -r -a restart_parts <<< "$restart_values"
  for value in "${restart_parts[@]}"; do
    [[ -z "$value" ]] && continue
    restart_total=$(( restart_total + value ))
  done

  status_display="$phase"
  IFS=',' read -r -a waiting_parts <<< "$waiting_reasons"
  for reason in "${waiting_parts[@]}"; do
    [[ -z "$reason" ]] && continue
    status_display="$reason"
    break
  done

  deployment_name="$(resolve_deployment_name "$pod_namespace" "$owner_kind" "$owner_name")"

  age_display="$(format_age "$created_at")"
  [[ -z "$node_name" ]] && node_name='-'

  pod_rows+=$'| '"$(escape_pipes "$pod_namespace")"$' | '"$(escape_pipes "$deployment_name")"$' | '"$(escape_pipes "$pod_name")"$' | '"$(escape_pipes "$ready_display")"$' | '"$(escape_pipes "$status_display")"$' | '"$(escape_pipes "$restart_total")"$' | '"$(escape_pipes "$age_display")"$' | '"$(escape_pipes "$node_name")"$' |\n'
done <<< "$pod_lines"

if [[ -z "$pod_rows" ]]; then
  pod_rows='| - | - | - | - | No matching pods found | - | - | - |'
fi

deployment_rows=""
while IFS=$'\t' read -r deployment_namespace deployment_name desired_replicas ready_replicas updated_replicas available_replicas created_at; do
  [[ -z "$deployment_name" ]] && continue

  [[ -z "$desired_replicas" ]] && desired_replicas='0'
  [[ -z "$ready_replicas" ]] && ready_replicas='0'
  [[ -z "$updated_replicas" ]] && updated_replicas='0'
  [[ -z "$available_replicas" ]] && available_replicas='0'

  age_display="$(format_age "$created_at")"
  ready_display="${ready_replicas}/${desired_replicas}"

  deployment_rows+=$'| '"$(escape_pipes "$deployment_namespace")"$' | '"$(escape_pipes "$deployment_name")"$' | '"$(escape_pipes "$ready_display")"$' | '"$(escape_pipes "$updated_replicas")"$' | '"$(escape_pipes "$available_replicas")"$' | '"$(escape_pipes "$age_display")"$' |\n'
done <<< "$deployment_lines"

if [[ -z "$deployment_rows" ]]; then
  deployment_rows='| - | - | No matching deployments found | - | - | - |'
fi

warning_lines='No warning events were returned for the selected scope.'
if [[ -n "$warning_event_lines" ]]; then
  recent_warning_lines="$(printf '%s\n' "$warning_event_lines" | tail -n "$warning_limit")"
  if [[ -n "$recent_warning_lines" ]]; then
    warning_lines=$'```text\n'"$recent_warning_lines"$'\n```'
  fi
fi

write_page \
  '> This page shows a generated snapshot. It is not direct unrestricted cluster access, and it does not expose kubeconfig or cluster credentials.' \
  "${pod_rows%$'\n'}" \
  "${deployment_rows%$'\n'}" \
  "$warning_lines"
