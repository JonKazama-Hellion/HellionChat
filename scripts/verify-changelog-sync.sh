#!/usr/bin/env bash
# verify-changelog-sync.sh — Block C.
# Strips .0 suffix from repo.json AssemblyVersion to derive the 3-digit tag/version.
# yaml.changelog is a single multi-line block with **Hellion Chat X.Y.Z** subblocks.

set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

YAML="$ROOT/HellionChat/HellionChat.yaml"
REPO_JSON="$ROOT/repo.json"
FORGE_DIR="$ROOT/.github/forge-posts"

fail() { echo "verify-changelog-sync: FAIL — $1" >&2; exit 1; }
ok()   { echo "verify-changelog-sync: OK — $1"; }

VER="$(jq -r '.[0].AssemblyVersion' "$REPO_JSON" | sed -E 's/\.0$//')"
TAG="v$VER"

grep -qE "^[[:space:]]*\*\*Hellion Chat ${VER}" "$YAML" \
  || fail "$YAML changelog missing **Hellion Chat ${VER}** subblock. Fix: add the v${VER} block at the top of the changelog field."

jq -r '.[0].Changelog' "$REPO_JSON" | grep -qE "^[[:space:]]*\*\*Hellion Chat ${VER}" \
  || fail "$REPO_JSON Changelog missing **Hellion Chat ${VER}** subblock. Fix: copy the yaml changelog over."

FORGE_FILE="$FORGE_DIR/${TAG}.md"
[ -f "$FORGE_FILE" ] || fail "$FORGE_FILE missing. Fix: create from previous tag's file as template."

SUBTITLE="$(awk '/^---$/{f=!f; next} f && /^subtitle:/' "$FORGE_FILE" | sed -E 's/^subtitle:[[:space:]]*//' | tr -d '\"')"
[ -n "$SUBTITLE" ] || fail "$FORGE_FILE frontmatter missing 'subtitle'."
[ "${#SUBTITLE}" -le 60 ] || fail "$FORGE_FILE subtitle is ${#SUBTITLE} chars (max 60)."

NATUR="$(awk '/^---$/{f=!f; next} f && /^versionsnatur:/' "$FORGE_FILE" | sed -E 's/^versionsnatur:[[:space:]]*//' | tr -d '\"')"
[ -n "$NATUR" ] || fail "$FORGE_FILE frontmatter missing 'versionsnatur'."
[ "${#NATUR}" -le 40 ] || fail "$FORGE_FILE versionsnatur is ${#NATUR} chars (max 40)."

BODY="$(awk '/^---$/{f++; next} f==2' "$FORGE_FILE")"
TITLE_LEN=$((${#VER} + 16 + ${#SUBTITLE}))
FOOTER_LEN=80
TOTAL=$((TITLE_LEN + ${#BODY} + FOOTER_LEN))
[ "$TOTAL" -le 5500 ] || fail "Forge embed total ~${TOTAL} chars > 5500 cap."

YAML_VERSIONS="$(grep -cE '^[[:space:]]*\*\*Hellion Chat' "$YAML" || true)"
[ "$YAML_VERSIONS" -le 4 ] || fail "$YAML changelog has $YAML_VERSIONS version subblocks (max 4). Fix: move oldest to docs/CHANGELOG.md."

ok "yaml/repo.json/forge-post in sync for $TAG, embed-total ~${TOTAL}/5500, $YAML_VERSIONS/4 subblocks"
