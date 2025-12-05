#!/bin/bash
# FRP-05 Day Zero: Master Validation Script
# Purpose: Run all Day 0 validation checks in sequence
# Usage: ./scripts/frp05-day-zero.sh [database_connection_string]

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

DB_CONN="${1:-${DATABASE_URL:-postgresql://postgres:postgres@localhost:5432/harvestry_dev}}"

echo ""
echo -e "${CYAN}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║                                                                ║${NC}"
echo -e "${CYAN}║          FRP-05 DAY ZERO INFRASTRUCTURE VALIDATION             ║${NC}"
echo -e "${CYAN}║                                                                ║${NC}"
echo -e "${CYAN}║  Validating all prerequisites before Slice 1 execution         ║${NC}"
echo -e "${CYAN}║                                                                ║${NC}"
echo -e "${CYAN}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${BLUE}Database:${NC} ${DB_CONN}"
echo -e "${BLUE}Started:${NC}  $(date)"
echo ""

# Create logs directory
mkdir -p logs

# Results tracking
RESULTS_FILE="logs/frp05-day0-summary.txt"
echo "FRP-05 Day Zero Validation Summary" > "$RESULTS_FILE"
echo "Date: $(date)" >> "$RESULTS_FILE"
echo "Database: ${DB_CONN}" >> "$RESULTS_FILE"
echo "" >> "$RESULTS_FILE"

TOTAL_PASSED=0
TOTAL_FAILED=0
TOTAL_WARNINGS=0

echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${YELLOW}Phase 1: TimescaleDB Validation${NC}"
echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
echo ""
echo "Testing: Extension enablement, hypertables, compression, retention, aggregates"
echo ""

if ./scripts/db/validate-timescaledb.sh "$DB_CONN"; then
    echo "" >> "$RESULTS_FILE"
    echo "✓ PHASE 1 PASSED: TimescaleDB validation successful" >> "$RESULTS_FILE"
    PHASE1_STATUS="${GREEN}✓ PASS${NC}"
    ((TOTAL_PASSED++))
else
    echo "" >> "$RESULTS_FILE"
    echo "✗ PHASE 1 FAILED: TimescaleDB validation failed" >> "$RESULTS_FILE"
    PHASE1_STATUS="${RED}✗ FAIL${NC}"
    ((TOTAL_FAILED++))
fi

echo ""
echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${YELLOW}Phase 2: Logical Replication Validation${NC}"
echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
echo ""
echo "Testing: WAL level, replication slots, permissions"
echo ""

if ./scripts/db/validate-logical-replication.sh "$DB_CONN"; then
    echo "" >> "$RESULTS_FILE"
    echo "✓ PHASE 2 PASSED: Logical replication available" >> "$RESULTS_FILE"
    PHASE2_STATUS="${GREEN}✓ PASS${NC}"
    ((TOTAL_PASSED++))
else
    echo "" >> "$RESULTS_FILE"
    echo "⚠ PHASE 2 WARNING: Logical replication not available - fallback required" >> "$RESULTS_FILE"
    PHASE2_STATUS="${YELLOW}⚠ FALLBACK NEEDED${NC}"
    ((TOTAL_WARNINGS++))
fi

echo ""
echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${YELLOW}Phase 3: MQTT Broker Configuration Check${NC}"
echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
echo ""

if [ -z "${TELEMETRY_MQTT_BROKER_URL}" ]; then
    echo -e "${YELLOW}⚠ MQTT broker URL not configured${NC}"
    echo ""
    echo "Action required:"
    echo "  1. Configure MQTT broker (see docs/telemetry/mqtt-configuration-template.md)"
    echo "  2. Set TELEMETRY_MQTT_BROKER_URL environment variable"
    echo "  3. Test connectivity with: mosquitto_pub or MQTT client"
    echo ""
    echo "⚠ PHASE 3 PENDING: MQTT configuration not complete" >> "$RESULTS_FILE"
    PHASE3_STATUS="${YELLOW}⚠ PENDING${NC}"
    ((TOTAL_WARNINGS++))
