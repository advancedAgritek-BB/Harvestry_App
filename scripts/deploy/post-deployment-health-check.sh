#!/bin/bash
# Harvestry ERP - Post-Deployment Health Check
# Track A: Validate deployment health before promoting

set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Default values
ENVIRONMENT="staging"
SERVICE=""
NAMESPACE=""
HEALTH_CHECK_RETRIES=20
HEALTH_CHECK_INTERVAL=15
SMOKE_TEST_ENABLED=true
CONTRACT_TEST_ENABLED=true

# Print usage
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Run comprehensive health checks after deployment.

OPTIONS:
    -e, --env ENV         Environment (staging, production) [default: staging]
    -s, --service SERVICE Service name (required)
    -n, --namespace NS    Kubernetes namespace [default: harvestry-ENV]
    -r, --retries NUM     Health check retries [default: 20]
    -i, --interval SECS   Interval between retries [default: 15]
    --no-smoke            Skip smoke tests
    --no-contract         Skip contract tests
    -h, --help            Show this help message

EXAMPLES:
    # Standard health check
    $0 --env staging --service api-gateway

    # Quick check (no smoke/contract tests)
    $0 --env staging --service api-gateway --no-smoke --no-contract

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
        -n|--namespace)
            NAMESPACE="$2"
            shift 2
            ;;
        -r|--retries)
            HEALTH_CHECK_RETRIES="$2"
            shift 2
            ;;
        -i|--interval)
            HEALTH_CHECK_INTERVAL="$2"
            shift 2
            ;;
        --no-smoke)
            SMOKE_TEST_ENABLED=false
            shift
            ;;
        --no-contract)
            CONTRACT_TEST_ENABLED=false
            shift
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

# Set default namespace
if [ -z "$NAMESPACE" ]; then
    NAMESPACE="harvestry-${ENVIRONMENT}"
fi

echo -e "${YELLOW}======================================${NC}"
echo -e "${YELLOW}Post-Deployment Health Check${NC}"
echo -e "${YELLOW}======================================${NC}"
echo ""
echo "Environment: $ENVIRONMENT"
echo "Service:     $SERVICE"
echo "Namespace:   $NAMESPACE"
echo ""

CHECKS_FAILED=0

# ==============================================================================
# Check 1: Pod Status
# ==============================================================================
echo -e "${YELLOW}[1/5] Checking pod status...${NC}"

POD_COUNT=$(kubectl --namespace="$NAMESPACE" get pods -l app="$SERVICE" --no-headers 2>/dev/null | wc -l)

if [ "$POD_COUNT" -eq 0 ]; then
    echo -e "${RED}✗ No pods found for service $SERVICE${NC}"
    CHECKS_FAILED=$((CHECKS_FAILED + 1))
else
    echo -e "${GREEN}✓ Found $POD_COUNT pod(s)${NC}"
    
    # Check pod ready status
    READY_PODS=$(kubectl --namespace="$NAMESPACE" get pods -l app="$SERVICE" \
        -o jsonpath='{.items[*].status.conditions[?(@.type=="Ready")].status}' | grep -o True | wc -l)
    
    if [ "$READY_PODS" -eq "$POD_COUNT" ]; then
        echo -e "${GREEN}✓ All pods ready ($READY_PODS/$POD_COUNT)${NC}"
    else
        echo -e "${RED}✗ Not all pods ready ($READY_PODS/$POD_COUNT)${NC}"
        CHECKS_FAILED=$((CHECKS_FAILED + 1))
    fi
fi

echo ""

# ==============================================================================
# Check 2: Health Endpoints
# ==============================================================================
echo -e "${YELLOW}[2/5] Checking health endpoints...${NC}"

HEALTH_CHECK_SUCCESS=false
for i in $(seq 1 $HEALTH_CHECK_RETRIES); do
    echo -e "${YELLOW}Attempt $i/$HEALTH_CHECK_RETRIES...${NC}"
    
    POD=$(kubectl --namespace="$NAMESPACE" get pods -l app="$SERVICE" \
        -o jsonpath='{.items[0].metadata.name}' 2>/dev/null)
    
    if [ -z "$POD" ]; then
        echo -e "${RED}No pod found${NC}"
        sleep "$HEALTH_CHECK_INTERVAL"
        continue
    fi
    
    # Check liveness
    if kubectl --namespace="$NAMESPACE" exec "$POD" -- \
        wget -q -O- http://localhost:8080/health/live > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Liveness check passed${NC}"
    else
        echo -e "${RED}✗ Liveness check failed${NC}"
        sleep "$HEALTH_CHECK_INTERVAL"
        continue
    fi
    
    # Check readiness
    if kubectl --namespace="$NAMESPACE" exec "$POD" -- \
        wget -q -O- http://localhost:8080/health/ready > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Readiness check passed${NC}"
        HEALTH_CHECK_SUCCESS=true
        break
    else
        echo -e "${RED}✗ Readiness check failed${NC}"
        sleep "$HEALTH_CHECK_INTERVAL"
    fi
