#!/bin/bash

# =====================================================
# FRP05 Telemetry Migrations - Runner Script
# Description: Runs all migrations in order
# Author: AI Agent
# Date: 2025-10-02
# =====================================================

set -e  # Exit on error

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}FRP05 Telemetry Migrations${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Check for DATABASE_URL
if [ -z "$DATABASE_URL" ]; then
    echo -e "${RED}ERROR: DATABASE_URL environment variable is not set${NC}"
    echo ""
    echo "Example:"
    echo "  export DATABASE_URL=\"postgresql://postgres:password@localhost:5432/harvestry\""
    echo ""
    exit 1
fi

echo -e "${GREEN}✓${NC} Database URL configured"
echo ""

# Function to run a migration
run_migration() {
    local file=$1
    local description=$2
    
    echo -e "${YELLOW}Running:${NC} $file"
    echo -e "${YELLOW}Description:${NC} $description"
    echo ""
    
    if psql "$DATABASE_URL" -f "$SCRIPT_DIR/$file" -v ON_ERROR_STOP=1; then
        echo -e "${GREEN}✓ Success${NC}"
    else
        echo -e "${RED}✗ Failed${NC}"
        exit 1
    fi
    
    echo ""
    echo "----------------------------------------"
    echo ""
}

# Run migrations in order
echo "Starting migrations..."
echo ""

run_migration "001_initial_schema.sql" "Creating base tables and indexes"
run_migration "002_timescaledb_setup.sql" "Configuring TimescaleDB features"
run_migration "003_additional_indexes.sql" "Creating performance indexes"
run_migration "004_rls_policies.sql" "Setting up row-level security"

# Ask before running seed data
echo -e "${YELLOW}Do you want to run seed data migration? (y/N)${NC}"
read -r response
if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
    run_migration "005_seed_data.sql" "Creating test/development seed data"
else
    echo "Skipping seed data migration"
    echo ""
fi

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}All migrations completed successfully!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Show summary
echo "Summary:"
echo "  ✓ Base tables created"
echo "  ✓ TimescaleDB hypertables configured"
echo "  ✓ Compression and retention policies set"
echo "  ✓ Continuous aggregates created"
echo "  ✓ Performance indexes added"
echo "  ✓ Row-level security enabled"

if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
    echo "  ✓ Seed data inserted"
fi

echo ""
echo "Next steps:"
echo "  1. Verify schema: psql \$DATABASE_URL -c '\\dt'"
echo "  2. Check hypertables: psql \$DATABASE_URL -c \"SELECT * FROM timescaledb_information.hypertables;\""
echo "  3. Test RLS: psql \$DATABASE_URL -c \"SELECT * FROM test_rls_policies();\""
echo ""