else
    echo -e "${BLUE}MQTT Broker: ${TELEMETRY_MQTT_BROKER_URL}${NC}"
    echo ""
    echo -e "${YELLOW}Manual validation required:${NC}"
    echo "  1. Test connection: mosquitto_pub -h <broker> -t test -m 'hello'"
    echo "  2. Verify topic permissions"
    echo "  3. Test throughput (aim for 100 msg/s minimum)"
    echo ""
    echo "✓ PHASE 3 CONFIGURED: MQTT broker URL set (manual testing required)" >> "$RESULTS_FILE"
    PHASE3_STATUS="${GREEN}✓ CONFIGURED${NC}"
    ((TOTAL_PASSED++))
fi

echo ""
echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${YELLOW}Phase 4: Environment Variables Check${NC}"
echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
echo ""

REQUIRED_VARS=(
    "TELEMETRY_DB_CONNECTION"
    "TELEMETRY_MQTT_BROKER_URL"
    "TELEMETRY_MAX_BATCH_SIZE"
    "TELEMETRY_COPY_BATCH_BYTES"
)

MISSING_VARS=0
for VAR in "${REQUIRED_VARS[@]}"; do
    if [ -z "${!VAR}" ]; then
        echo -e "${YELLOW}⚠ Missing: ${VAR}${NC}"
        ((MISSING_VARS++))
    else
        echo -e "${GREEN}✓ Set: ${VAR}${NC}"
    fi
done

if [ $MISSING_VARS -eq 0 ]; then
    echo ""
    echo -e "${GREEN}✓ All required environment variables set${NC}"
    echo "✓ PHASE 4 PASSED: Environment variables configured" >> "$RESULTS_FILE"
    PHASE4_STATUS="${GREEN}✓ PASS${NC}"
    ((TOTAL_PASSED++))
else
    echo ""
    echo -e "${YELLOW}⚠ ${MISSING_VARS} required variable(s) missing${NC}"
    echo ""
    echo "Run: ./scripts/setup/setup-frp05-environment.sh dev"
    echo "⚠ PHASE 4 WARNING: ${MISSING_VARS} environment variable(s) missing" >> "$RESULTS_FILE"
    PHASE4_STATUS="${YELLOW}⚠ INCOMPLETE${NC}"
    ((TOTAL_WARNINGS++))
fi

echo ""
echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${YELLOW}Phase 5: Load Test Environment Check${NC}"
echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
echo ""

if [ -f "tests/load/telemetry-ingest-load.js" ]; then
    echo -e "${GREEN}✓ Load test script exists: tests/load/telemetry-ingest-load.js${NC}"
    echo ""
    echo -e "${YELLOW}Manual validation required:${NC}"
    echo "  1. Ensure k6 is installed: k6 version"
    echo "  2. Run baseline test: k6 run tests/load/telemetry-ingest-load.js"
    echo "  3. Verify p95 < 1.0s with test load"
    echo ""
    echo "✓ PHASE 5 READY: Load test script available" >> "$RESULTS_FILE"
    PHASE5_STATUS="${GREEN}✓ READY${NC}"
    ((TOTAL_PASSED++))
else
    echo -e "${RED}✗ Load test script not found${NC}"
    echo "⚠ PHASE 5 WARNING: Load test script missing" >> "$RESULTS_FILE"
    PHASE5_STATUS="${YELLOW}⚠ MISSING${NC}"
    ((TOTAL_WARNINGS++))
fi

