#!/bin/bash
# Harvestry ERP - Run Contract Tests
# Track A: Validate API contracts

set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Default values
ENVIRONMENT="dev"
TEST_TYPE="all"

# Print usage
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Run contract tests for REST and WebSocket APIs.

OPTIONS:
    -e, --env ENV        Environment (dev, staging, production) [default: dev]
    -t, --type TYPE      Test type (all, rest, websocket) [default: all]
    -h, --help           Show this help message

EXAMPLES:
    # Run all contract tests
    $0 --env dev

    # Run only REST contract tests
    $0 --env staging --type rest

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
        -t|--type)
            TEST_TYPE="$2"
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

echo -e "${GREEN}======================================${NC}"
echo -e "${GREEN}Contract Tests${NC}"
echo -e "${GREEN}======================================${NC}"
echo ""
echo "Environment: $ENVIRONMENT"
echo "Test Type:   $TEST_TYPE"
echo ""

TESTS_FAILED=0

# Run REST contract tests
if [ "$TEST_TYPE" = "all" ] || [ "$TEST_TYPE" = "rest" ]; then
    echo -e "${YELLOW}Running REST contract tests...${NC}"
    
    if npm run test:contract:rest; then
        echo -e "${GREEN}✓ REST contract tests passed${NC}"
    else
        echo -e "${RED}✗ REST contract tests failed${NC}"
        TESTS_FAILED=$((TESTS_FAILED + 1))
    fi
    
    echo ""
fi

# Run WebSocket contract tests
if [ "$TEST_TYPE" = "all" ] || [ "$TEST_TYPE" = "websocket" ]; then
    echo -e "${YELLOW}Running WebSocket contract tests...${NC}"
    
    if npm run test:contract:websocket; then
        echo -e "${GREEN}✓ WebSocket contract tests passed${NC}"
    else
        echo -e "${RED}✗ WebSocket contract tests failed${NC}"
        TESTS_FAILED=$((TESTS_FAILED + 1))
    fi
    
    echo ""
fi

# Summary
echo -e "${GREEN}======================================${NC}"

if [ $TESTS_FAILED -eq 0 ]; then
    echo -e "${GREEN}✓ ALL CONTRACT TESTS PASSED${NC}"
    exit 0
else
    echo -e "${RED}✗ $TESTS_FAILED TEST SUITE(S) FAILED${NC}"
    exit 1
fi
