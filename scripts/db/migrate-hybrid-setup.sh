#!/bin/bash
# Hybrid Database Setup - Supabase (Relational) + Timescale Cloud (Time-Series)
# Run this for complete Track B database setup

set -e

# Create secure temp file and ensure cleanup
LOG_FILE="$(mktemp)"
chmod 600 "$LOG_FILE"
trap 'rm -f "$LOG_FILE"' EXIT

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${GREEN}════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}  Harvestry ERP - Hybrid Database Setup            ${NC}"
echo -e "${GREEN}  Supabase (Relational) + Timescale Cloud (Time-Series)${NC}"
echo -e "${GREEN}════════════════════════════════════════════════════${NC}"
echo ""

# Load environment safely
if [ -f .env.local ]; then
    set -a
    # Source while filtering comments
    while IFS= read -r line || [ -n "$line" ]; do
        [[ "$line" =~ ^[[:space:]]*# ]] && continue
        [[ -z "${line// }" ]] && continue
        if [[ "$line" =~ ^([A-Za-z_][A-Za-z0-9_]*)=(.*)$ ]]; then
            key="${BASH_REMATCH[1]}"
            value="${BASH_REMATCH[2]}"
            # Strip optional surrounding quotes
            value="${value#\"}"
            value="${value%\"}"
            value="${value#\'}"
            value="${value%\'}"
            declare -gx "$key=$value"
        fi
    done < .env.local
    set +a
    echo -e "${GREEN}✓ Loaded .env.local${NC}"
else
    echo -e "${RED}✗ Error: .env.local not found!${NC}"
    exit 1
fi

echo ""

# Check required variables
if [ -z "$DATABASE_URL_DIRECT" ]; then
    echo -e "${RED}✗ DATABASE_URL_DIRECT not set (Supabase)${NC}"
    exit 1
fi

if [ -z "$DATABASE_URL_TIMESCALE" ]; then
    echo -e "${YELLOW}⚠ DATABASE_URL_TIMESCALE not set${NC}"
    echo "  Timescale Cloud setup will be skipped for now."
    echo "  You can run ./scripts/db/migrate-timescale-cloud.sh later"
    SKIP_TIMESCALE=true
else
    SKIP_TIMESCALE=false
fi

echo ""

# ============================================================================
# Part 1: Supabase (Relational)
# ============================================================================

echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}Part 1: Supabase (Relational Data)${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""

echo -e "${YELLOW}► Testing Supabase connection...${NC}"
if psql "$DATABASE_URL_DIRECT" -c "SELECT version();" > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Supabase connection successful${NC}"
else
    echo -e "${RED}✗ Cannot connect to Supabase${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}► Running Supabase relational migrations...${NC}"

# Baseline migrations
for file in src/database/migrations/baseline/*.sql; do
    if [ -f "$file" ]; then
        filename=$(basename "$file")
        echo "  ► $filename"
        if psql "$DATABASE_URL_DIRECT" -f "$file" > "$LOG_FILE" 2>&1; then
            echo -e "    ${GREEN}✓${NC}"
        else
            echo -e "    ${RED}✗ Failed${NC}"
            cat "$LOG_FILE"
            exit 1
        fi
    fi
done

echo ""
echo -e "${GREEN}✓ Supabase setup complete${NC}"
echo ""

# ============================================================================
# Part 2: Timescale Cloud (Time-Series)
# ============================================================================

if [ "$SKIP_TIMESCALE" = false ]; then
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${BLUE}Part 2: Timescale Cloud (Time-Series Data)${NC}"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""

    echo -e "${YELLOW}► Testing Timescale Cloud connection...${NC}"
    if psql "$DATABASE_URL_TIMESCALE" -c "SELECT version();" > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Timescale Cloud connection successful${NC}"
    else
        echo -e "${RED}✗ Cannot connect to Timescale Cloud${NC}"
        echo "Check DATABASE_URL_TIMESCALE in .env.local"
        exit 1
    fi

    echo ""
    echo -e "${YELLOW}► Verifying TimescaleDB extension...${NC}"
    psql "$DATABASE_URL_TIMESCALE" -c "SELECT extname, extversion FROM pg_extension WHERE extname = 'timescaledb';" | head -3

    echo ""
    echo -e "${YELLOW}► Running Timescale Cloud migrations...${NC}"

    # Timescale migrations
    for file in src/database/migrations/timescale/*.sql; do
        if [ -f "$file" ]; then
            filename=$(basename "$file")
            echo "  ► $filename"
            if psql "$DATABASE_URL_TIMESCALE" -f "$file" > "$LOG_FILE" 2>&1; then
                echo -e "    ${GREEN}✓${NC}"
            else
                echo -e "    ${RED}✗ Failed${NC}"
                cat "$LOG_FILE"
                exit 1
            fi
        fi
    done

    echo ""
    echo -e "${GREEN}✓ Timescale Cloud setup complete${NC}"
else
    echo -e "${YELLOW}⚠ Skipping Timescale Cloud setup${NC}"
    echo "  Run ./scripts/db/migrate-timescale-cloud.sh when ready"
fi

echo ""

# ============================================================================
# Verification
# ============================================================================

echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}Verification${NC}"
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""

echo -e "${YELLOW}Supabase (Relational):${NC}"
psql "$DATABASE_URL_DIRECT" -t -c "
SELECT 
    CASE 
        WHEN COUNT(*) >= 2 THEN '  ✓ Baseline tables created (audit_trail, outbox)'
        ELSE '  ✗ Missing baseline tables'
    END
FROM pg_tables 
WHERE schemaname = 'public' 
AND tablename IN ('audit_trail', 'outbox');
"

if [ "$SKIP_TIMESCALE" = false ]; then
    echo ""
    echo -e "${YELLOW}Timescale Cloud (Time-Series):${NC}"
    psql "$DATABASE_URL_TIMESCALE" -t -c "
    SELECT 
        CASE 
            WHEN COUNT(*) >= 1 THEN '  ✓ Hypertables created (' || COUNT(*)::text || ' total)'
            ELSE '  ⚠ No hypertables created yet'
        END
    FROM timescaledb_information.hypertables;
    "
fi

echo ""
echo -e "${GREEN}════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}✓ Hybrid database setup complete!${NC}"
echo -e "${GREEN}════════════════════════════════════════════════════${NC}"
echo ""

echo -e "${BLUE}Architecture Summary:${NC}"
echo "  • Supabase:       Relational data (Identity, Tasks, Inventory)"
echo "  • Timescale Cloud: Time-series data (Telemetry, Alerts)"
echo "  • ClickHouse:      OLAP analytics (optional, add later)"
echo ""

echo -e "${BLUE}Next steps:${NC}"
echo "  1. Start FRP-01 (Identity) - uses Supabase only"
echo "  2. Setup Timescale Cloud before FRP-05 (Telemetry)"
echo "  3. Review: docs/setup/HYBRID_DATABASE_ARCHITECTURE.md"
echo ""