# Final summary
echo ""
echo ""
echo -e "${CYAN}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║                      DAY ZERO SUMMARY                          ║${NC}"
echo -e "${CYAN}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${BLUE}Phase 1: TimescaleDB${NC}              ${PHASE1_STATUS}"
echo -e "${BLUE}Phase 2: Logical Replication${NC}      ${PHASE2_STATUS}"
echo -e "${BLUE}Phase 3: MQTT Broker${NC}              ${PHASE3_STATUS}"
echo -e "${BLUE}Phase 4: Environment Variables${NC}    ${PHASE4_STATUS}"
echo -e "${BLUE}Phase 5: Load Test Environment${NC}    ${PHASE5_STATUS}"
echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "Passed:   ${GREEN}${TOTAL_PASSED}${NC}/5"
echo -e "Warnings: ${YELLOW}${TOTAL_WARNINGS}${NC}/5"
echo -e "Failed:   ${RED}${TOTAL_FAILED}${NC}/5"
echo ""

# Write summary to file
echo "" >> "$RESULTS_FILE"
echo "========================================" >> "$RESULTS_FILE"
echo "SUMMARY" >> "$RESULTS_FILE"
echo "========================================" >> "$RESULTS_FILE"
echo "Passed: $TOTAL_PASSED/5" >> "$RESULTS_FILE"
echo "Warnings: $TOTAL_WARNINGS/5" >> "$RESULTS_FILE"
echo "Failed: $TOTAL_FAILED/5" >> "$RESULTS_FILE"

# GO/NO-GO decision
echo -e "${CYAN}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║                      GO/NO-GO DECISION                         ║${NC}"
echo -e "${CYAN}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""

if [ $TOTAL_FAILED -eq 0 ]; then
    if [ $TOTAL_WARNINGS -eq 0 ]; then
        echo -e "${GREEN}✓ GO: All validations passed!${NC}"
        echo ""
        echo -e "${GREEN}FRP-05 is ready for Slice 1 execution.${NC}"
        echo ""
        echo "DECISION: GO - All validations passed" >> "$RESULTS_FILE"
        DECISION_EXIT=0
    else
        echo -e "${YELLOW}⚠ GO WITH CONDITIONS: Some manual steps required${NC}"
        echo ""
        echo "Review warnings above and complete manual validations:"
        echo ""
        if [[ "$PHASE2_STATUS" == *"FALLBACK"* ]]; then
            echo "  - Logical replication unavailable: implement polling-based fallback"
        fi
        if [[ "$PHASE3_STATUS" == *"PENDING"* ]]; then
            echo "  - MQTT broker: complete configuration and connectivity test"
        fi
        if [[ "$PHASE4_STATUS" == *"INCOMPLETE"* ]]; then
            echo "  - Environment variables: set missing required variables"
        fi
        if [[ "$PHASE5_STATUS" == *"MISSING"* ]]; then
            echo "  - Load test: ensure k6 script is available"
        fi
        echo ""
        echo -e "${YELLOW}FRP-05 can proceed with conditions noted above.${NC}"
        echo ""
        echo "DECISION: GO WITH CONDITIONS - Manual validation needed" >> "$RESULTS_FILE"
        DECISION_EXIT=0
    fi
else
    echo -e "${RED}✗ NO-GO: Critical failures detected${NC}"
    echo ""
    echo "Fix critical failures before proceeding:"
    echo ""
    if [[ "$PHASE1_STATUS" == *"FAIL"* ]]; then
        echo "  - TimescaleDB: Extension or features unavailable"
        echo "    → Contact database team to enable TimescaleDB"
    fi
    echo ""
    echo -e "${RED}Cannot proceed with FRP-05 until failures are resolved.${NC}"
    echo ""
    echo "DECISION: NO-GO - Critical failures" >> "$RESULTS_FILE"
    DECISION_EXIT=1
fi

echo ""
echo -e "${BLUE}Completed:${NC} $(date)"
echo -e "${BLUE}Results:${NC}   ${RESULTS_FILE}"
echo ""
echo -e "${CYAN}════════════════════════════════════════════════════════════════${NC}"
echo ""

# Create Day Zero results document
echo -e "${BLUE}Creating Day Zero results document...${NC}"
./scripts/setup/create-day-zero-results.sh
echo ""

exit $DECISION_EXIT

