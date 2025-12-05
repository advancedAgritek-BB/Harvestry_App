# FRP05 Telemetry Service - Database Migrations

This directory contains SQL migrations for the FRP05 Telemetry Service.

## Migration Files

| # | File | Description | Status |
|---|------|-------------|--------|
| 001 | `001_initial_schema.sql` | Creates base tables and indexes | ✅ Ready |
| 002 | `002_timescaledb_setup.sql` | Configures TimescaleDB features | ✅ Ready |
| 003 | `003_additional_indexes.sql` | Performance optimization indexes | ✅ Ready |
| 004 | `004_rls_policies.sql` | Row-Level Security policies | ✅ Ready |
| 005 | `005_seed_data.sql` | Test/development seed data | ✅ Ready |

## Prerequisites

- **PostgreSQL 14+** with TimescaleDB extension
- **Supabase** or self-hosted PostgreSQL
- **psql** command-line tool or database client

## Quick Start

### Option 1: Run All Migrations (Recommended)

```bash
# Navigate to migrations directory
cd src/database/migrations/telemetry

# Run all migrations in order
psql $DATABASE_URL -f 001_initial_schema.sql
psql $DATABASE_URL -f 002_timescaledb_setup.sql
psql $DATABASE_URL -f 003_additional_indexes.sql
psql $DATABASE_URL -f 004_rls_policies.sql
psql $DATABASE_URL -f 005_seed_data.sql
```

### Option 2: Run Single Migration Script

```bash
# Create the all-in-one migration script
cat 001_initial_schema.sql \
    002_timescaledb_setup.sql \
    003_additional_indexes.sql \
    004_rls_policies.sql \
    005_seed_data.sql > full_migration.sql

# Run it
psql $DATABASE_URL -f full_migration.sql
```

### Option 3: Use the Helper Script

```bash
# Run the migration script
./run_migrations.sh
```

## Migration Details

### 001: Initial Schema

Creates all base tables:
- `sensor_streams` - Sensor configuration
- `sensor_readings` - Time-series data (will become hypertable)
- `alert_rules` - Alert rule definitions
- `alert_instances` - Fired alerts
- `ingestion_sessions` - Device connection tracking
- `ingestion_errors` - Error logging

**Verification:**
```sql
SELECT COUNT(*) FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name IN (
    'sensor_streams', 'sensor_readings', 'alert_rules',
    'alert_instances', 'ingestion_sessions', 'ingestion_errors'
  );
-- Should return: 6
```

### 002: TimescaleDB Setup

Configures TimescaleDB features:
- Converts `sensor_readings` to hypertable (1-day chunks)
- Enables compression (7-day policy)
- Sets retention (2-year policy)
- Creates 4 continuous aggregates (1min, 5min, 1hour, 1day)
- Configures automatic refresh policies

**Verification:**
```sql
-- Check hypertable
SELECT * FROM timescaledb_information.hypertables
WHERE hypertable_name = 'sensor_readings';

-- Check continuous aggregates
SELECT view_name
FROM timescaledb_information.continuous_aggregates;
-- Should return: sensor_readings_1min, sensor_readings_5min, sensor_readings_1hour, sensor_readings_1day

-- Get stats
SELECT * FROM get_sensor_readings_stats();
```

### 003: Additional Indexes

Creates performance optimization indexes:
- Location hierarchy indexes
- JSONB GIN indexes for metadata
- Partial indexes for recent data
- Array indexes for `stream_ids`
- Continuous aggregate indexes

**Verification:**
```sql
-- Count indexes
SELECT COUNT(*)
FROM pg_indexes
WHERE schemaname = 'public'
  AND tablename IN (
    'sensor_streams', 'sensor_readings', 'alert_rules',
    'alert_instances', 'ingestion_sessions', 'ingestion_errors'
  );

-- Get index usage stats
SELECT * FROM get_index_usage_stats();
```

### 004: RLS Policies

Implements Row-Level Security:
- Multi-tenant isolation by `site_id`
- Admin override capabilities
- Service role bypass for workers
- Separate policies for SELECT/INSERT/UPDATE/DELETE

**Verification:**
```sql
-- Check RLS status
SELECT * FROM test_rls_policies();

-- Should show RLS enabled on all 6 tables with policies
```

### 005: Seed Data

Creates test/development data:
- Test site and equipment
- 5 sensor streams (Temperature, Humidity, CO2, VPD, PAR)
- 3 alert rules
- ~4,320 sensor readings (24h history)
- Refreshes continuous aggregates

**Verification:**
```sql
-- Check data counts
SELECT
    (SELECT COUNT(*) FROM sensor_streams) as streams,
    (SELECT COUNT(*) FROM sensor_readings) as readings,
    (SELECT COUNT(*) FROM alert_rules) as rules;

-- Sample data
SELECT * FROM sensor_readings_1hour
ORDER BY bucket DESC LIMIT 5;
```

**Clear seed data:**
```sql
SELECT clear_seed_data();
```

## Database Connection

### Supabase

