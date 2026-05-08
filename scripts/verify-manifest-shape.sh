#!/usr/bin/env bash
# verify-manifest-shape.sh — Block B. Required fields in lowercase yaml,
# DalamudApiLevel sanity in repo.json, no own DalamudPackager.targets,
# Icon AND every ImageUrl reachable.

set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

YAML="$ROOT/HellionChat/HellionChat.yaml"
REPO_JSON="$ROOT/repo.json"
TARGETS_OWN="$ROOT/HellionChat/DalamudPackager.targets"

fail() { echo "verify-manifest-shape: FAIL — $1" >&2; exit 1; }
ok()   { echo "verify-manifest-shape: OK — $1"; }

for FIELD in name author punchline description repo_url icon_url image_urls tags changelog; do
  grep -qE "^${FIELD}:" "$YAML" || fail "$YAML missing required field: $FIELD"
done

[ -f "$TARGETS_OWN" ] && fail "Own DalamudPackager.targets at $TARGETS_OWN strips Icon/ImageUrls. DELETE it; SDK default works."

API_LEVEL="$(jq -r '.[0].DalamudApiLevel' "$REPO_JSON")"
case "$API_LEVEL" in
  ''|null) fail "$REPO_JSON missing DalamudApiLevel" ;;
esac
[[ "$API_LEVEL" =~ ^[0-9]+$ ]] || fail "$REPO_JSON DalamudApiLevel must be integer, got: $API_LEVEL"
[ "$API_LEVEL" -ge 12 ] || fail "$REPO_JSON DalamudApiLevel=$API_LEVEL is below SDK 15 floor (12)."

if [ "${HOOKS_OFFLINE:-0}" != "1" ]; then
  ICON_URL="$(jq -r '.[0].IconUrl' "$REPO_JSON")"
  curl -fsI "$ICON_URL" > /dev/null || fail "IconUrl unreachable: $ICON_URL"
  ok "IconUrl reachable: $ICON_URL"

  COUNT="$(jq -r '.[0].ImageUrls | length' "$REPO_JSON")"
  [ "$COUNT" -ge 1 ] || fail "repo.json ImageUrls is empty"
  for i in $(seq 0 $((COUNT - 1))); do
    URL="$(jq -r ".[0].ImageUrls[$i]" "$REPO_JSON")"
    curl -fsI "$URL" > /dev/null || fail "ImageUrls[$i] unreachable: $URL"
  done
  ok "$COUNT ImageUrl(s) reachable, DalamudApiLevel=$API_LEVEL"
else
  ok "skipped URL reachability (HOOKS_OFFLINE=1), DalamudApiLevel=$API_LEVEL"
fi
