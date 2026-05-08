#!/usr/bin/env bash
# setup-hooks.sh — installs pre-push hook via core.hooksPath. Idempotent.
# Note: NO pre-commit hook — test execution is local-only in the Build-Suite repo,
# so the plugin repo's pre-commit cannot run tests. Versions/manifest/changelog
# checks happen on pre-push only.

set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

git config core.hooksPath .githooks
chmod +x .githooks/pre-push
echo "setup-hooks: core.hooksPath set to .githooks. pre-push live."
