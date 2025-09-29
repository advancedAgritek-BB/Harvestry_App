#!/bin/bash
# Harvestry ERP - Database Rollback Script
# Track A: Safe migration rollback

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default values
ENVIRONMENT="development"
TARGET_MIGRATION=""
CONNECTION_STRING=""

# Print usage
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Rollback database migrations safely.

OPTIONS:
    -e, --env ENV           Environment (development, staging, production) [default: development]
    -t, --to MIGRATION      Target migration name to rollback to
    -c, --connection STRING Database connection string (overrides environment default)
    -h, --help              Show this help message

EXAMPLES:
    # Rollback last migration in development
    $0

    # Rollback to specific migration in staging
    $0 --env staging --to 20250929120000_CreateSensorReadingsHypertable

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
        -t|--to)
            TARGET_MIGRATION="$2"
            shift 2
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
    exit 1
fi

echo -e "${YELLOW}======================================${NC}"
echo -e "${YELLOW}Harvestry ERP - Database Rollback${NC}"
echo -e "${YELLOW}======================================${NC}"
echo ""
echo "Environment:     $ENVIRONMENT"
echo "Target Migration: ${TARGET_MIGRATION:-[last migration]}"
echo ""

# Production safety check
if [ "$ENVIRONMENT" = "production" ]; then
    echo -e "${RED}WARNING: You are about to ROLLBACK migrations in PRODUCTION!${NC}"
    echo -e "${RED}This operation may result in DATA LOSS if not carefully planned.${NC}"
    read -p "Type 'ROLLBACK PRODUCTION' to continue: " confirmation
    if [ "$confirmation" != "ROLLBACK PRODUCTION" ]; then
        echo -e "${GREEN}Rollback cancelled.${NC}"
        exit 1
    fi
fi

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: dotnet CLI not found. Please install .NET SDK.${NC}"
    exit 1
fi

# List current migrations
echo -e "${YELLOW}Current applied migrations:${NC}"
dotnet ef migrations list \
    --project src/shared/data-access \
    --startup-project src/backend/services/gateway/API \
    --connection "$CONNECTION_STRING" \
    --no-build \
    2>&1 | grep "Applied" || true

echo ""

# Perform rollback
if [ -n "$TARGET_MIGRATION" ]; then
    echo -e "${YELLOW}Rolling back to migration: $TARGET_MIGRATION${NC}"
    TARGET_ARG="$TARGET_MIGRATION"
else
    echo -e "${YELLOW}Rolling back last migration...${NC}"
    TARGET_ARG="0"
fi

if dotnet ef database update "$TARGET_ARG" \
    --project src/shared/data-access \
    --startup-project src/backend/services/gateway/API \
    --connection "$CONNECTION_STRING" \
    --verbose; then
    
    echo ""
    echo -e "${GREEN}✓ Rollback completed successfully!${NC}"
    
    echo ""
    echo -e "${YELLOW}Remaining applied migrations:${NC}"
    dotnet ef migrations list \
        --project src/shared/data-access \
        --startup-project src/backend/services/gateway/API \
        --connection "$CONNECTION_STRING" \
        --no-build \
        2>&1 | grep "Applied" || echo "No migrations applied"
    
    exit 0
else
    echo ""
    echo -e "${RED}✗ Rollback failed!${NC}"
    echo ""
    echo -e "${YELLOW}Troubleshooting:${NC}"
    echo "1. Verify the target migration name is correct"
    echo "2. Check if the Down() method is properly implemented"
    echo "3. Review error messages above"
    echo "4. Consult runbooks: docs/ops/runbooks/Runbook_DB_Rollback_Failure.md"
    exit 1
fi