done

if [ "$HEALTH_CHECK_SUCCESS" = false ]; then
    echo -e "${RED}✗ Health checks failed after $HEALTH_CHECK_RETRIES attempts${NC}"
    CHECKS_FAILED=$((CHECKS_FAILED + 1))
fi

echo ""

# ==============================================================================
# Check 3: Service Endpoints
# ==============================================================================
echo -e "${YELLOW}[3/5] Checking service endpoints...${NC}"

ENDPOINTS=$(kubectl --namespace="$NAMESPACE" get endpoints "$SERVICE" \
    -o jsonpath='{.subsets[*].addresses[*].ip}' 2>/dev/null | wc -w)

if [ "$ENDPOINTS" -eq 0 ]; then
    echo -e "${RED}✗ No endpoints found for service $SERVICE${NC}"
    CHECKS_FAILED=$((CHECKS_FAILED + 1))
else
    echo -e "${GREEN}✓ Service has $ENDPOINTS endpoint(s)${NC}"
fi

echo ""

# ==============================================================================
# Check 4: Smoke Tests (Optional)
# ==============================================================================
if [ "$SMOKE_TEST_ENABLED" = true ]; then
    echo -e "${YELLOW}[4/5] Running smoke tests...${NC}"
    
    # Get service URL
    if [ "$ENVIRONMENT" = "production" ]; then
        SERVICE_URL="https://api.harvestry.com"
    else
        SERVICE_URL="https://staging-api.harvestry.com"
    fi
    
    # Test 1: API health
    if curl -f -s "${SERVICE_URL}/health" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ API health endpoint accessible${NC}"
    else
        echo -e "${RED}✗ API health endpoint failed${NC}"
        CHECKS_FAILED=$((CHECKS_FAILED + 1))
    fi
    
    # Test 2: Metrics endpoint
    if curl -f -s "${SERVICE_URL}/metrics" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Metrics endpoint accessible${NC}"
    else
        echo -e "${YELLOW}⚠ Metrics endpoint not accessible (non-critical)${NC}"
    fi
    
    echo ""
else
    echo -e "${YELLOW}[4/5] Skipping smoke tests${NC}"
    echo ""
fi

# ==============================================================================
# Check 5: Contract Tests (Optional)
# ==============================================================================
if [ "$CONTRACT_TEST_ENABLED" = true ]; then
    echo -e "${YELLOW}[5/5] Running contract tests...${NC}"
    
    # Run contract tests if available
    if [ -f "./tests/contract/run-tests.sh" ]; then
        if ./tests/contract/run-tests.sh --env="$ENVIRONMENT" --service="$SERVICE"; then
            echo -e "${GREEN}✓ Contract tests passed${NC}"
        else
            echo -e "${RED}✗ Contract tests failed${NC}"
            CHECKS_FAILED=$((CHECKS_FAILED + 1))
        fi
    else
        echo -e "${YELLOW}⚠ No contract tests found (non-critical)${NC}"
    fi
    
    echo ""
else
    echo -e "${YELLOW}[5/5] Skipping contract tests${NC}"
    echo ""
fi

# ==============================================================================
# Summary
# ==============================================================================
echo -e "${YELLOW}======================================${NC}"

if [ $CHECKS_FAILED -eq 0 ]; then
    echo -e "${GREEN}✓ ALL CHECKS PASSED${NC}"
    echo -e "${GREEN}======================================${NC}"
    echo ""
    echo "Deployment is healthy. Safe to proceed with traffic shift."
    echo ""
    exit 0
else
    echo -e "${RED}✗ $CHECKS_FAILED CHECK(S) FAILED${NC}"
    echo -e "${RED}======================================${NC}"
    echo ""
    echo "Deployment health check failed. DO NOT proceed with traffic shift."
    echo ""
    echo -e "${YELLOW}Next Steps:${NC}"
    echo "  1. Check pod logs: kubectl --namespace=$NAMESPACE logs -l app=$SERVICE"
    echo "  2. Describe pods: kubectl --namespace=$NAMESPACE describe pods -l app=$SERVICE"
    echo "  3. Check events: kubectl --namespace=$NAMESPACE get events --sort-by='.lastTimestamp'"
    echo "  4. Consider rollback: ./scripts/deploy/blue-green-deploy.sh --rollback"
    echo ""
    exit 1
fi
