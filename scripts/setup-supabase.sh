#!/bin/bash
# Quick Supabase Setup Script
# Run this after creating your Supabase project

set -e

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${GREEN}════════════════════════════════════════${NC}"
echo -e "${GREEN}  Harvestry ERP - Supabase Quick Setup  ${NC}"
echo -e "${GREEN}════════════════════════════════════════${NC}"
echo ""

# Step 1: Check prerequisites
echo -e "${BLUE}Step 1: Checking prerequisites...${NC}"

if ! command -v psql &> /dev/null; then
    echo -e "${YELLOW}⚠ psql not found. Install with: brew install postgresql${NC}"
    exit 1
fi

if ! command -v supabase &> /dev/null; then
    echo -e "${YELLOW}⚠ supabase CLI not found. Install with: brew install supabase/tap/supabase${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Prerequisites met${NC}"
echo ""

# Step 2: Create .env.local if not exists
echo -e "${BLUE}Step 2: Setting up environment...${NC}"

if [ ! -f .env.local ]; then
    echo "Creating .env.local from template..."
    
    cat > .env.local << 'EOF'
# Supabase Configuration
# Fill these in from your Supabase project settings

DATABASE_URL=postgresql://postgres.your_project_ref:[PASSWORD]@aws-0-us-west-1.pooler.supabase.com:6543/postgres
DATABASE_URL_DIRECT=postgresql://postgres:[PASSWORD]@db.your_project_ref.supabase.co:5432/postgres

SUPABASE_URL=https://your_project_ref.supabase.co
SUPABASE_ANON_KEY=your_anon_key_here
SUPABASE_SERVICE_ROLE_KEY=your_service_role_key_here

SUPABASE_PROJECT_ID=your_project_ref

NODE_ENV=development
LOG_LEVEL=Information
EOF
    
    echo -e "${GREEN}✓ Created .env.local${NC}"
    echo -e "${YELLOW}⚠ Edit .env.local with your Supabase credentials before continuing${NC}"
    echo ""
    echo "Get your credentials from:"
    echo "  https://app.supabase.com/project/_/settings/database"
    echo ""
    read -p "Press Enter after you've filled in .env.local..."
else
    echo -e "${GREEN}✓ .env.local already exists${NC}"
fi

echo ""

# Step 3: Test connection
echo -e "${BLUE}Step 3: Testing database connection...${NC}"

source .env.local

if [ -z "$DATABASE_URL_DIRECT" ]; then
    echo -e "${YELLOW}⚠ DATABASE_URL_DIRECT not set in .env.local${NC}"
    exit 1
fi

if psql "$DATABASE_URL_DIRECT" -c "SELECT version();" > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Database connection successful${NC}"
else
    echo -e "${YELLOW}✗ Cannot connect to database${NC}"
    echo "Check your credentials in .env.local"
    exit 1
fi

echo ""

# Step 4: Enable TimescaleDB
echo -e "${BLUE}Step 4: Enabling TimescaleDB...${NC}"

if psql "$DATABASE_URL_DIRECT" -v ON_ERROR_STOP=1 << 'SQL'
CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;
SELECT extname, extversion FROM pg_extension WHERE extname = 'timescaledb';
SQL
then
    echo -e "${GREEN}✓ TimescaleDB enabled${NC}"
else
    echo -e "${YELLOW}✗ Failed to enable TimescaleDB extension${NC}"
    exit 1
fi
echo ""

# Step 5: Run migrations
echo -e "${BLUE}Step 5: Running migrations...${NC}"

if [ -f scripts/db/migrate-supabase.sh ] && [ -r scripts/db/migrate-supabase.sh ]; then
    chmod +x scripts/db/migrate-supabase.sh
    ./scripts/db/migrate-supabase.sh
else
    echo -e "${YELLOW}✗ Error: scripts/db/migrate-supabase.sh not found or not readable${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}════════════════════════════════════════${NC}"
echo -e "${GREEN}  ✓ Supabase setup complete!            ${NC}"
echo -e "${GREEN}════════════════════════════════════════${NC}"
echo ""
echo -e "${BLUE}Next steps:${NC}"
echo "  1. Review setup in Supabase Dashboard"
echo "  2. Start FRP-01 implementation (Identity/RLS/ABAC)"
echo "  3. Run: supabase gen types typescript --local"
echo ""
