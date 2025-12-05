#!/usr/bin/env bash
set -euo pipefail

# Summarize k6 JSON results and optionally append to FRP05 results doc.
# Usage:
#   ./scripts/load/summarize-k6-results.sh logs/k6-baseline-*.json [--append]

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <k6-summary.json> [--append]" >&2
  exit 1
fi

JSON_PATH="$1"
APPEND="${2:-}"

if ! command -v jq >/dev/null 2>&1; then
  echo "jq is required to summarize k6 results. Install jq and retry." >&2
  exit 2
fi

if [[ ! -f "$JSON_PATH" ]]; then
  echo "File not found: $JSON_PATH" >&2
  exit 3
fi

NAME=$(jq -r '.root_group.name // "k6"' "$JSON_PATH")
START=$(jq -r '.state.testRunDurationMs // 0' "$JSON_PATH" 2>/dev/null || echo 0)
P95=$(jq -r '.metrics.http_req_duration["p(95)"] // .metrics.http_req_duration.percentiles["p(95)"] // empty' "$JSON_PATH")
P99=$(jq -r '.metrics.http_req_duration["p(99)"] // .metrics.http_req_duration.percentiles["p(99)"] // empty' "$JSON_PATH")
AVG=$(jq -r '.metrics.http_req_duration.avg // empty' "$JSON_PATH")
REQS=$(jq -r '.metrics.http_reqs.count // empty' "$JSON_PATH")
RATE=$(jq -r '.metrics.http_reqs.rate // empty' "$JSON_PATH")
ERR_RATE=$(jq -r '.metrics.http_req_failed.rate // empty' "$JSON_PATH")
DATA_SENT=$(jq -r '.metrics.data_sent.sum // 0' "$JSON_PATH")
DATA_RECV=$(jq -r '.metrics.data_received.sum // 0' "$JSON_PATH")

echo "k6 Summary: $JSON_PATH"
echo "- test:     $NAME"
echo "- p95:      ${P95:-n/a} ms"
echo "- p99:      ${P99:-n/a} ms"
echo "- avg:      ${AVG:-n/a} ms"
echo "- reqs:     ${REQS:-n/a} (rate=${RATE:-n/a}/s)"
echo "- err rate: ${ERR_RATE:-n/a}"
echo "- data:     sent=${DATA_SENT}B recv=${DATA_RECV}B"

if [[ "$APPEND" == "--append" ]]; then
  DOC="docs/FRP05_DAY_ZERO_RESULTS.md"
  TS=$(date +"%Y-%m-%d %H:%M:%S %Z")
  {
    echo "\n---\n"
    echo "### k6 Summary (${TS})"
    echo "File: \\`$JSON_PATH\\`"
    echo ""
    echo "- p95: ${P95:-n/a} ms"
    echo "- p99: ${P99:-n/a} ms"
    echo "- avg: ${AVG:-n/a} ms"
    echo "- requests: ${REQS:-n/a} (rate ${RATE:-n/a}/s)"
    echo "- error rate: ${ERR_RATE:-n/a}"
    echo "- data: sent=${DATA_SENT}B, received=${DATA_RECV}B"
  } >> "$DOC"
  echo "Appended summary to $DOC"
fi

