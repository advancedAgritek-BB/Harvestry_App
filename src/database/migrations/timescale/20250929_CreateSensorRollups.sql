-- Migration: Create continuous aggregates for sensor rollups
-- Track A: 1m, 5m, 1h aggregations for SLO monitoring
-- Author: Data & AI Squad
-- Date: 2025-09-29

-- ============================================================================
-- UP Migration: 1-minute rollups
-- ============================================================================

BEGIN;

CREATE MATERIALIZED VIEW IF NOT EXISTS sensor_rollups_1m
WITH (timescaledb.continuous) AS
SELECT 
    site_id,
    stream_id,
    time_bucket('1 minute', ts) AS bucket,
    AVG(value) AS avg_value,
    MIN(value) AS min_value,
    MAX(value) AS max_value,
    STDDEV(value) AS stddev_value,
    COUNT(*) AS sample_count,
    COUNT(*) FILTER (WHERE quality_code = 0) AS good_samples,
    COUNT(*) FILTER (WHERE quality_code > 0) AS bad_samples
FROM sensor_readings
GROUP BY site_id, stream_id, bucket
WITH NO DATA;

-- Add refresh policy (refresh every 1 minute, lag 1 minute)
SELECT add_continuous_aggregate_policy(
    'sensor_rollups_1m',
    start_offset => INTERVAL '2 hours',
    end_offset => INTERVAL '1 minute',
    schedule_interval => INTERVAL '1 minute'
);

-- Add compression for 1m rollups (compress after 30 days)
ALTER MATERIALIZED VIEW sensor_rollups_1m SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'stream_id,site_id',
    timescaledb.compress_orderby = 'bucket DESC'
);

SELECT add_compression_policy('sensor_rollups_1m', INTERVAL '30 days');

-- Add retention (drop after 180 days)
SELECT add_retention_policy('sensor_rollups_1m', INTERVAL '180 days');

-- Create indexes
CREATE INDEX IF NOT EXISTS ix_sensor_rollups_1m_site_bucket
    ON sensor_rollups_1m (site_id, bucket DESC);

CREATE INDEX IF NOT EXISTS ix_sensor_rollups_1m_stream_bucket
    ON sensor_rollups_1m (stream_id, bucket DESC);

COMMENT ON MATERIALIZED VIEW sensor_rollups_1m IS '1-minute sensor data rollups for real-time monitoring';

COMMIT;

-- ============================================================================
-- UP Migration: 5-minute rollups
-- ============================================================================

BEGIN;

CREATE MATERIALIZED VIEW IF NOT EXISTS sensor_rollups_5m
WITH (timescaledb.continuous) AS
SELECT 
    site_id,
    stream_id,
    time_bucket('5 minutes', ts) AS bucket,
    AVG(value) AS avg_value,
    MIN(value) AS min_value,
    MAX(value) AS max_value,
    STDDEV(value) AS stddev_value,
    COUNT(*) AS sample_count
FROM sensor_readings
GROUP BY site_id, stream_id, bucket
WITH NO DATA;

-- Add refresh policy
SELECT add_continuous_aggregate_policy(
    'sensor_rollups_5m',
    start_offset => INTERVAL '12 hours',
    end_offset => INTERVAL '5 minutes',
    schedule_interval => INTERVAL '5 minutes'
);

-- Add compression
ALTER MATERIALIZED VIEW sensor_rollups_5m SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'stream_id,site_id',
    timescaledb.compress_orderby = 'bucket DESC'
);

SELECT add_compression_policy('sensor_rollups_5m', INTERVAL '60 days');

-- Add retention
SELECT add_retention_policy('sensor_rollups_5m', INTERVAL '365 days');

-- Create indexes
CREATE INDEX IF NOT EXISTS ix_sensor_rollups_5m_site_bucket
    ON sensor_rollups_5m (site_id, bucket DESC);

CREATE INDEX IF NOT EXISTS ix_sensor_rollups_5m_stream_bucket
    ON sensor_rollups_5m (stream_id, bucket DESC);

COMMENT ON MATERIALIZED VIEW sensor_rollups_5m IS '5-minute sensor data rollups for dashboards';

COMMIT;

-- ============================================================================
-- UP Migration: 1-hour rollups
-- ============================================================================

BEGIN;

CREATE MATERIALIZED VIEW IF NOT EXISTS sensor_rollups_1h
WITH (timescaledb.continuous) AS
SELECT 
    site_id,
    stream_id,
    time_bucket('1 hour', ts) AS bucket,
    AVG(value) AS avg_value,
    MIN(value) AS min_value,
    MAX(value) AS max_value,
    STDDEV(value) AS stddev_value,
    COUNT(*) AS sample_count
FROM sensor_readings
GROUP BY site_id, stream_id, bucket
WITH NO DATA;

-- Add refresh policy
SELECT add_continuous_aggregate_policy(
    'sensor_rollups_1h',
    start_offset => INTERVAL '1 day',
    end_offset => INTERVAL '1 hour',
    schedule_interval => INTERVAL '1 hour'
);

-- Add compression
ALTER MATERIALIZED VIEW sensor_rollups_1h SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'stream_id,site_id',
    timescaledb.compress_orderby = 'bucket DESC'
);

SELECT add_compression_policy('sensor_rollups_1h', INTERVAL '90 days');

-- Add retention (keep for 2 years)
SELECT add_retention_policy('sensor_rollups_1h', INTERVAL '730 days');

-- Create indexes
CREATE INDEX IF NOT EXISTS ix_sensor_rollups_1h_site_bucket
    ON sensor_rollups_1h (site_id, bucket DESC);

CREATE INDEX IF NOT EXISTS ix_sensor_rollups_1h_stream_bucket
    ON sensor_rollups_1h (stream_id, bucket DESC);

COMMENT ON MATERIALIZED VIEW sensor_rollups_1h IS '1-hour sensor data rollups for historical analysis';

COMMIT;

-- ============================================================================
-- DOWN Migration
-- ============================================================================

BEGIN;

-- Remove 1h rollups
SELECT remove_retention_policy('sensor_rollups_1h', if_exists => TRUE);
SELECT remove_compression_policy('sensor_rollups_1h', if_exists => TRUE);
SELECT remove_continuous_aggregate_policy('sensor_rollups_1h', if_exists => TRUE);
DROP MATERIALIZED VIEW IF EXISTS sensor_rollups_1h CASCADE;

-- Remove 5m rollups
SELECT remove_retention_policy('sensor_rollups_5m', if_exists => TRUE);
SELECT remove_compression_policy('sensor_rollups_5m', if_exists => TRUE);
SELECT remove_continuous_aggregate_policy('sensor_rollups_5m', if_exists => TRUE);
DROP MATERIALIZED VIEW IF EXISTS sensor_rollups_5m CASCADE;

-- Remove 1m rollups
SELECT remove_retention_policy('sensor_rollups_1m', if_exists => TRUE);
SELECT remove_compression_policy('sensor_rollups_1m', if_exists => TRUE);
SELECT remove_continuous_aggregate_policy('sensor_rollups_1m', if_exists => TRUE);
DROP MATERIALIZED VIEW IF EXISTS sensor_rollups_1m CASCADE;

COMMIT;
