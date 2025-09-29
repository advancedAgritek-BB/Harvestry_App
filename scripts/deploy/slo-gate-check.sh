#!/bin/bash
# Harvestry ERP - SLO Gate Check
# Track A: Validate error budget before deployment

set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Default values
ENVIRONMENT="staging"
SERVICE=""
SLO_TARGET=0.999  # 99.9% availability
PROMETHEUS_URL="http://localhost:9090"
LOOKBACK_HOURS=1
ERROR_BUDGET_THRESHOLD=20  # % of error budget remaining to allow deploy

# Print usage
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Check SLO compliance and error budget before deployment.

OPTIONS:
    -e, --env ENV          Environment (staging, production) [default: staging]
    -s, --service SERVICE  Service name (required)
    -p, --prometheus URL   Prometheus URL [default: http://localhost:9090]
    -t, --threshold PCT    Min error budget % to allow deploy [default: 20]
    -l, --lookback HOURS   Hours to look back [default: 1]
    -h, --help             Show this help message

EXAMPLES:
    # Check before staging deploy
    $0 --env staging --service api-gateway

    # Check with custom threshold
    $0 --env production --service api-gateway --threshold 30

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
        -s|--service)
            SERVICE="$2"
            shift 2
            ;;
        -p|--prometheus)
            PROMETHEUS_URL="$2"
            shift 2
            ;;
        -t|--threshold)
            ERROR_BUDGET_THRESHOLD="$2"
            shift 2
            ;;
        -l|--lookback)
            LOOKBACK_HOURS="$2"
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

# Validate required parameters
if [ -z "$SERVICE" ]; then
    echo -e "${RED}Error: Service is required${NC}"
    usage
fi

echo -e "${YELLOW}======================================${NC}"
echo -e "${YELLOW}Harvestry ERP - SLO Gate Check${NC}"
echo -e "${YELLOW}======================================${NC}"
echo ""
echo "Environment:     $ENVIRONMENT"
echo "Service:         $SERVICE"
echo "SLO Target:      $(echo "$SLO_TARGET * 100" | bc)%"
echo "Lookback:        ${LOOKBACK_HOURS}h"
echo "Threshold:       ${ERROR_BUDGET_THRESHOLD}%"
echo ""

# Query Prometheus for error rate
echo -e "${YELLOW}Querying Prometheus for error rate...${NC}"

QUERY="rate(http_request_errors_total{service=\"${SERVICE}\",environment=\"${ENVIRONMENT}\"}[${LOOKBACK_HOURS}h]) / rate(http_requests_total{service=\"${SERVICE}\",environment=\"${ENVIRONMENT}\"}[${LOOKBACK_HOURS}h])"

RESPONSE=$(curl -s "${PROMETHEUS_URL}/api/v1/query" --data-urlencode "query=${QUERY}")

# Extract error rate from response
ERROR_RATE=$(echo "$RESPONSE" | jq -r '.data.result[0].value[1]' 2>/dev/null || echo "null")

if [ "$ERROR_RATE" = "null" ] || [ -z "$ERROR_RATE" ]; then
    echo -e "${RED}✗ Failed to query Prometheus or no data available${NC}"
    echo ""
    echo "Response: $RESPONSE"
    echo ""
    echo -e "${YELLOW}Possible issues:${NC}"
    echo "  1. Prometheus not accessible at $PROMETHEUS_URL"
    echo "  2. No metrics for service=$SERVICE, environment=$ENVIRONMENT"
    echo "  3. Time range ${LOOKBACK_HOURS}h has no data"
    echo ""
    exit 1
fi

# Calculate availability and error budget
AVAILABILITY=$(echo "1 - $ERROR_RATE" | bc -l)
AVAILABILITY_PCT=$(echo "$AVAILABILITY * 100" | bc -l)

ERROR_BUDGET=$(echo "1 - $SLO_TARGET" | bc -l)
ERROR_BUDGET_CONSUMED=$(echo "$ERROR_RATE / $ERROR_BUDGET * 100" | bc -l)
ERROR_BUDGET_REMAINING=$(echo "100 - $ERROR_BUDGET_CONSUMED" | bc -l)

echo ""
echo -e "${YELLOW}=== SLO Metrics ===${NC}"
printf "Error Rate:              %.4f%%\n" "$(echo "$ERROR_RATE * 100" | bc -l)"
printf "Availability:            %.4f%%\n" "$AVAILABILITY_PCT"
printf "Error Budget Consumed:   %.2f%%\n" "$ERROR_BUDGET_CONSUMED"
printf "Error Budget Remaining:  %.2f%%\n" "$ERROR_BUDGET_REMAINING"
echo ""

# Check if SLO is being met
if (( $(echo "$AVAILABILITY < $SLO_TARGET" | bc -l) )); then
    echo -e "${RED}✗ SLO VIOLATION: Availability ${AVAILABILITY_PCT}% < target $(echo "$SLO_TARGET * 100" | bc)%${NC}"
    SLO_MET=false
else
    echo -e "${GREEN}✓ SLO met: Availability ${AVAILABILITY_PCT}% >= target $(echo "$SLO_TARGET * 100" | bc)%${NC}"
    SLO_MET=true
fi

# Check if error budget allows deployment
if (( $(echo "$ERROR_BUDGET_REMAINING < $ERROR_BUDGET_THRESHOLD" | bc -l) )); then
    echo -e "${RED}✗ ERROR BUDGET LOW: ${ERROR_BUDGET_REMAINING}% remaining < ${ERROR_BUDGET_THRESHOLD}% threshold${NC}"
    BUDGET_OK=false
else
    echo -e "${GREEN}✓ Error budget sufficient: ${ERROR_BUDGET_REMAINING}% remaining${NC}"
    BUDGET_OK=true
fi

echo ""

# Final decision
if [ "$SLO_MET" = true ] && [ "$BUDGET_OK" = true ]; then
    echo -e "${GREEN}======================================${NC}"
    echo -e "${GREEN}✓ DEPLOYMENT APPROVED${NC}"
    echo -e "${GREEN}======================================${NC}"
    echo ""
    echo "SLO compliance verified. Deployment can proceed."
    echo ""
    exit 0
else
    echo -e "${RED}======================================${NC}"
    echo -e "${RED}✗ DEPLOYMENT BLOCKED${NC}"
    echo -e "${RED}======================================${NC}"
    echo ""
    echo "Deployment blocked due to SLO/error budget violation."
    echo ""
    echo -e "${YELLOW}Override Procedure:${NC}"
    echo "  1. Create incident ticket documenting reason"
    echo "  2. Obtain approval from:"
    echo "     - VP Product"
    echo "     - SRE Lead"
    echo "  3. Use override flag: --force-deploy"
    echo "  4. Post-mortem required within 24 hours"
    echo ""
    echo -e "${YELLOW}Investigate Before Proceeding:${NC}"
    echo "  - Check Grafana dashboards: http://grafana:3001"
    echo "  - Review recent errors in Sentry"
    echo "  - Analyze slow burn rate trends"
    echo "  - Consider rolling back recent changes"
    echo ""
    exit 1
fi
