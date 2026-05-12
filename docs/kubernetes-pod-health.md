---
title: Kubernetes Pod Health
---

# Kubernetes Pod Health

This page shows the last committed snapshot of the SharpChess Kubernetes workload. It is generated locally with `kubectl` and then published through DocFX, so the page does not expose direct cluster access, kubeconfig content, or Kubernetes credentials.

_Generated at 2026-05-12 20:21 UTC._

- Namespace scope: `default`
- App label selector: `app=sharpchess-api`

> This page shows a generated snapshot. It is not direct unrestricted cluster access, and it does not expose kubeconfig or cluster credentials.

## Pod Snapshot

| Namespace | Deployment | Pod | Ready | Status | Restarts | Age | Node |
| --- | --- | --- | --- | --- | --- | --- | --- |
| default | sharpchess-api | sharpchess-api-76d5888f8-2cdcn | 1/1 (True) | Running | 0 | 25d 11h | k3d-sharpchess-cluster-server-0 |
| default | sharpchess-api | sharpchess-api-76d5888f8-w6965 | 1/1 (True) | Running | 0 | 25d 11h | k3d-sharpchess-cluster-server-0 |

## Deployment Snapshot

| Namespace | Deployment | Ready | Updated | Available | Age |
| --- | --- | --- | --- | --- | --- |
| - | - | No matching deployments found | - | - | - |

## Recent Warning Events

No warning events were returned for the selected scope.

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

```bash
K8S_NAMESPACE=default APP_LABEL=sharpchess-api bash scripts/generate-kubernetes-pod-health.sh
```

The script uses `KUBECONFIG` if it is set, or falls back to `~/.kube/config` on your machine.

## Why This Page Exists

This page gives deployment monitoring evidence without giving the static DocFX site direct cluster access. The snapshot is generated on a machine that can reach the cluster, committed to the repository, and then published with the rest of the documentation.

## Troubleshooting

- **Pending**: check node capacity, image pull access, PVC binding, and whether the namespace exists.
- **ImagePullBackOff**: check the image name, tag, registry access, and any pull secrets.
- **CrashLoopBackOff**: check container logs, startup configuration, environment variables, and database connectivity.
- **NotReady**: check readiness probes, dependency health, migrations, and whether the service is listening on the expected port.
