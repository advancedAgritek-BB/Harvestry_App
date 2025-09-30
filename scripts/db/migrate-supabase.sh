#!/bin/bash
# Supabase Migration Script for Harvestry ERP
# Runs all pending migrations in order

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Harvestry ERP - Supabase Migrations${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Load environment variables safely
if [ -f .env.local ]; then
    set -a
    # Source while filtering comments
    while IFS= read -r line || [ -n "$line" ]; do
        [[ "$line" =~ ^[[:space:]]*# ]] && continue
        [[ -z "${line// }" ]] && continue
        [[ "$line" =~ ^([A-Za-z_][A-Za-z0-9_]*)=(.*)$ ]] && eval "export $line"
    done < .env.local
    set +a
    echo -e "${GREEN}✓ Loaded .env.local${NC}"
else
    echo -e "${RED}✗ Error: .env.local not found!${NC}"
    echo "Copy .env.template to .env.local and fill in your Supabase credentials."
    exit 1
fi

# Check if DATABASE_URL_DIRECT is set
if [ -z "$DATABASE_URL_DIRECT" ]; then
    echo -e "${RED}✗ Error: DATABASE_URL_DIRECT not set in .env.local${NC}"
    echo "Use the direct connection string for migrations (not the pooler)"
    exit 1
fi

echo -e "${BLUE}Database: ${DATABASE_URL_DIRECT}${NC}"
echo ""

# Function to run a migration file
run_migration() {
    local file=$1
    local name=$(basename "$file")
    
    echo -e "${YELLOW}► Running: ${name}${NC}"
    
    if psql "$DATABASE_URL_DIRECT" -f "$file" > /tmp/migration_output.log 2>&1; then
        echo -e "${GREEN}✓ Success: ${name}${NC}"
        return 0
    else
        echo -e "${RED}✗ Failed: ${name}${NC}"
        echo -e "${RED}Error output:${NC}"
        cat /tmp/migration_output.log
        exit 1
    fi
}

# Test database connection
echo -e "${BLUE}Testing database connection...${NC}"
if psql "$DATABASE_URL_DIRECT" -c "SELECT version();" > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Database connection successful${NC}"
    echo ""
else
    echo -e "${RED}✗ Cannot connect to database${NC}"
    echo "Check your DATABASE_URL_DIRECT in .env.local"
    exit 1
fi

# Step 1: Baseline migrations
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}Step 1: Baseline Migrations${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

if [ -d "src/database/migrations/baseline" ]; then
    for file in src/database/migrations/baseline/*.sql; do
        if [ -f "$file" ]; then
            run_migration "$file"
        fi
    done
    echo ""
else
    echo -e "${YELLOW}⚠ No baseline migrations found${NC}"
    echo ""
fi

# Step 2: TimescaleDB migrations
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}Step 2: TimescaleDB Migrations${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

if [ -d "src/database/migrations/timescale" ]; then
    for file in src/database/migrations/timescale/*.sql; do
        if [ -f "$file" ]; then
            run_migration "$file"
        fi
    done
    echo ""
else
    echo -e "${YELLOW}⚠ No TimescaleDB migrations found${NC}"
    echo ""
fi

# Step 3: FRP-specific migrations (when created)
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}Step 3: FRP Migrations${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

frp_found=false

for frp_dir in src/database/migrations/frp*; do
    if [ -d "$frp_dir" ]; then
        frp_found=true
        frp_name=$(basename "$frp_dir")
        echo -e "${BLUE}► ${frp_name}${NC}"
        
        for file in "$frp_dir"/*.sql; do
            if [ -f "$file" ]; then
                run_migration "$file"
            fi
        done
        echo ""
    fi
done

if [ "$frp_found" = false ]; then
    echo -e "${YELLOW}⚠ No FRP migrations found yet${NC}"
    echo "  FRP migrations will be created during Track B implementation"
    echo ""
fi

# Success summary
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}✓ All migrations completed successfully!${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""

# Verify database setup
echo -e "${BLUE}Verifying database setup...${NC}"
echo ""

# Check baseline tables
baseline_check=$(psql "$DATABASE_URL_DIRECT" -t -c "
SELECT 
    CASE 
        WHEN COUNT(*) >= 2 THEN '✓ Baseline tables created (audit_trail, outbox)'
        ELSE '✗ Missing baseline tables'
    END
FROM pg_tables 
WHERE schemaname = 'public' 
AND tablename IN ('audit_trail', 'outbox');
")
echo -e "${GREEN}${baseline_check}${NC}"

# Check hypertables
hypertable_check=$(psql "$DATABASE_URL_DIRECT" -t -c "
SELECT 
    CASE 
        WHEN COUNT(*) >= 1 THEN '✓ Hypertables created (' || COUNT(*)::text || ' total)'
        ELSE '⚠ No hypertables created yet'
    END
FROM timescaledb_information.hypertables;
")
echo -e "${GREEN}${hypertable_check}${NC}"

# Check RLS policies
rls_check=$(psql "$DATABASE_URL_DIRECT" -t -c "
SELECT 
    CASE 
        WHEN COUNT(*) >= 1 THEN '✓ RLS policies configured (' || COUNT(*)::text || ' policies)'
        ELSE '⚠ No RLS policies found yet'
    END
FROM pg_policies
WHERE schemaname = 'public';
")
echo -e "${GREEN}${rls_check}${NC}"

# Check functions
function_check=$(psql "$DATABASE_URL_DIRECT" -t -c "
SELECT 
    CASE 
        WHEN COUNT(*) >= 2 THEN '✓ Functions created (' || COUNT(*)::text || ' functions)'
        ELSE '⚠ Missing functions'
    END
FROM pg_proc p
JOIN pg_namespace n ON p.pronamespace = n.oid
WHERE n.nspname = 'public'
AND p.proname IN ('log_audit_event', 'compute_audit_hash');
")
echo -e "${GREEN}${function_check}${NC}"

echo ""
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}Database ready for Track B development!${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""

echo -e "${BLUE}Next steps:${NC}"
echo "  1. Verify setup in Supabase Dashboard"
echo "  2. Start FRP-01 (Identity, Roles, RLS/ABAC)"
echo "  3. Run 'supabase gen types typescript' to generate types for frontend"
echo ""
