#!/usr/bin/env bash
# verify-version-consistency.sh — Block A of preflight.
# csproj <Version> is 3-digit SemVer; repo.json AssemblyVersion is 4-digit (.0 suffix).

set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

CSPROJ="$ROOT/HellionChat/HellionChat.csproj"
REPO_JSON="$ROOT/repo.json"

fail() { echo "verify-version-consistency: FAIL — $1" >&2; exit 1; }
ok()   { echo "verify-version-consistency: OK — $1"; }

CSPROJ_VER="$(grep -oE '<Version>[^<]+</Version>' "$CSPROJ" | head -1 | sed -E 's/<[^>]+>//g')"
[ -n "$CSPROJ_VER" ] || fail "$CSPROJ has no <Version> element"

EXPECTED_4DIGIT="${CSPROJ_VER}.0"

REPO_VER="$(jq -r '.[0].AssemblyVersion' "$REPO_JSON")"
[ "$REPO_VER" = "$EXPECTED_4DIGIT" ] \
  || fail "csproj=$CSPROJ_VER expects repo.json AssemblyVersion=$EXPECTED_4DIGIT but got $REPO_VER. Fix: align in $REPO_JSON."

TEST_VER="$(jq -r '.[0].TestingAssemblyVersion' "$REPO_JSON")"
[ "$TEST_VER" = "$EXPECTED_4DIGIT" ] \
  || fail "TestingAssemblyVersion=$TEST_VER must match $EXPECTED_4DIGIT. Fix: align in $REPO_JSON."

TAG="v$CSPROJ_VER"
for KEY in DownloadLinkInstall DownloadLinkUpdate DownloadLinkTesting; do
  URL="$(jq -r ".[0].$KEY" "$REPO_JSON")"
  case "$URL" in
    *"/$TAG/"*) ;;
    *) fail "$KEY=$URL does not contain tag $TAG. Fix: update $REPO_JSON $KEY to releases/download/$TAG/latest.zip." ;;
  esac
done

ok "csproj=$CSPROJ_VER, repo.json=$EXPECTED_4DIGIT, tag $TAG present in DownloadLinks"
