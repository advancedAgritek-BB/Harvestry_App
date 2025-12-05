# Telemetry Integration Tests

## Local Setup

- Start TimescaleDB locally (example):
  - `docker run --platform linux/arm64/v8 -d --name tsdb -p 55432:5432 -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=harvestry_dev timescale/timescaledb:latest-pg14`
- Export connection string:
  - `export TELEMETRY_DB_CONNECTION=postgresql://postgres:postgres@localhost:55432/harvestry_dev`
- Run tests:
  - `dotnet test Harvestry.Telemetry.IntegrationTests.csproj`

## Optional: Isolated Temp Database per Run

Set env to create a fresh temporary database for each run (clean slate, drops on exit):

```bash
export TELEMETRY_TEST_CREATE_TEMP_DB=true
```

The harness converts URL strings to Npgsql format, creates a temp DB via the `postgres` admin connection, runs base schema migrations (001, 003, 005; GRANTs/RLS omitted locally), then executes tests.

## Notes

- Continuous aggregates (rollups) are validated in staging as part of the load/latency gates, not in local integration tests.
- The harness avoids RLS/GRANTs so it works with a default local DB.
- Realtime dispatcher is a no‑op for tests — signal delivery is covered by staging checks.

