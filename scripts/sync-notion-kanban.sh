#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

dotnet run \
  --project "$repo_root/tools/SharpChess.NotionKanbanSync/SharpChess.NotionKanbanSync.csproj" \
  -- \
  --output "$repo_root/docs/kanban.md"
