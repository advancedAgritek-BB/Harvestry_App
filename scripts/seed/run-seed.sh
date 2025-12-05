#!/usr/bin/env bash

# ============================================================================
# Harvestry Seed Data Runner
# 
# Usage:
#   ./run-seed.sh [environment] [--force]
#
# Environments:
#   dev      - Full pilot site data (default)
#   staging  - Full pilot site data
#   test     - Minimal test fixtures for CI
#
# Options:
#   --force  - Skip confirmation prompt
#
# Example:
#   ./run-seed.sh dev
#   ./run-seed.sh test --force
# ============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENV="${1:-dev}"
FORCE="${2:-}"

# Color output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Determine which seed file to use
get_seed_file() {
    case "$ENV" in
        dev|development|staging)
            echo "${SCRIPT_DIR}/seed-pilot-site.sql"
            ;;
        test|ci)
            echo "${SCRIPT_DIR}/seed-test-fixtures.sql"
            ;;
        *)
            log_error "Unknown environment: $ENV"
            log_info "Valid environments: dev, staging, test"
            exit 1
            ;;
    esac
}

# Get database connection string from environment or config
get_connection_string() {
    # Check environment variables in order of precedence
    if [[ -n "${DATABASE_URL:-}" ]]; then
        echo "$DATABASE_URL"
    elif [[ -n "${HARVESTRY_DB_CONNECTION:-}" ]]; then
        echo "$HARVESTRY_DB_CONNECTION"
    elif [[ -n "${IDENTITY_DB_CONNECTION:-}" ]]; then
        echo "$IDENTITY_DB_CONNECTION"
    else
        # Default to local development database
        echo "postgresql://postgres:postgres@localhost:5432/harvestry"
    fi
}

# Validate prerequisites
check_prerequisites() {
    if ! command -v psql &> /dev/null; then
        log_error "psql is not installed or not in PATH"
        log_info "Install PostgreSQL client: brew install postgresql (macOS) or apt-get install postgresql-client (Linux)"
        exit 1
    fi
}

# Confirm before running (unless --force)
confirm_execution() {
    if [[ "$FORCE" == "--force" ]]; then
        return 0
    fi
    
    log_warn "This will seed the database with ${ENV} data."
    log_warn "Existing data with matching IDs will be updated."
    read -p "Continue? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        log_info "Aborted."
        exit 0
    fi
}

# Run the seed script
run_seed() {
    local seed_file
    seed_file=$(get_seed_file)
    
    if [[ ! -f "$seed_file" ]]; then
        log_error "Seed file not found: $seed_file"
        exit 1
    fi
    
    local conn_string
    conn_string=$(get_connection_string)
    
    log_info "Environment: $ENV"
    log_info "Seed file: $(basename "$seed_file")"
    log_info "Connecting to database..."
    
    # Run psql with the seed file
    if psql "$conn_string" -f "$seed_file"; then
        log_success "Seed data applied successfully!"
    else
        log_error "Failed to apply seed data"
        exit 1
    fi
}

# Verify seed was applied
verify_seed() {
    local conn_string
    conn_string=$(get_connection_string)
    
    log_info "Verifying seed data..."
    
    # Quick verification query
    local count
    count=$(psql "$conn_string" -t -c "SELECT COUNT(*) FROM organizations;" 2>/dev/null | tr -d ' ')
    
    if [[ "$count" -gt 0 ]]; then
        log_success "Verified: $count organization(s) in database"
    else
        log_warn "No organizations found - seed may have failed"
    fi
    
    # Show summary for dev/staging
    if [[ "$ENV" != "test" && "$ENV" != "ci" ]]; then
        echo ""
        log_info "Database Summary:"
        psql "$conn_string" -t -c "
            SELECT 'Organizations: ' || COUNT(*) FROM organizations
            UNION ALL
            SELECT 'Sites: ' || COUNT(*) FROM sites
            UNION ALL
            SELECT 'Users: ' || COUNT(*) FROM users
            UNION ALL
            SELECT 'Badges: ' || COUNT(*) FROM badges
            UNION ALL
            SELECT 'Rooms: ' || COUNT(*) FROM rooms;
        " 2>/dev/null || true
    fi
}

# Main execution
main() {
    echo ""
    echo "=========================================="
    echo "  Harvestry Database Seed Runner"
    echo "=========================================="
    echo ""
    
    check_prerequisites
    confirm_execution
    run_seed
    verify_seed
    
    echo ""
    log_success "Done!"
}

main "$@"

