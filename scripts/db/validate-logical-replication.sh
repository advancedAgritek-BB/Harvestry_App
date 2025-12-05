#!/bin/bash
# FRP-05 Day Zero: Logical Replication Validation Script
# Purpose: Validate logical replication availability for WAL-based real-time push
# Usage: ./validate-logical-replication.sh [connection_string]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Connection string
DB_CONN="${1:-${DATABASE_URL:-postgresql://postgres:postgres@localhost:5432/harvestry_dev}}"

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}FRP-05 Logical Replication Validation${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Results tracking
RESULTS_FILE="$(pwd)/logs/frp05-day0-replication-results.txt"
mkdir -p "$(pwd)/logs"
echo "Logical Replication Validation Results - $(date)" > "$RESULTS_FILE"
echo "Connection: ${DB_CONN}" >> "$RESULTS_FILE"
echo "" >> "$RESULTS_FILE"

PASSED=0
FAILED=0
WARNINGS=0

echo -e "${YELLOW}Phase 1: Database Configuration Check${NC}"
echo "-----------------------------------"
echo ""

# Test 1: Check WAL level
echo -e "${BLUE}Checking WAL level...${NC}"
WAL_LEVEL=$(psql "$DB_CONN" -t -c "SHOW wal_level;" 2>/dev/null | xargs)
echo "WAL Level: $WAL_LEVEL" >> "$RESULTS_FILE"

if [ "$WAL_LEVEL" = "logical" ]; then
    echo -e "${GREEN}✓ WAL level is 'logical' - replication ready${NC}"
    echo "PASS: WAL level is logical" >> "$RESULTS_FILE"
    ((PASSED++))
elif [ "$WAL_LEVEL" = "replica" ] || [ "$WAL_LEVEL" = "minimal" ]; then
    echo -e "${RED}✗ WAL level is '$WAL_LEVEL' - requires 'logical'${NC}"
    echo "FAIL: WAL level is $WAL_LEVEL (requires logical)" >> "$RESULTS_FILE"
    echo -e "${YELLOW}Action: Set wal_level = logical in postgresql.conf and restart${NC}"
    ((FAILED++))
else
    echo -e "${YELLOW}⚠ Unknown WAL level: $WAL_LEVEL${NC}"
    echo "WARNING: Unknown WAL level" >> "$RESULTS_FILE"
    ((WARNINGS++))
fi

# Test 2: Check max_replication_slots
echo -e "${BLUE}Checking replication slot limit...${NC}"
MAX_SLOTS=$(psql "$DB_CONN" -t -c "SHOW max_replication_slots;" 2>/dev/null | xargs)
USED_SLOTS=$(psql "$DB_CONN" -t -c "SELECT COUNT(*) FROM pg_replication_slots;" 2>/dev/null | xargs)
AVAILABLE_SLOTS=$((MAX_SLOTS - USED_SLOTS))

echo "Max Replication Slots: $MAX_SLOTS" >> "$RESULTS_FILE"
echo "Used Slots: $USED_SLOTS" >> "$RESULTS_FILE"
echo "Available Slots: $AVAILABLE_SLOTS" >> "$RESULTS_FILE"

if [ "$AVAILABLE_SLOTS" -ge 2 ]; then
    echo -e "${GREEN}✓ Sufficient replication slots available ($AVAILABLE_SLOTS free)${NC}"
    echo "PASS: Sufficient replication slots" >> "$RESULTS_FILE"
    ((PASSED++))
elif [ "$AVAILABLE_SLOTS" -ge 1 ]; then
    echo -e "${YELLOW}⚠ Limited replication slots ($AVAILABLE_SLOTS free)${NC}"
    echo "WARNING: Limited replication slots" >> "$RESULTS_FILE"
    ((WARNINGS++))
else
    echo -e "${RED}✗ No replication slots available${NC}"
    echo "FAIL: No replication slots available" >> "$RESULTS_FILE"
    echo -e "${YELLOW}Action: Increase max_replication_slots in postgresql.conf${NC}"
    ((FAILED++))
fi

# Test 3: Check max_wal_senders
echo -e "${BLUE}Checking WAL sender limit...${NC}"
MAX_WAL_SENDERS=$(psql "$DB_CONN" -t -c "SHOW max_wal_senders;" 2>/dev/null | xargs)
ACTIVE_SENDERS=$(psql "$DB_CONN" -t -c "SELECT COUNT(*) FROM pg_stat_replication;" 2>/dev/null | xargs)
AVAILABLE_SENDERS=$((MAX_WAL_SENDERS - ACTIVE_SENDERS))

echo "Max WAL Senders: $MAX_WAL_SENDERS" >> "$RESULTS_FILE"
echo "Active Senders: $ACTIVE_SENDERS" >> "$RESULTS_FILE"
echo "Available Senders: $AVAILABLE_SENDERS" >> "$RESULTS_FILE"

if [ "$AVAILABLE_SENDERS" -ge 1 ]; then
    echo -e "${GREEN}✓ Sufficient WAL senders available ($AVAILABLE_SENDERS free)${NC}"
    echo "PASS: Sufficient WAL senders" >> "$RESULTS_FILE"
    ((PASSED++))
else
    echo -e "${YELLOW}⚠ No WAL senders available${NC}"
    echo "WARNING: No WAL senders available" >> "$RESULTS_FILE"
    echo -e "${YELLOW}Action: Increase max_wal_senders in postgresql.conf${NC}"
    ((WARNINGS++))
fi

echo ""
echo -e "${YELLOW}Phase 2: Replication Permissions${NC}"
echo "-----------------------------------"
echo ""

# Test 4: Check replication role
echo -e "${BLUE}Checking replication permissions...${NC}"
HAS_REPLICATION=$(psql "$DB_CONN" -t -c "SELECT COUNT(*) FROM pg_roles WHERE rolname = current_user AND rolreplication = true;" 2>/dev/null | xargs)

if [ "$HAS_REPLICATION" = "1" ]; then
    echo -e "${GREEN}✓ Current user has REPLICATION privilege${NC}"
    echo "PASS: REPLICATION privilege granted" >> "$RESULTS_FILE"
    ((PASSED++))
else
    echo -e "${RED}✗ Current user lacks REPLICATION privilege${NC}"
    echo "FAIL: REPLICATION privilege not granted" >> "$RESULTS_FILE"
    echo -e "${YELLOW}Action: GRANT REPLICATION TO current_user${NC}"
    ((FAILED++))
fi

echo ""
echo -e "${YELLOW}Phase 3: Replication Slot Testing${NC}"
echo "-----------------------------------"
echo ""

# Test 5: Create test replication slot
echo -e "${BLUE}Testing replication slot creation...${NC}"
TEST_SLOT_NAME="frp05_test_slot_$(date +%s)"

if psql "$DB_CONN" -c "SELECT * FROM pg_create_logical_replication_slot('$TEST_SLOT_NAME', 'pgoutput');" >> "$RESULTS_FILE" 2>&1; then
    echo -e "${GREEN}✓ Test replication slot created successfully${NC}"
    echo "PASS: Replication slot creation" >> "$RESULTS_FILE"
    ((PASSED++))
    
    # Test 6: Verify slot exists
    echo -e "${BLUE}Verifying slot exists...${NC}"
    SLOT_EXISTS=$(psql "$DB_CONN" -t -c "SELECT COUNT(*) FROM pg_replication_slots WHERE slot_name = '$TEST_SLOT_NAME';" 2>/dev/null | xargs)
    
    if [ "$SLOT_EXISTS" = "1" ]; then
        echo -e "${GREEN}✓ Replication slot verified${NC}"
        echo "PASS: Replication slot verified" >> "$RESULTS_FILE"
        ((PASSED++))
    else
        echo -e "${RED}✗ Replication slot not found${NC}"
        echo "FAIL: Replication slot not found" >> "$RESULTS_FILE"
        ((FAILED++))
    fi
    
    # Cleanup: Drop test slot
    echo -e "${BLUE}Cleaning up test slot...${NC}"
    psql "$DB_CONN" -c "SELECT pg_drop_replication_slot('$TEST_SLOT_NAME');" > /dev/null 2>&1
    echo -e "${GREEN}✓ Test slot dropped${NC}"
    
else
    echo -e "${RED}✗ Cannot create replication slot${NC}"
    echo "FAIL: Replication slot creation failed" >> "$RESULTS_FILE"
    echo -e "${YELLOW}Possible reasons:${NC}"
    echo -e "${YELLOW}  - Insufficient permissions${NC}"
    echo -e "${YELLOW}  - WAL level not set to 'logical'${NC}"
    echo -e "${YELLOW}  - Managed database with restrictions${NC}"
    ((FAILED++))
fi

echo ""
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}Validation Results${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo -e "Passed:   ${GREEN}${PASSED}${NC}"
echo -e "Failed:   ${RED}${FAILED}${NC}"
echo -e "Warnings: ${YELLOW}${WARNINGS}${NC}"
echo ""

# Write summary
echo "" >> "$RESULTS_FILE"
echo "========================================" >> "$RESULTS_FILE"
echo "SUMMARY" >> "$RESULTS_FILE"
echo "========================================" >> "$RESULTS_FILE"
echo "Passed: $PASSED" >> "$RESULTS_FILE"
echo "Failed: $FAILED" >> "$RESULTS_FILE"
echo "Warnings: $WARNINGS" >> "$RESULTS_FILE"
echo "" >> "$RESULTS_FILE"

if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}✓ Logical replication is available!${NC}"
    if [ $WARNINGS -gt 0 ]; then
        echo "RESULT: PASS WITH WARNINGS" >> "$RESULTS_FILE"
        echo ""
        echo -e "${YELLOW}Recommendation: WAL-based real-time push can proceed with monitoring${NC}"
    else
        echo "RESULT: FULL PASS" >> "$RESULTS_FILE"
        echo ""
        echo -e "${GREEN}Recommendation: WAL-based real-time push ready for implementation${NC}"
    fi
    echo ""
    echo -e "Results saved to: ${BLUE}${RESULTS_FILE}${NC}"
    exit 0
