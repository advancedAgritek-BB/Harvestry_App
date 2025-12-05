#!/usr/bin/env bash

# ============================================================================
# Harvestry Load Test Runner
#
# Prerequisites:
#   - k6 installed (brew install k6 or https://k6.io/docs/getting-started/installation/)
#   - Telemetry API running
#   - Database seeded with test data
#
# Usage:
#   ./run-load-tests.sh [test-type] [options]
#
# Test Types:
#   baseline   - Quick validation (5 min, low load)
#   sustained  - Full SLO validation (15 min, target load)
#   stress     - Find breaking point (increasing load)
#
# Options:
#   --api-url URL    - API base URL (default: http://localhost:5000)
#   --output DIR     - Results output directory (default: ./results)
#   --dry-run        - Show what would be run without executing
#
# Example:
#   ./run-load-tests.sh baseline
#   ./run-load-tests.sh sustained --api-url http://telemetry-api:5000
# ============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEST_TYPE="${1:-baseline}"
API_URL="${API_URL:-http://localhost:5000}"
OUTPUT_DIR="${OUTPUT_DIR:-${SCRIPT_DIR}/results}"
DRY_RUN=false
API_TOKEN="${API_TOKEN:-dev-token}"

# Color output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }
log_step() { echo -e "${CYAN}[STEP]${NC} $1"; }

# Parse arguments
parse_args() {
    shift # Remove test type
    while [[ $# -gt 0 ]]; do
        case "$1" in
            --api-url)
                API_URL="$2"
                shift 2
                ;;
            --output)
                OUTPUT_DIR="$2"
                shift 2
                ;;
            --dry-run)
                DRY_RUN=true
                shift
                ;;
            *)
                log_error "Unknown option: $1"
                exit 1
                ;;
        esac
    done
}

# Check prerequisites
check_prerequisites() {
    log_step "Checking prerequisites..."
    
    if ! command -v k6 &> /dev/null; then
        log_error "k6 is not installed"
        log_info "Install with: brew install k6 (macOS) or see https://k6.io/docs/getting-started/installation/"
        exit 1
    fi
    
    local k6_version
    k6_version=$(k6 version 2>&1 | head -n1)
    log_info "k6 version: $k6_version"
    
    # Check if API is reachable
    if ! curl -s --connect-timeout 5 "${API_URL}/health" > /dev/null 2>&1; then
        log_warn "API at ${API_URL} may not be reachable"
        log_info "Make sure the Telemetry API is running"
    else
        log_success "API is reachable at ${API_URL}"
    fi
}

# Create output directory
setup_output() {
    mkdir -p "${OUTPUT_DIR}"
    log_info "Results will be saved to: ${OUTPUT_DIR}"
}

# Get test configuration
get_test_config() {
    case "$TEST_TYPE" in
        baseline)
            echo "telemetry-ingest-load.js"
            ;;
        sustained)
            echo "telemetry-ingest-load.js"
            ;;
        stress)
            echo "telemetry-ingest-load.js"
            ;;
        realtime)
            echo "realtime-push-load.js"
            ;;
        gateway)
            echo "api-gateway-load.js"
            ;;
        *)
            log_error "Unknown test type: $TEST_TYPE"
            log_info "Valid types: baseline, sustained, stress, realtime, gateway"
            exit 1
            ;;
    esac
}

# Run the load test
run_test() {
    local test_file
    test_file=$(get_test_config)
    local test_path="${SCRIPT_DIR}/${test_file}"
    
    if [[ ! -f "$test_path" ]]; then
        log_error "Test file not found: $test_path"
        exit 1
    fi
    
    local timestamp
    timestamp=$(date +%Y%m%d_%H%M%S)
    local result_file="${OUTPUT_DIR}/${TEST_TYPE}_${timestamp}"
    
    # Build k6 options based on test type
    local k6_opts=""
    case "$TEST_TYPE" in
        baseline)
            k6_opts="--duration 5m --vus 10"
            ;;
        sustained)
            k6_opts="--duration 15m --vus 50"
            ;;
        stress)
            k6_opts="--stage 2m:50 --stage 5m:100 --stage 2m:150 --stage 5m:150 --stage 2m:0"
            ;;
    esac
    
    log_step "Running ${TEST_TYPE} load test..."
    log_info "Test file: ${test_file}"
    log_info "Target API: ${API_URL}"
    log_info "Options: ${k6_opts}"
    
    if [[ "$DRY_RUN" == "true" ]]; then
        log_warn "DRY RUN - would execute:"
        echo "  k6 run ${k6_opts} -e BASE_URL=${API_URL} -e API_TOKEN=${API_TOKEN} ${test_path} --out json=${result_file}.json --summary-export=${result_file}_summary.json"
        return 0
    fi
    
    # Run k6
    if k6 run ${k6_opts} \
        -e BASE_URL="${API_URL}" \
        -e API_TOKEN="${API_TOKEN}" \
        "${test_path}" \
        --out json="${result_file}.json" \
        --summary-export="${result_file}_summary.json"; then
        log_success "Load test completed successfully!"
        log_info "Results: ${result_file}.json"
        log_info "Summary: ${result_file}_summary.json"
        return 0
    else
        log_error "Load test failed!"
        return 1
    fi
}

# Analyze results
analyze_results() {
    local latest_summary
    latest_summary=$(ls -t "${OUTPUT_DIR}"/*_summary.json 2>/dev/null | head -n1)
    
    if [[ -z "$latest_summary" || ! -f "$latest_summary" ]]; then
        log_warn "No summary file found to analyze"
        return 0
    fi
    
    log_step "Analyzing results from: $(basename "$latest_summary")"
    
    # Extract key metrics using jq if available
    if command -v jq &> /dev/null; then
        echo ""
        echo "=========================================="
        echo "  Load Test Results Summary"
        echo "=========================================="
        
        local http_req_duration_p95
        http_req_duration_p95=$(jq -r '.metrics.http_req_duration.values."p(95)" // "N/A"' "$latest_summary")
        
        local http_req_failed_rate
        http_req_failed_rate=$(jq -r '.metrics.http_req_failed.values.rate // 0' "$latest_summary")
        
        local iterations
        iterations=$(jq -r '.metrics.iterations.values.count // 0' "$latest_summary")
        
        echo ""
        echo "  HTTP Request Duration (p95): ${http_req_duration_p95}ms"
        echo "  HTTP Request Failed Rate:    ${http_req_failed_rate}"
        echo "  Total Iterations:            ${iterations}"
        echo ""
        
        # Check SLO thresholds
        if [[ "$http_req_duration_p95" != "N/A" ]]; then
            local p95_int
            p95_int=$(echo "$http_req_duration_p95" | cut -d. -f1)
            if [[ "$p95_int" -lt 1000 ]]; then
                log_success "✓ SLO MET: p95 < 1000ms (${http_req_duration_p95}ms)"
            else
                log_error "✗ SLO VIOLATED: p95 >= 1000ms (${http_req_duration_p95}ms)"
            fi
        fi
        
        echo "=========================================="
    else
        log_info "Install jq for detailed analysis: brew install jq"
        cat "$latest_summary"
    fi
}

# Main execution
main() {
    echo ""
    echo "=========================================="
    echo "  Harvestry Load Test Runner"
    echo "=========================================="
    echo ""
    
    if [[ $# -gt 0 ]]; then
        parse_args "$@"
    fi
    
    check_prerequisites
    setup_output
    run_test
    analyze_results
    
    echo ""
    log_success "Done!"
}

main "$@"

