#!/usr/bin/env bash
# preflight.sh — pre-push gate. Blocks A/B/C verify config drift; Block D is a
# headless `dotnet build` to catch compile-time API drift. Test execution lives
# in the local Build-Suite repo and is NOT part of this preflight.

set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

echo "==> preflight: Block A — version consistency"
./scripts/verify-version-consistency.sh

echo "==> preflight: Block B — manifest shape"
./scripts/verify-manifest-shape.sh

echo "==> preflight: Block C — changelog sync"
./scripts/verify-changelog-sync.sh

echo "==> preflight: Block D — plugin compile health"
dotnet build HellionChat/HellionChat.csproj --configuration Release --nologo --verbosity quiet

echo "==> preflight: ALL GREEN"
