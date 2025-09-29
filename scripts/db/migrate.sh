#!/bin/bash
# Harvestry ERP - Database Migration Script
# Track A: Zero-downtime migration execution

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default values
DRY_RUN=false
ENVIRONMENT="development"
VERBOSE=false
CONNECTION_STRING=""

# Print usage
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Apply database migrations with zero-downtime strategy.

OPTIONS:
    -e, --env ENV           Environment (development, staging, production) [default: development]
    -d, --dry-run           Perform dry-run without applying changes
    -v, --verbose           Enable verbose output
    -c, --connection STRING Database connection string (overrides environment default)
    -h, --help              Show this help message

EXAMPLES:
    # Apply migrations to development
    $0

    # Dry-run for staging
    $0 --env staging --dry-run

    # Apply to production with verbose output
    $0 --env production --verbose

    # Use custom connection string
    $0 --connection "Host=localhost;Database=mydb;Username=user;Password=pass"

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
        -d|--dry-run)
            DRY_RUN=true
            shift
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -c|--connection)
            CONNECTION_STRING="$2"
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

# Set connection string based on environment if not provided
if [ -z "$CONNECTION_STRING" ]; then
    case $ENVIRONMENT in
        development)
            CONNECTION_STRING="Host=localhost;Port=5432;Database=harvestry_dev;Username=harvestry_user;Password=harvestry_dev_password"
            ;;
        staging)
            CONNECTION_STRING="${STAGING_DB_CONNECTION_STRING:-}"
            ;;
        production)
            CONNECTION_STRING="${PRODUCTION_DB_CONNECTION_STRING:-}"
            ;;
        *)
            echo -e "${RED}Error: Invalid environment: $ENVIRONMENT${NC}"
            exit 1
            ;;
    esac
fi

# Validate connection string
if [ -z "$CONNECTION_STRING" ]; then
    echo -e "${RED}Error: Connection string not set for environment: $ENVIRONMENT${NC}"
    echo "Set ${ENVIRONMENT^^}_DB_CONNECTION_STRING environment variable"
    exit 1
fi

echo -e "${GREEN}======================================${NC}"
echo -e "${GREEN}Harvestry ERP - Database Migration${NC}"
echo -e "${GREEN}======================================${NC}"
echo ""
echo "Environment:  $ENVIRONMENT"
echo "Dry Run:      $DRY_RUN"
echo "Verbose:      $VERBOSE"
echo ""

# Production safety check
if [ "$ENVIRONMENT" = "production" ] && [ "$DRY_RUN" = false ]; then
    echo -e "${YELLOW}WARNING: You are about to apply migrations to PRODUCTION!${NC}"
    read -p "Type 'APPLY PRODUCTION MIGRATIONS' to continue: " confirmation
    if [ "$confirmation" != "APPLY PRODUCTION MIGRATIONS" ]; then
        echo -e "${RED}Migration cancelled.${NC}"
        exit 1
    fi
fi

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: dotnet CLI not found. Please install .NET SDK.${NC}"
    exit 1
fi

# Check for pending migrations
echo -e "${YELLOW}Checking for pending migrations...${NC}"
PENDING_MIGRATIONS=$(dotnet ef migrations list \
    --project src/shared/data-access \
    --startup-project src/backend/services/gateway/API \
    --connection "$CONNECTION_STRING" \
    --no-build \
    2>&1 | grep "Pending" || true)

if [ -z "$PENDING_MIGRATIONS" ]; then
    echo -e "${GREEN}✓ No pending migrations. Database is up to date.${NC}"
    exit 0
fi

echo -e "${YELLOW}Pending migrations:${NC}"
echo "$PENDING_MIGRATIONS"
echo ""

# Perform dry-run if requested
if [ "$DRY_RUN" = true ]; then
    echo -e "${YELLOW}Performing dry-run (no changes will be applied)...${NC}"
    dotnet ef database update \
        --project src/shared/data-access \
        --startup-project src/backend/services/gateway/API \
        --connection "$CONNECTION_STRING" \
        --verbose \
        --dry-run
    
    echo -e "${GREEN}✓ Dry-run completed successfully.${NC}"
    exit 0
fi

# Apply migrations
echo -e "${YELLOW}Applying migrations...${NC}"

VERBOSE_FLAG=""
if [ "$VERBOSE" = true ]; then
    VERBOSE_FLAG="--verbose"
fi

if dotnet ef database update \
    --project src/shared/data-access \
    --startup-project src/backend/services/gateway/API \
    --connection "$CONNECTION_STRING" \
    $VERBOSE_FLAG; then
    
    echo ""
    echo -e "${GREEN}✓ Migrations applied successfully!${NC}"
    
    # Verify TimescaleDB extensions
    echo ""
    echo -e "${YELLOW}Verifying TimescaleDB installation...${NC}"
    
    # Extract connection details for psql
    HOST=$(echo "$CONNECTION_STRING" | sed -n 's/.*Host=\([^;]*\).*/\1/p')
    PORT=$(echo "$CONNECTION_STRING" | sed -n 's/.*Port=\([^;]*\).*/\1/p')
    DATABASE=$(echo "$CONNECTION_STRING" | sed -n 's/.*Database=\([^;]*\).*/\1/p')
    USERNAME=$(echo "$CONNECTION_STRING" | sed -n 's/.*Username=\([^;]*\).*/\1/p')
    
    export PGPASSWORD=$(echo "$CONNECTION_STRING" | sed -n 's/.*Password=\([^;]*\).*/\1/p')
    
    TIMESCALE_VERSION=$(psql -h "$HOST" -p "$PORT" -U "$USERNAME" -d "$DATABASE" -t -c "SELECT extversion FROM pg_extension WHERE extname = 'timescaledb';" 2>/dev/null || echo "")
    
    if [ -n "$TIMESCALE_VERSION" ]; then
        echo -e "${GREEN}✓ TimescaleDB v$TIMESCALE_VERSION is installed${NC}"
    else
        echo -e "${YELLOW}⚠ TimescaleDB extension not found (may not be required yet)${NC}"
    fi
    
    # List hypertables
    HYPERTABLES=$(psql -h "$HOST" -p "$PORT" -U "$USERNAME" -d "$DATABASE" -t -c "SELECT COUNT(*) FROM timescaledb_information.hypertables;" 2>/dev/null || echo "0")
    
    if [ "$HYPERTABLES" -gt 0 ]; then
        echo -e "${GREEN}✓ Found $HYPERTABLES hypertable(s)${NC}"
        psql -h "$HOST" -p "$PORT" -U "$USERNAME" -d "$DATABASE" -c "SELECT hypertable_name, num_chunks FROM timescaledb_information.hypertables;" 2>/dev/null || true
    fi
    
    unset PGPASSWORD
    
    echo ""
    echo -e "${GREEN}Migration completed successfully!${NC}"
    exit 0
else
    echo ""
    echo -e "${RED}✗ Migration failed!${NC}"
    echo ""
    echo -e "${YELLOW}Troubleshooting:${NC}"
    echo "1. Check database connection"
    echo "2. Verify database user has sufficient privileges"
    echo "3. Review error messages above"
    echo "4. Run with --verbose for detailed output"
    echo "5. Check runbooks: docs/ops/runbooks/Runbook_DB_Migration_Failure.md"
    exit 1
fi