else
    echo -e "${RED}✗ Logical replication not available${NC}"
    echo "RESULT: FAIL" >> "$RESULTS_FILE"
    echo "" >> "$RESULTS_FILE"
    echo "FALLBACK STRATEGY REQUIRED:" >> "$RESULTS_FILE"
    echo "- Implement polling-based real-time push" >> "$RESULTS_FILE"
    echo "- Use database triggers + notification queue" >> "$RESULTS_FILE"
    echo "- Accept 2-5 second latency instead of sub-second" >> "$RESULTS_FILE"
    echo ""
    echo -e "${YELLOW}========================================${NC}"
    echo -e "${YELLOW}Fallback Strategy${NC}"
    echo -e "${YELLOW}========================================${NC}"
    echo ""
    echo -e "${YELLOW}Since logical replication is unavailable, use polling-based approach:${NC}"
    echo ""
    echo -e "1. Create notification queue table"
    echo -e "2. Use database trigger on sensor_readings INSERT"
    echo -e "3. Poll queue every 1-2 seconds from background worker"
    echo -e "4. Fan out to SignalR subscribers"
    echo -e "5. Accept 2-5s latency (still meets most use cases)"
    echo ""
    echo -e "This approach:"
    echo -e "  ${GREEN}✓${NC} Works in managed databases"
    echo -e "  ${GREEN}✓${NC} No special permissions required"
    echo -e "  ${GREEN}✓${NC} Simple to implement"
    echo -e "  ${YELLOW}⚠${NC} Slightly higher latency (2-5s vs <1s)"
    echo ""
    echo -e "Results saved to: ${BLUE}${RESULTS_FILE}${NC}"
    exit 1
fi

