#!/bin/bash
# Harvestry ERP - Acceptance Test Harness
# Track A: Validate staging meets all SLO targets for 7 consecutive days

set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Default values
ENVIRONMENT="staging"
DURATION_DAYS=7
PROMETHEUS_URL="http://prometheus:9090"
CHECK_INTERVAL=3600  # 1 hour
RESULTS_DIR="./test-results/acceptance"

# SLO Targets
SLO_API_AVAILABILITY=0.999  # 99.9%
SLO_INGEST_P95=1000         # 1000ms
SLO_PUSH_P95=1500           # 1500ms
SLO_COMMAND_P95=800         # 800ms
SLO_TASK_P95=300            # 300ms

# Print usage
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Run 7-day acceptance test harness to validate SLO compliance.

OPTIONS:
    -e, --env ENV         Environment (staging, production) [default: staging]
    -d, --days DAYS       Duration in days [default: 7]
    -p, --prometheus URL  Prometheus URL [default: http://prometheus:9090]
    -i, --interval SECS   Check interval in seconds [default: 3600]
    -o, --output DIR      Output directory [default: ./test-results/acceptance]
    -h, --help            Show this help message

EXAMPLES:
    # Run 7-day acceptance test
    $0 --env staging

    # Run shorter 3-day test
    $0 --env staging --days 3

EOF
    exit 1
}

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--env)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -d|--days)
            DURATION_DAYS="$2"
            shift 2
            ;;
        -p|--prometheus)
            PROMETHEUS_URL="$2"
            shift 2
            ;;
        -i|--interval)
            CHECK_INTERVAL="$2"
            shift 2
            ;;
        -o|--output)
            RESULTS_DIR="$2"
            shift 2
            ;;
        -h|--help)
            usage
            ;;
        *)
            echo -e "${RED}Error: Unknown option $1${NC}"
            usage
            ;;
    esac
done

# Create results directory
mkdir -p "$RESULTS_DIR"

# Start time
START_TIME=$(date +%s)
END_TIME=$((START_TIME + (DURATION_DAYS * 86400)))

echo -e "${BLUE}======================================${NC}"
echo -e "${BLUE}Harvestry ERP - Acceptance Tests${NC}"
echo -e "${BLUE}======================================${NC}"
echo ""
echo "Environment:  $ENVIRONMENT"
echo "Duration:     ${DURATION_DAYS} days"
echo "Start:        $(date -r $START_TIME)"
echo "End:          $(date -r $END_TIME)"
echo "Check Every:  $((CHECK_INTERVAL / 60)) minutes"
echo ""
echo -e "${YELLOW}SLO Targets:${NC}"
echo "  API Availability:      ≥ $(echo "$SLO_API_AVAILABILITY * 100" | bc)%"
echo "  Ingest p95:            < ${SLO_INGEST_P95}ms"
echo "  Push p95:              < ${SLO_PUSH_P95}ms"
echo "  Command p95:           < ${SLO_COMMAND_P95}ms"
echo "  Task p95:              < ${SLO_TASK_P95}ms"
echo ""

# Initialize results file
RESULTS_FILE="${RESULTS_DIR}/acceptance_test_$(date +%Y%m%d_%H%M%S).json"
echo "{" > "$RESULTS_FILE"
echo "  \"start_time\": \"$(date -Iseconds)\"," >> "$RESULTS_FILE"
echo "  \"duration_days\": $DURATION_DAYS," >> "$RESULTS_FILE"
echo "  \"environment\": \"$ENVIRONMENT\"," >> "$RESULTS_FILE"
echo "  \"checks\": [" >> "$RESULTS_FILE"

CHECKS_FAILED=0
CHECK_COUNT=0
TOTAL_CHECKS=$((DURATION_DAYS * 86400 / CHECK_INTERVAL))

echo -e "${GREEN}Starting acceptance test monitoring...${NC}"
echo ""

# Function to query Prometheus
query_prometheus() {
    local query=$1
    curl -s "${PROMETHEUS_URL}/api/v1/query" --data-urlencode "query=${query}" | \
        jq -r '.data.result[0].value[1]' 2>/dev/null || echo "null"
}

