#!/bin/bash
# Supabase Migration Script for Harvestry ERP
# Runs all pending migrations in order

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Load environment variables safely
if [ -f .env.local ]; then
    # Safe parsing that avoids command injection
    while IFS= read -r line || [ -n "$line" ]; do
        # Skip comments and empty lines
        [[ "$line" =~ ^[[:space:]]*# ]] && continue
        [[ -z "${line// }" ]] && continue
        
        # Validate key name (alphanumeric and underscore only)
        if [[ "$line" =~ ^([A-Za-z_][A-Za-z0-9_]*)=(.*)$ ]]; then
            key="${BASH_REMATCH[1]}"
            value="${BASH_REMATCH[2]}"
            # Strip optional surrounding quotes
            value="${value#\"}"
            value="${value%\"}"
            value="${value#\'}"
            value="${value%\'}"
            export "$key=$value"
        fi
    done < .env.local
else
    echo -e "${RED}Error: .env.local not found!${NC}"
    echo "Copy .env.template to .env.local and fill in your Supabase credentials."
    exit 1
fi

# Check if DATABASE_URL is set
if [ -z "$DATABASE_URL" ]; then
    echo -e "${RED}Error: DATABASE_URL not set in .env.local${NC}"
    exit 1
fi

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Harvestry ERP - Supabase Migrations${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Function to run a migration file
run_migration() {
    local file=$1
    local name=$(basename "$file")
    
    echo -e "${YELLOW}Running: ${name}${NC}"
    
    if psql "$DATABASE_URL" -f "$file" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Success: ${name}${NC}"
    else
        echo -e "${RED}✗ Failed: ${name}${NC}"
        exit 1
    fi
}

# Step 1: Baseline migrations
echo -e "${GREEN}Step 1: Baseline Migrations${NC}"
echo "---"

if [ -d "src/database/migrations/baseline" ]; then
    for file in src/database/migrations/baseline/*.sql; do
        if [ -f "$file" ]; then
            run_migration "$file"
        fi
    done
else
    echo -e "${YELLOW}No baseline migrations found${NC}"
fi

echo ""

# Step 2: TimescaleDB migrations
echo -e "${GREEN}Step 2: TimescaleDB Migrations${NC}"
echo "---"

if [ -d "src/database/migrations/timescale" ]; then
    for file in src/database/migrations/timescale/*.sql; do
        if [ -f "$file" ]; then
            run_migration "$file"
        fi
    done
else
    echo -e "${YELLOW}No TimescaleDB migrations found${NC}"
fi

echo ""

# Step 3: FRP-specific migrations (when created)
echo -e "${GREEN}Step 3: FRP Migrations${NC}"
echo "---"

for frp_dir in src/database/migrations/frp*; do
    if [ -d "$frp_dir" ]; then
        frp_name=$(basename "$frp_dir")
        echo -e "${YELLOW}FRP: ${frp_name}${NC}"
        
        for file in "$frp_dir"/*.sql; do
            if [ -f "$file" ]; then
                run_migration "$file"
            fi
        done
    fi
done

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}✓ All migrations completed successfully!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Verify key tables exist
echo -e "${YELLOW}Verifying database setup...${NC}"

psql "$DATABASE_URL" -t -c "
SELECT 
    CASE 
        WHEN COUNT(*) >= 2 THEN '✓ Baseline tables created'
        ELSE '✗ Missing baseline tables'
    END
FROM pg_tables 
WHERE schemaname = 'public' 
AND tablename IN ('audit_trail', 'outbox');
"

echo ""
echo -e "${GREEN}Database ready for Track B development!${NC}"
echo ""
echo "Note: TimescaleDB migrations are managed separately via Timescale Cloud."
echo "See docs/setup/HYBRID_DATABASE_ARCHITECTURE.md for details."
