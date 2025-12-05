#!/usr/bin/env bash
set -euo pipefail

# FRP-05: k6 sustained gate (10k msg/s for 15m) helper
# Usage:
#   BASE_URL=https://staging.example.com \
#   SITE_ID=<guid> EQUIPMENT_ID=<guid> \
#   ./scripts/load/run-k6-sustained.sh

if ! command -v k6 >/dev/null 2>&1; then
  echo "k6 not found. Install k6 and retry." >&2
  exit 1
fi

BASE_URL="${BASE_URL:-}"
SITE_ID="${SITE_ID:-}"
EQUIPMENT_ID="${EQUIPMENT_ID:-}"
VUS="${VUS:-800}"
DURATION="${DURATION:-15m}"
RAMP_UP="${RAMP_UP:-2m}"
BATCH_SIZE="${BATCH_SIZE:-500}"

if [[ -z "$BASE_URL" || -z "$SITE_ID" || -z "$EQUIPMENT_ID" ]]; then
  echo "Required env vars: BASE_URL, SITE_ID, EQUIPMENT_ID" >&2
  exit 1
fi

mkdir -p logs
STAMP=$(date +%Y%m%d_%H%M%S)
OUT_JSON="logs/k6-sustained-${STAMP}.json"

echo "Running k6 sustained: ${VUS} VUs for ${DURATION} (ramp ${RAMP_UP}) -> ${OUT_JSON}"

K6_SUMMARY_EXPORT="$OUT_JSON" \
  k6 run --vus "$VUS" --duration "$DURATION" \
  -e BASE_URL="$BASE_URL" -e SITE_ID="$SITE_ID" -e EQUIPMENT_ID="$EQUIPMENT_ID" \
  -e BATCH_SIZE="$BATCH_SIZE" -e RAMP_UP="$RAMP_UP" \
  tests/load/telemetry-ingest-load.js

echo "Sustained test complete. Results JSON: ${OUT_JSON}"
echo "Targets: ingest p95 < 1000 ms; error < 1%; realtime p95 < 1.5 s; rollup freshness < 60 s."

