-- Migration: Create sensor_readings hypertable
-- Track A: Telemetry ingest with TimescaleDB
-- Author: Data & AI Squad
-- Date: 2025-09-29

-- ============================================================================
-- UP Migration
-- ============================================================================

BEGIN;

-- Create sensor_readings table
CREATE TABLE IF NOT EXISTS sensor_readings (
    stream_id UUID NOT NULL,
    ts TIMESTAMPTZ NOT NULL,
    value DOUBLE PRECISION NOT NULL,
    unit VARCHAR(50) NOT NULL,
    site_id UUID NOT NULL,
    device_id UUID,
    quality_code SMALLINT DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    PRIMARY KEY (stream_id, ts)
);

-- Convert to hypertable (1 day chunks)
SELECT create_hypertable(
    'sensor_readings',
    'ts',
    chunk_time_interval => INTERVAL '1 day',
    if_not_exists => TRUE
);

-- Add compression policy (compress chunks older than 7 days)
ALTER TABLE sensor_readings SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'stream_id,site_id',
    timescaledb.compress_orderby = 'ts DESC'
);

SELECT add_compression_policy('sensor_readings', INTERVAL '7 days');

-- Add retention policy (drop chunks older than 90 days)
SELECT add_retention_policy('sensor_readings', INTERVAL '90 days');

-- Create indexes for common query patterns
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_sensor_readings_site_id_ts 
    ON sensor_readings USING BRIN (site_id, ts);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_sensor_readings_device_id_ts 
    ON sensor_readings USING BRIN (device_id, ts) 
    WHERE device_id IS NOT NULL;

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_sensor_readings_stream_id 
    ON sensor_readings (stream_id);

-- Add RLS policies
ALTER TABLE sensor_readings ENABLE ROW LEVEL SECURITY;

-- Site-scoped access policy
CREATE POLICY sensor_readings_site_isolation ON sensor_readings
FOR ALL
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
    )
);

-- Service account bypass policy
CREATE POLICY sensor_readings_service_account ON sensor_readings
FOR ALL
USING (current_setting('app.user_role', TRUE) = 'service_account');

-- Add comments
COMMENT ON TABLE sensor_readings IS 'Time-series sensor data with TimescaleDB hypertable';
COMMENT ON COLUMN sensor_readings.stream_id IS 'Unique identifier for sensor stream';
COMMENT ON COLUMN sensor_readings.ts IS 'Timestamp of reading (partition key)';
COMMENT ON COLUMN sensor_readings.value IS 'Sensor reading value';
COMMENT ON COLUMN sensor_readings.unit IS 'Unit of measurement (e.g., celsius, ppm, vpd)';
COMMENT ON COLUMN sensor_readings.quality_code IS 'Data quality indicator (0=good, 1=suspect, 2=bad)';

COMMIT;

-- ============================================================================
-- DOWN Migration
-- ============================================================================

BEGIN;

-- Remove RLS policies
DROP POLICY IF EXISTS sensor_readings_site_isolation ON sensor_readings;
DROP POLICY IF EXISTS sensor_readings_service_account ON sensor_readings;

-- Remove retention and compression policies
SELECT remove_retention_policy('sensor_readings', if_exists => TRUE);
SELECT remove_compression_policy('sensor_readings', if_exists => TRUE);

-- Drop indexes
DROP INDEX CONCURRENTLY IF EXISTS ix_sensor_readings_site_id_ts;
DROP INDEX CONCURRENTLY IF EXISTS ix_sensor_readings_device_id_ts;
DROP INDEX CONCURRENTLY IF EXISTS ix_sensor_readings_stream_id;

-- Drop hypertable (cascades to chunks)
DROP TABLE IF EXISTS sensor_readings CASCADE;

COMMIT;