# Function to check SLO
check_slo() {
    local name=$1
    local actual=$2
    local target=$3
    local comparison=$4  # "lt" or "gte"
    
    if [ "$actual" = "null" ] || [ -z "$actual" ]; then
        echo -e "${YELLOW}⚠${NC}  ${name}: No data"
        return 1
    fi
    
    if [ "$comparison" = "lt" ]; then
        if (( $(echo "$actual < $target" | bc -l) )); then
            echo -e "${GREEN}✓${NC}  ${name}: ${actual} < ${target}"
            return 0
        else
            echo -e "${RED}✗${NC}  ${name}: ${actual} >= ${target}"
            return 1
        fi
    else  # gte
        if (( $(echo "$actual >= $target" | bc -l) )); then
            echo -e "${GREEN}✓${NC}  ${name}: ${actual} >= ${target}"
            return 0
        else
            echo -e "${RED}✗${NC}  ${name}: ${actual} < ${target}"
            return 1
        fi
    fi
}

# Main monitoring loop
while [ $(date +%s) -lt $END_TIME ]; do
    CHECK_COUNT=$((CHECK_COUNT + 1))
    CURRENT_TIME=$(date +%s)
    ELAPSED_HOURS=$(( (CURRENT_TIME - START_TIME) / 3600 ))
    
    echo -e "${BLUE}Check $CHECK_COUNT/$TOTAL_CHECKS - Hour $ELAPSED_HOURS/${DURATION_DAYS}d${NC}"
    echo "Time: $(date)"
    echo ""
    
    CHECK_FAILED=false
    
    # Check 1: API Availability
    API_ERROR_RATE=$(query_prometheus "rate(http_request_errors_total{environment=\"${ENVIRONMENT}\"}[1h]) / rate(http_requests_total{environment=\"${ENVIRONMENT}\"}[1h])")
    if [ "$API_ERROR_RATE" != "null" ]; then
        API_AVAILABILITY=$(echo "1 - $API_ERROR_RATE" | bc -l)
    else
        API_AVAILABILITY="null"
    fi
    
    if ! check_slo "API Availability" "$API_AVAILABILITY" "$SLO_API_AVAILABILITY" "gte"; then
        CHECK_FAILED=true
    fi
    
    # Check 2: Ingest p95
    INGEST_P95=$(query_prometheus "histogram_quantile(0.95, rate(telemetry_ingest_duration_seconds_bucket{environment=\"${ENVIRONMENT}\"}[1h])) * 1000")
    if ! check_slo "Ingest p95" "$INGEST_P95" "$SLO_INGEST_P95" "lt"; then
        CHECK_FAILED=true
    fi
    
    # Check 3: Push p95
    PUSH_P95=$(query_prometheus "histogram_quantile(0.95, rate(realtime_push_duration_seconds_bucket{environment=\"${ENVIRONMENT}\"}[1h])) * 1000")
    if ! check_slo "Push p95" "$PUSH_P95" "$SLO_PUSH_P95" "lt"; then
        CHECK_FAILED=true
    fi
    
    # Check 4: Command p95
    COMMAND_P95=$(query_prometheus "histogram_quantile(0.95, rate(command_dispatch_duration_seconds_bucket{environment=\"${ENVIRONMENT}\"}[1h])) * 1000")
    if ! check_slo "Command p95" "$COMMAND_P95" "$SLO_COMMAND_P95" "lt"; then
        CHECK_FAILED=true
    fi
    
    # Check 5: Task p95
    TASK_P95=$(query_prometheus "histogram_quantile(0.95, rate(task_operation_duration_seconds_bucket{environment=\"${ENVIRONMENT}\"}[1h])) * 1000")
    if ! check_slo "Task p95" "$TASK_P95" "$SLO_TASK_P95" "lt"; then
        CHECK_FAILED=true
    fi
    
    # Record results
    cat >> "$RESULTS_FILE" << EOF
    {
      "check_number": $CHECK_COUNT,
      "timestamp": "$(date -Iseconds)",
      "elapsed_hours": $ELAPSED_HOURS,
      "api_availability": $API_AVAILABILITY,
      "ingest_p95_ms": $INGEST_P95,
      "push_p95_ms": $PUSH_P95,
      "command_p95_ms": $COMMAND_P95,
      "task_p95_ms": $TASK_P95,
      "passed": $([ "$CHECK_FAILED" = "false" ] && echo "true" || echo "false")
    }$([ $CHECK_COUNT -lt $TOTAL_CHECKS ] && echo ",")
