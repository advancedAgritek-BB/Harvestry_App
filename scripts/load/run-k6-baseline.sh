#!/usr/bin/env bash
set -euo pipefail

# FRP-05: k6 baseline load test helper
# Usage:
#   BASE_URL=https://staging.example.com \
#   SITE_ID=<guid> EQUIPMENT_ID=<guid> \
#   ./scripts/load/run-k6-baseline.sh

if ! command -v k6 >/dev/null 2>&1; then
  echo "k6 not found. Install k6 and retry." >&2
  exit 1
fi

BASE_URL="${BASE_URL:-}"
SITE_ID="${SITE_ID:-}"
EQUIPMENT_ID="${EQUIPMENT_ID:-}"
VUS="${VUS:-50}"
DURATION="${DURATION:-5m}"

if [[ -z "$BASE_URL" || -z "$SITE_ID" || -z "$EQUIPMENT_ID" ]]; then
  echo "Required env vars: BASE_URL, SITE_ID, EQUIPMENT_ID" >&2
  exit 1
fi

mkdir -p logs
STAMP=$(date +%Y%m%d_%H%M%S)
OUT_JSON="logs/k6-baseline-${STAMP}.json"

echo "Running k6 baseline: ${VUS} VUs for ${DURATION} -> ${OUT_JSON}"

K6_SUMMARY_EXPORT="$OUT_JSON" \
  k6 run --vus "$VUS" --duration "$DURATION" \
  -e BASE_URL="$BASE_URL" -e SITE_ID="$SITE_ID" -e EQUIPMENT_ID="$EQUIPMENT_ID" \
  tests/load/telemetry-ingest-load.js

echo "Baseline complete. Results JSON: ${OUT_JSON}"
echo "Tip: attach JSON to docs/FRP05_DAY_ZERO_RESULTS.md and record p95 + error rate."

