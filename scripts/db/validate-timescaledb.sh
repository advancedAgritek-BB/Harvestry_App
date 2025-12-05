#!/bin/bash
# FRP-05 Day Zero: TimescaleDB Validation Script
# Purpose: Validate TimescaleDB features before FRP-05 implementation
# Usage: ./validate-timescaledb.sh [connection_string]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Connection string (default to env var or localhost)
DB_CONN="${1:-${DATABASE_URL:-postgresql://postgres:postgres@localhost:5432/harvestry_dev}}"

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}FRP-05 TimescaleDB Validation${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""

# Results tracking
RESULTS_FILE="$(pwd)/logs/frp05-day0-timescaledb-results.txt"
mkdir -p "$(pwd)/logs"
echo "TimescaleDB Validation Results - $(date)" > "$RESULTS_FILE"
echo "Connection: ${DB_CONN}" >> "$RESULTS_FILE"
echo "" >> "$RESULTS_FILE"

# Helper function to run SQL and capture output
run_sql() {
    local sql="$1"
    local description="$2"
    echo -e "${BLUE}Testing: ${description}${NC}"
    if psql "$DB_CONN" -c "$sql" >> "$RESULTS_FILE" 2>&1; then
        echo -e "${GREEN}✓ PASS: ${description}${NC}"
        echo "PASS: ${description}" >> "$RESULTS_FILE"
        return 0
    else
        echo -e "${RED}✗ FAIL: ${description}${NC}"
        echo "FAIL: ${description}" >> "$RESULTS_FILE"
        return 1
    fi
}

# Track overall status
PASSED=0
FAILED=0
WARNINGS=0

echo -e "${YELLOW}Phase 1: Extension Validation${NC}"
echo "-----------------------------------"
echo ""

# Test 1: Check PostgreSQL version
echo -e "${BLUE}Checking PostgreSQL version...${NC}"
if PG_VERSION=$(psql "$DB_CONN" -t -c "SELECT version();" 2>/dev/null); then
    echo -e "${GREEN}✓ PostgreSQL Connected${NC}"
    echo "PostgreSQL Version: $PG_VERSION" >> "$RESULTS_FILE"
    ((PASSED++))
else
    echo -e "${RED}✗ Cannot connect to database${NC}"
    echo "FAIL: Cannot connect to database" >> "$RESULTS_FILE"
    ((FAILED++))
    exit 1
fi

# Test 2: Check if TimescaleDB extension exists
echo -e "${BLUE}Checking TimescaleDB availability...${NC}"
if TSDB_AVAILABLE=$(psql "$DB_CONN" -t -c "SELECT 1 FROM pg_available_extensions WHERE name = 'timescaledb';" 2>/dev/null | xargs); then
    if [ "$TSDB_AVAILABLE" = "1" ]; then
        echo -e "${GREEN}✓ TimescaleDB extension is available${NC}"
        echo "PASS: TimescaleDB extension available" >> "$RESULTS_FILE"
        ((PASSED++))
    else
        echo -e "${RED}✗ TimescaleDB extension not available${NC}"
        echo "FAIL: TimescaleDB extension not available" >> "$RESULTS_FILE"
        echo -e "${YELLOW}Action: Install TimescaleDB extension on database server${NC}"
        ((FAILED++))
        exit 1
    fi
fi

# Test 3: Enable TimescaleDB extension
echo -e "${BLUE}Enabling TimescaleDB extension...${NC}"
if psql "$DB_CONN" -c "CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;" >> "$RESULTS_FILE" 2>&1; then
    TSDB_VERSION=$(psql "$DB_CONN" -t -c "SELECT extversion FROM pg_extension WHERE extname = 'timescaledb';" 2>/dev/null | xargs)
    echo -e "${GREEN}✓ TimescaleDB extension enabled (version: $TSDB_VERSION)${NC}"
    echo "PASS: TimescaleDB enabled - version $TSDB_VERSION" >> "$RESULTS_FILE"
    ((PASSED++))
else
    echo -e "${RED}✗ Cannot enable TimescaleDB extension${NC}"
    echo "FAIL: Cannot enable TimescaleDB extension" >> "$RESULTS_FILE"
    echo -e "${YELLOW}Action: Check database permissions (may require superuser)${NC}"
    ((FAILED++))
    exit 1
fi

echo ""
echo -e "${YELLOW}Phase 2: Hypertable Testing${NC}"
echo "-----------------------------------"
echo ""

# Test 4: Create test table
echo -e "${BLUE}Creating test table...${NC}"
psql "$DB_CONN" -c "DROP TABLE IF EXISTS frp05_test_hypertable CASCADE;" > /dev/null 2>&1
if run_sql "CREATE TABLE frp05_test_hypertable (time TIMESTAMPTZ NOT NULL, sensor_id UUID NOT NULL, value DOUBLE PRECISION, metadata JSONB);" "Create test table"; then
    ((PASSED++))
else
    ((FAILED++))
fi

# Test 5: Convert to hypertable
echo -e "${BLUE}Converting to hypertable...${NC}"
if run_sql "SELECT create_hypertable('frp05_test_hypertable', 'time', chunk_time_interval => INTERVAL '1 day', if_not_exists => TRUE);" "Convert to hypertable"; then
    ((PASSED++))
else
    ((FAILED++))
fi

# Test 6: Verify hypertable creation
echo -e "${BLUE}Verifying hypertable...${NC}"
HYPERTABLE_CHECK=$(psql "$DB_CONN" -t -c "SELECT COUNT(*) FROM timescaledb_information.hypertables WHERE hypertable_name = 'frp05_test_hypertable';" 2>/dev/null | xargs)
if [ "$HYPERTABLE_CHECK" = "1" ]; then
    echo -e "${GREEN}✓ Hypertable verified${NC}"
    echo "PASS: Hypertable verified" >> "$RESULTS_FILE"
    ((PASSED++))
else
    echo -e "${RED}✗ Hypertable not found${NC}"
    echo "FAIL: Hypertable not found" >> "$RESULTS_FILE"
    ((FAILED++))
fi

# Test 7: Insert test data
echo -e "${BLUE}Inserting test data...${NC}"
if run_sql "INSERT INTO frp05_test_hypertable (time, sensor_id, value) VALUES (NOW(), gen_random_uuid(), 42.0), (NOW() - INTERVAL '1 hour', gen_random_uuid(), 43.5), (NOW() - INTERVAL '2 hours', gen_random_uuid(), 41.2);" "Insert test data"; then
    ((PASSED++))
else
    ((FAILED++))
fi

# Test 8: Query test data
echo -e "${BLUE}Querying test data...${NC}"
if run_sql "SELECT COUNT(*) FROM frp05_test_hypertable;" "Query test data"; then
    ((PASSED++))
else
    ((FAILED++))
fi

echo ""
echo -e "${YELLOW}Phase 3: Compression Policy Testing${NC}"
echo "-----------------------------------"
echo ""

# Test 9: Add compression policy
echo -e "${BLUE}Adding compression policy...${NC}"
if run_sql "ALTER TABLE frp05_test_hypertable SET (timescaledb.compress, timescaledb.compress_segmentby = 'sensor_id', timescaledb.compress_orderby = 'time DESC');" "Enable compression"; then
    ((PASSED++))
else
    echo -e "${YELLOW}⚠ Compression configuration failed${NC}"
    echo "WARNING: Compression may require elevated privileges" >> "$RESULTS_FILE"
    ((WARNINGS++))
fi

# Test 10: Add compression policy job
echo -e "${BLUE}Adding compression policy job...${NC}"
if run_sql "SELECT add_compression_policy('frp05_test_hypertable', compress_after => INTERVAL '1 hour', if_not_exists => TRUE);" "Add compression policy"; then
    echo -e "${GREEN}✓ Compression policy added${NC}"
    echo "PASS: Compression policy added" >> "$RESULTS_FILE"
    ((PASSED++))
else
    echo -e "${YELLOW}⚠ Compression policy failed (may require manual compression)${NC}"
    echo "WARNING: Compression policy requires background job privileges" >> "$RESULTS_FILE"
    ((WARNINGS++))
fi

echo ""
echo -e "${YELLOW}Phase 4: Retention Policy Testing${NC}"
echo "-----------------------------------"
echo ""

# Test 11: Add retention policy
echo -e "${BLUE}Adding retention policy...${NC}"
if run_sql "SELECT add_retention_policy('frp05_test_hypertable', drop_after => INTERVAL '7 days', if_not_exists => TRUE);" "Add retention policy"; then
    echo -e "${GREEN}✓ Retention policy added${NC}"
    echo "PASS: Retention policy added" >> "$RESULTS_FILE"
    ((PASSED++))
else
    echo -e "${YELLOW}⚠ Retention policy failed (manual cleanup may be needed)${NC}"
    echo "WARNING: Retention policy requires background job privileges" >> "$RESULTS_FILE"
    ((WARNINGS++))
fi

echo ""
echo -e "${YELLOW}Phase 5: Continuous Aggregate Testing${NC}"
echo "-----------------------------------"
echo ""

# Test 12: Create continuous aggregate
echo -e "${BLUE}Creating continuous aggregate...${NC}"
psql "$DB_CONN" -c "DROP MATERIALIZED VIEW IF EXISTS frp05_test_agg CASCADE;" > /dev/null 2>&1
if run_sql "CREATE MATERIALIZED VIEW frp05_test_agg WITH (timescaledb.continuous) AS SELECT time_bucket('5 minutes', time) AS bucket, sensor_id, AVG(value) as avg_value, COUNT(*) as sample_count FROM frp05_test_hypertable GROUP BY bucket, sensor_id;" "Create continuous aggregate"; then
    ((PASSED++))
else
    ((FAILED++))
fi

# Test 13: Add refresh policy
echo -e "${BLUE}Adding refresh policy...${NC}"
if run_sql "SELECT add_continuous_aggregate_policy('frp05_test_agg', start_offset => INTERVAL '1 hour', end_offset => INTERVAL '1 minute', schedule_interval => INTERVAL '5 minutes', if_not_exists => TRUE);" "Add refresh policy"; then
    echo -e "${GREEN}✓ Refresh policy added${NC}"
    echo "PASS: Refresh policy added" >> "$RESULTS_FILE"
    ((PASSED++))
else
    echo -e "${YELLOW}⚠ Refresh policy failed${NC}"
    echo "WARNING: Refresh policy may require manual refresh" >> "$RESULTS_FILE"
    ((WARNINGS++))
fi

# Test 14: Query continuous aggregate
echo -e "${BLUE}Querying continuous aggregate...${NC}"
if run_sql "SELECT COUNT(*) FROM frp05_test_agg;" "Query continuous aggregate"; then
    ((PASSED++))
else
    ((FAILED++))
fi

echo ""
echo -e "${YELLOW}Phase 6: Cleanup${NC}"
echo "-----------------------------------"
echo ""

# Cleanup test objects
echo -e "${BLUE}Cleaning up test objects...${NC}"
psql "$DB_CONN" -c "DROP MATERIALIZED VIEW IF EXISTS frp05_test_agg CASCADE;" > /dev/null 2>&1
psql "$DB_CONN" -c "DROP TABLE IF EXISTS frp05_test_hypertable CASCADE;" > /dev/null 2>&1
echo -e "${GREEN}✓ Cleanup complete${NC}"

echo ""
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}Validation Results${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo -e "Passed:   ${GREEN}${PASSED}${NC}"
echo -e "Failed:   ${RED}${FAILED}${NC}"
echo -e "Warnings: ${YELLOW}${WARNINGS}${NC}"
echo ""

# Write summary to results file
echo "" >> "$RESULTS_FILE"
echo "========================================" >> "$RESULTS_FILE"
echo "SUMMARY" >> "$RESULTS_FILE"
echo "========================================" >> "$RESULTS_FILE"
echo "Passed: $PASSED" >> "$RESULTS_FILE"
echo "Failed: $FAILED" >> "$RESULTS_FILE"
echo "Warnings: $WARNINGS" >> "$RESULTS_FILE"
echo "" >> "$RESULTS_FILE"

if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}✓ All critical tests passed!${NC}"
    if [ $WARNINGS -gt 0 ]; then
        echo -e "${YELLOW}⚠ Some warnings encountered - review results file${NC}"
        echo "RESULT: PASS WITH WARNINGS" >> "$RESULTS_FILE"
        echo ""
        echo -e "${YELLOW}Recommendation: FRP-05 can proceed with manual fallbacks for warnings${NC}"
    else
        echo "RESULT: FULL PASS" >> "$RESULTS_FILE"
        echo ""
        echo -e "${GREEN}Recommendation: FRP-05 is GO for execution${NC}"
    fi
    echo ""
    echo -e "Results saved to: ${BLUE}${RESULTS_FILE}${NC}"
    exit 0
else
    echo -e "${RED}✗ Critical failures detected${NC}"
    echo "RESULT: FAIL" >> "$RESULTS_FILE"
    echo ""
    echo -e "${RED}Recommendation: Fix failures before proceeding with FRP-05${NC}"
    echo ""
    echo -e "Results saved to: ${BLUE}${RESULTS_FILE}${NC}"
    exit 1
fi

