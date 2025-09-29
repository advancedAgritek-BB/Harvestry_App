#!/bin/bash
# Harvestry ERP - Run Load Tests
# Track A: Execute k6 load tests and validate SLO targets

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default values
ENVIRONMENT="staging"
TEST_SUITE="all"
BASE_URL=""
API_TOKEN=""
RESULTS_DIR="./test-results/load"

# Print usage
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Run k6 load tests to validate SLO targets.

OPTIONS:
    -e, --env ENV        Environment (staging, production) [default: staging]
    -t, --test SUITE     Test suite (all, telemetry, realtime, api) [default: all]
    -u, --url URL        Base URL for API [default: env-specific]
    -k, --token TOKEN    API token for authentication
    -o, --output DIR     Output directory for results [default: ./test-results/load]
    -h, --help           Show this help message

EXAMPLES:
    # Run all tests against staging
    $0 --env staging

    # Run only telemetry ingest test
    $0 --env staging --test telemetry

    # Run with custom URL
    $0 --url https://api.harvestry.com --token my-token

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
        -t|--test)
            TEST_SUITE="$2"
            shift 2
            ;;
        -u|--url)
            BASE_URL="$2"
            shift 2
            ;;
        -k|--token)
            API_TOKEN="$2"
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

# Set environment-specific defaults
if [ -z "$BASE_URL" ]; then
    case $ENVIRONMENT in
        staging)
            BASE_URL="https://staging-api.harvestry.com"
            ;;
        production)
            BASE_URL="https://api.harvestry.com"
            ;;
        *)
            BASE_URL="http://localhost:5000"
            ;;
    esac
fi

# Check for k6
if ! command -v k6 &> /dev/null; then
    echo -e "${RED}Error: k6 not found. Please install k6:${NC}"
    echo "  macOS: brew install k6"
    echo "  Linux: sudo apt-get install k6"
    echo "  Or download from: https://k6.io/docs/getting-started/installation/"
    exit 1
fi

# Create results directory
mkdir -p "$RESULTS_DIR"

echo -e "${GREEN}======================================${NC}"
echo -e "${GREEN}Harvestry ERP - Load Testing${NC}"
echo -e "${GREEN}======================================${NC}"
echo ""
echo "Environment:  $ENVIRONMENT"
echo "Base URL:     $BASE_URL"
echo "Test Suite:   $TEST_SUITE"
echo "Results:      $RESULTS_DIR"
echo ""

# Production safety check
if [ "$ENVIRONMENT" = "production" ]; then
    echo -e "${RED}WARNING: You are about to run load tests against PRODUCTION!${NC}"
    read -p "Type 'RUN PRODUCTION LOAD TEST' to continue: " confirmation
    if [ "$confirmation" != "RUN PRODUCTION LOAD TEST" ]; then
        echo -e "${GREEN}Load test cancelled.${NC}"
        exit 1
    fi
fi

# Run tests
TESTS_FAILED=0
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

run_test() {
    local test_name=$1
    local test_script=$2
    local output_file="${RESULTS_DIR}/${test_name}_${TIMESTAMP}.json"
    
    echo -e "${YELLOW}Running $test_name test...${NC}"
    
    if k6 run \
        --out json="$output_file" \
        -e BASE_URL="$BASE_URL" \
        -e API_TOKEN="$API_TOKEN" \
        "$test_script"; then
        echo -e "${GREEN}✓ $test_name test passed${NC}"
        echo ""
    else
        echo -e "${RED}✗ $test_name test failed${NC}"
        echo ""
        TESTS_FAILED=$((TESTS_FAILED + 1))
    fi
}

# Execute test suite
case $TEST_SUITE in
    telemetry)
        run_test "telemetry-ingest" "tests/load/telemetry-ingest-load.js"
        ;;
    realtime)
        run_test "realtime-push" "tests/load/realtime-push-load.js"
        ;;
    api)
        run_test "api-gateway" "tests/load/api-gateway-load.js"
        ;;
    all)
        run_test "telemetry-ingest" "tests/load/telemetry-ingest-load.js"
        run_test "realtime-push" "tests/load/realtime-push-load.js"
        run_test "api-gateway" "tests/load/api-gateway-load.js"
        ;;
    *)
        echo -e "${RED}Error: Invalid test suite: $TEST_SUITE${NC}"
        usage
        ;;
esac

# Generate summary report
echo -e "${YELLOW}Generating summary report...${NC}"

cat > "${RESULTS_DIR}/summary_${TIMESTAMP}.txt" << EOF
Harvestry ERP Load Test Summary
================================
Timestamp: $(date)
Environment: $ENVIRONMENT
Base URL: $BASE_URL
Test Suite: $TEST_SUITE

SLO Targets:
- Telemetry Ingest: p95 < 1.0s, p99 < 2.5s
- Realtime Push: p95 < 1.5s, p99 < 3.0s
- API Commands: p95 < 800ms
- API Tasks: p95 < 300ms

Results:
- Total Tests: $((TEST_SUITE == "all" ? 3 : 1))
- Passed: $(( (TEST_SUITE == "all" ? 3 : 1) - TESTS_FAILED ))
- Failed: $TESTS_FAILED

Detailed results available in: $RESULTS_DIR
EOF

cat "${RESULTS_DIR}/summary_${TIMESTAMP}.txt"

# Exit with appropriate code
if [ $TESTS_FAILED -gt 0 ]; then
    echo ""
    echo -e "${RED}✗ $TESTS_FAILED test(s) failed!${NC}"
    echo -e "${YELLOW}Review results in: $RESULTS_DIR${NC}"
    exit 1
else
    echo ""
    echo -e "${GREEN}✓ All tests passed!${NC}"
    exit 0
fi
