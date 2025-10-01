#!/usr/bin/env bash
# Spin up an ephemeral PostgreSQL container, apply migrations, run tests, and clean up.

set -euo pipefail

if ! command -v docker >/dev/null 2>&1; then
    echo "❌ docker is required to run this script" >&2
    exit 1
fi

if ! command -v psql >/dev/null 2>&1; then
    echo "❌ psql (PostgreSQL client) is required to apply migrations" >&2
    exit 1
fi

CONTAINER_NAME=${CONTAINER_NAME:-harvestry-test-db}
POSTGRES_IMAGE=${POSTGRES_IMAGE:-postgres:15-alpine}
TEST_DB_PORT=${TEST_DB_PORT:-6543}
TEST_DB_NAME=${TEST_DB_NAME:-harvestry_identity_test}
TEST_DB_USER=${TEST_DB_USER:-postgres}
TEST_DB_PASSWORD=${TEST_DB_PASSWORD:-postgres}

CONNECTION_URI="postgresql://${TEST_DB_USER}:${TEST_DB_PASSWORD}@localhost:${TEST_DB_PORT}/${TEST_DB_NAME}?sslmode=disable"
CONNECTION_STRING="Host=localhost;Port=${TEST_DB_PORT};Database=${TEST_DB_NAME};Username=${TEST_DB_USER};Password=${TEST_DB_PASSWORD};SSL Mode=Disable;Trust Server Certificate=true;Include Error Detail=true"

cleanup() {
    docker stop "$CONTAINER_NAME" >/dev/null 2>&1 || true
}
trap cleanup EXIT

echo "🚀 Starting PostgreSQL test container (${POSTGRES_IMAGE})..."
docker run \
  --rm \
  -d \
  --name "$CONTAINER_NAME" \
  -e POSTGRES_DB="$TEST_DB_NAME" \
  -e POSTGRES_USER="$TEST_DB_USER" \
  -e POSTGRES_PASSWORD="$TEST_DB_PASSWORD" \
  -p "${TEST_DB_PORT}:5432" \
  "$POSTGRES_IMAGE" >/dev/null

echo "⏳ Waiting for database to become ready..."
until docker exec "$CONTAINER_NAME" pg_isready -U "$TEST_DB_USER" >/dev/null 2>&1; do
    sleep 1
    printf '.'
done
printf '\n'

echo "🔧 Applying migrations to ${TEST_DB_NAME}..."
apply_migrations() {
    local directory=$1
    if [ -d "$directory" ]; then
        find "$directory" -maxdepth 1 -type f -name '*.sql' | sort | while read -r file; do
            echo "  • $(basename "$file")"
            PGPASSWORD="$TEST_DB_PASSWORD" psql "$CONNECTION_URI" -v ON_ERROR_STOP=1 -f "$file" >/dev/null
        done
    fi
}

apply_migrations "src/database/migrations/baseline"
apply_migrations "src/database/migrations/frp01"
apply_migrations "src/database/migrations/frp02"

echo "✅ Migrations applied"

export IDENTITY_DB_CONNECTION="$CONNECTION_STRING"
export SPATIAL_DB_CONNECTION="$CONNECTION_STRING"

echo "🧪 Running tests..."
dotnet test Harvestry.sln "$@"

echo "🏁 Tests completed"