```bash
# Set environment variable
export DATABASE_URL="postgresql://postgres:[PASSWORD]@[PROJECT-REF].supabase.co:5432/postgres"

# Or use Supabase CLI
supabase db push
```

### Local PostgreSQL

```bash
export DATABASE_URL="postgresql://postgres:password@localhost:5432/harvestry"
```

## Rollback

To rollback migrations (⚠️ **CAUTION: Data loss!**):

```sql
-- Drop all telemetry tables (cascades to policies and indexes)
DROP TABLE IF EXISTS ingestion_errors CASCADE;
DROP TABLE IF EXISTS ingestion_sessions CASCADE;
DROP TABLE IF EXISTS alert_instances CASCADE;
DROP TABLE IF EXISTS alert_rules CASCADE;
DROP TABLE IF EXISTS sensor_readings CASCADE;
DROP TABLE IF EXISTS sensor_streams CASCADE;

-- Drop continuous aggregates
DROP MATERIALIZED VIEW IF EXISTS sensor_readings_1day CASCADE;
DROP MATERIALIZED VIEW IF EXISTS sensor_readings_1hour CASCADE;
DROP MATERIALIZED VIEW IF EXISTS sensor_readings_5min CASCADE;
DROP MATERIALIZED VIEW IF EXISTS sensor_readings_1min CASCADE;

-- Drop helper functions
DROP FUNCTION IF EXISTS get_sensor_readings_stats();
DROP FUNCTION IF EXISTS get_index_usage_stats();
DROP FUNCTION IF EXISTS test_rls_policies();
DROP FUNCTION IF EXISTS clear_seed_data();
DROP FUNCTION IF EXISTS reindex_telemetry_tables();
DROP FUNCTION IF EXISTS auth.get_user_site_ids();
DROP FUNCTION IF EXISTS auth.is_admin();
```

## Monitoring

### Check Hypertable Health

```sql
-- Chunk information
SELECT *
FROM timescaledb_information.chunks
WHERE hypertable_name = 'sensor_readings'
ORDER BY range_start DESC
LIMIT 10;

-- Compression stats
SELECT * FROM get_sensor_readings_stats();
```

### Check Continuous Aggregates

```sql
-- Aggregate refresh stats
SELECT *
FROM timescaledb_information.continuous_aggregate_stats
ORDER BY view_name;

-- Manual refresh (if needed)
CALL refresh_continuous_aggregate('sensor_readings_1min', NULL, NULL);
```

### Check Index Usage

```sql
SELECT * FROM get_index_usage_stats()
ORDER BY idx_scan DESC;
```

## Troubleshooting

### TimescaleDB Extension Not Found

```sql
-- Install extension (requires superuser)
CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;
```

### Hypertable Creation Failed

```bash
# Check TimescaleDB version
SELECT extversion FROM pg_extension WHERE extname = 'timescaledb';

# Should be 2.0+ for full feature support
```

### RLS Policies Blocking Access

```sql
-- Temporarily disable RLS (development only)
ALTER TABLE sensor_streams DISABLE ROW LEVEL SECURITY;

-- Check current user and roles
SELECT current_user, current_role;

-- Check user site access
SELECT * FROM auth.get_user_site_ids();
```

### Continuous Aggregates Not Updating

```sql
-- Check background jobs
SELECT * FROM timescaledb_information.jobs
WHERE proc_name LIKE '%continuous_aggregate%';

-- Manually refresh
CALL refresh_continuous_aggregate('sensor_readings_1min', NULL, NULL);
```

## Best Practices

1. **Always backup** before running migrations in production
2. **Test migrations** in development/staging first
3. **Run migrations during low-traffic periods**
4. **Monitor database performance** after migrations
5. **Keep seed data** separate from production migrations

## Performance Tuning

### Adjust Chunk Interval

```sql
-- For higher write volume, use smaller chunks
SELECT set_chunk_time_interval('sensor_readings', INTERVAL '12 hours');
```

### Adjust Compression Policy

```sql
-- Compress sooner to save space
SELECT remove_compression_policy('sensor_readings');
SELECT add_compression_policy('sensor_readings', INTERVAL '3 days');
```

### Adjust Retention Policy

```sql
-- Keep data longer
SELECT remove_retention_policy('sensor_readings');
SELECT add_retention_policy('sensor_readings', INTERVAL '5 years');
```

## Related Documentation

- [TimescaleDB Docs](https://docs.timescaledb.com/)
- [PostgreSQL RLS](https://www.postgresql.org/docs/current/ddl-rowsecurity.html)
- [Supabase Database](https://supabase.com/docs/guides/database)
- [FRP05 Implementation Plan](../../../../docs/FRP05_IMPLEMENTATION_PLAN.md)

## Support

For issues or questions:
1. Check logs: `\timing` and `\pset pager off` in psql
2. Review migration output messages
3. Check database permissions
4. Verify TimescaleDB version compatibility

---

**Last Updated:** October 2, 2025  
**Version:** 1.0.0  
**Status:** Production Ready ✅