EOF
    
    if [ "$CHECK_FAILED" = "true" ]; then
        CHECKS_FAILED=$((CHECKS_FAILED + 1))
        echo -e "${RED}Check FAILED${NC}"
    else
        echo -e "${GREEN}Check PASSED${NC}"
    fi
    
    echo ""
    
    # Progress
    PERCENT_COMPLETE=$(echo "scale=1; ($ELAPSED_HOURS / ($DURATION_DAYS * 24)) * 100" | bc)
    echo -e "${YELLOW}Progress: ${PERCENT_COMPLETE}% (${ELAPSED_HOURS}h / $((DURATION_DAYS * 24))h)${NC}"
    echo -e "${YELLOW}Failed Checks: $CHECKS_FAILED / $CHECK_COUNT${NC}"
    echo ""
    
    # Sleep until next check
    REMAINING_TIME=$((END_TIME - CURRENT_TIME))
    if [ $REMAINING_TIME -lt $CHECK_INTERVAL ]; then
        echo -e "${YELLOW}Final check completed. Waiting for test completion...${NC}"
        sleep $REMAINING_TIME
        break
    else
        echo -e "${YELLOW}Next check in $((CHECK_INTERVAL / 60)) minutes...${NC}"
        sleep $CHECK_INTERVAL
    fi
done

# Finalize results file
echo "  ]," >> "$RESULTS_FILE"
echo "  \"end_time\": \"$(date -Iseconds)\"," >> "$RESULTS_FILE"
echo "  \"total_checks\": $CHECK_COUNT," >> "$RESULTS_FILE"
echo "  \"failed_checks\": $CHECKS_FAILED," >> "$RESULTS_FILE"
echo "  \"success_rate\": $(echo "scale=4; (($CHECK_COUNT - $CHECKS_FAILED) / $CHECK_COUNT) * 100" | bc)" >> "$RESULTS_FILE"
echo "}" >> "$RESULTS_FILE"

# Final summary
echo -e "${BLUE}======================================${NC}"
echo -e "${BLUE}Acceptance Test Complete${NC}"
echo -e "${BLUE}======================================${NC}"
echo ""
echo "Duration:       ${DURATION_DAYS} days"
echo "Total Checks:   $CHECK_COUNT"
echo "Failed Checks:  $CHECKS_FAILED"
echo "Success Rate:   $(echo "scale=2; (($CHECK_COUNT - $CHECKS_FAILED) / $CHECK_COUNT) * 100" | bc)%"
echo ""
echo "Results saved to: $RESULTS_FILE"
echo ""

# Final verdict
if [ $CHECKS_FAILED -eq 0 ]; then
    echo -e "${GREEN}✓ ACCEPTANCE TEST PASSED${NC}"
    echo ""
    echo "All SLO targets met for ${DURATION_DAYS} consecutive days."
    echo "Environment is ready for production release."
    echo ""
    exit 0
else
    echo -e "${RED}✗ ACCEPTANCE TEST FAILED${NC}"
    echo ""
    echo "$CHECKS_FAILED out of $CHECK_COUNT checks failed."
    echo "SLO targets not consistently met."
    echo ""
    echo -e "${YELLOW}Recommended Actions:${NC}"
    echo "  1. Review Grafana dashboards for degradation patterns"
    echo "  2. Check Sentry for error spikes"
    echo "  3. Analyze failed check timestamps"
    echo "  4. Run load tests to identify bottlenecks"
    echo "  5. Extend acceptance test period after fixes"
    echo ""
    exit 1
fi
