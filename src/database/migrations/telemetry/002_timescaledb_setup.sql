-- =====================================================
-- FRP05: Telemetry Service - TimescaleDB Setup
-- Migration: 002_timescaledb_setup.sql
-- Description: Configures TimescaleDB features for sensor_readings
-- Author: AI Agent
-- Date: 2025-10-02
-- Dependencies: 001_initial_schema.sql
-- =====================================================

-- =====================================================
-- HYPERTABLE CREATION
-- =====================================================

-- Convert sensor_readings to a hypertable
-- Partitioned by time (1 day chunks)
SELECT create_hypertable(
    'sensor_readings',
    'time',
    chunk_time_interval => INTERVAL '1 day',
    if_not_exists => TRUE
);

COMMENT ON TABLE sensor_readings IS 'TimescaleDB hypertable for sensor readings (1-day chunks)';

-- =====================================================
-- COMPRESSION POLICY
-- =====================================================

-- Enable compression on sensor_readings after 7 days
-- Compress older data to save storage space
ALTER TABLE sensor_readings SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'stream_id',
    timescaledb.compress_orderby = 'time DESC'
);

-- Add compression policy: compress chunks older than 7 days
SELECT add_compression_policy(
    'sensor_readings',
    INTERVAL '7 days',
    if_not_exists => TRUE
);

COMMENT ON TABLE sensor_readings IS 'TimescaleDB hypertable with compression (7-day policy)';

-- =====================================================
-- RETENTION POLICY
-- =====================================================

-- Add retention policy: drop chunks older than 2 years
-- Adjust retention period based on your requirements
SELECT add_retention_policy(
    'sensor_readings',
    INTERVAL '2 years',
    if_not_exists => TRUE
);

RAISE NOTICE 'Retention policy set: Data older than 2 years will be automatically dropped';

-- =====================================================
-- CONTINUOUS AGGREGATES (ROLLUPS)
-- =====================================================

-- 1-minute rollup (for recent data queries)
CREATE MATERIALIZED VIEW IF NOT EXISTS sensor_readings_1min
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('1 minute', time) AS bucket,
    stream_id,
    COUNT(*) as reading_count,
    AVG(value) as avg_value,
    MIN(value) as min_value,
    MAX(value) as max_value,
    STDDEV(value) as stddev_value,
    -- Quality metrics
    COUNT(*) FILTER (WHERE quality_code = 0) as good_count,
    COUNT(*) FILTER (WHERE (quality_code & 128) = 128) as uncertain_count,
    COUNT(*) FILTER (WHERE (quality_code & 192) = 192) as bad_count
FROM sensor_readings
GROUP BY bucket, stream_id
WITH NO DATA;

COMMENT ON MATERIALIZED VIEW sensor_readings_1min IS '1-minute aggregated sensor readings';

-- 5-minute rollup (for dashboard queries)
CREATE MATERIALIZED VIEW IF NOT EXISTS sensor_readings_5min
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('5 minutes', time) AS bucket,
    stream_id,
    COUNT(*) as reading_count,
    AVG(value) as avg_value,
    MIN(value) as min_value,
    MAX(value) as max_value,
    STDDEV(value) as stddev_value,
    COUNT(*) FILTER (WHERE quality_code = 0) as good_count,
    COUNT(*) FILTER (WHERE (quality_code & 128) = 128) as uncertain_count,
    COUNT(*) FILTER (WHERE (quality_code & 192) = 192) as bad_count
FROM sensor_readings
GROUP BY bucket, stream_id
WITH NO DATA;

COMMENT ON MATERIALIZED VIEW sensor_readings_5min IS '5-minute aggregated sensor readings';

-- 1-hour rollup (for historical analysis)
CREATE MATERIALIZED VIEW IF NOT EXISTS sensor_readings_1hour
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('1 hour', time) AS bucket,
    stream_id,
    COUNT(*) as reading_count,
    AVG(value) as avg_value,
    MIN(value) as min_value,
    MAX(value) as max_value,
    STDDEV(value) as stddev_value,
    COUNT(*) FILTER (WHERE quality_code = 0) as good_count,
    COUNT(*) FILTER (WHERE (quality_code & 128) = 128) as uncertain_count,
    COUNT(*) FILTER (WHERE (quality_code & 192) = 192) as bad_count
FROM sensor_readings
GROUP BY bucket, stream_id
WITH NO DATA;

COMMENT ON MATERIALIZED VIEW sensor_readings_1hour IS '1-hour aggregated sensor readings';

-- 1-day rollup (for long-term trends)
CREATE MATERIALIZED VIEW IF NOT EXISTS sensor_readings_1day
WITH (timescaledb.continuous) AS
SELECT
    time_bucket('1 day', time) AS bucket,
    stream_id,
    COUNT(*) as reading_count,
    AVG(value) as avg_value,
    MIN(value) as min_value,
    MAX(value) as max_value,
    STDDEV(value) as stddev_value,
    COUNT(*) FILTER (WHERE quality_code = 0) as good_count,
    COUNT(*) FILTER (WHERE (quality_code & 128) = 128) as uncertain_count,
    COUNT(*) FILTER (WHERE (quality_code & 192) = 192) as bad_count
FROM sensor_readings
GROUP BY bucket, stream_id
WITH NO DATA;

COMMENT ON MATERIALIZED VIEW sensor_readings_1day IS '1-day aggregated sensor readings';

-- =====================================================
-- CONTINUOUS AGGREGATE REFRESH POLICIES
-- =====================================================

-- Refresh 1-minute aggregates every 30 seconds
SELECT add_continuous_aggregate_policy(
    'sensor_readings_1min',
    start_offset => INTERVAL '2 hours',
    end_offset => INTERVAL '10 seconds',
    schedule_interval => INTERVAL '30 seconds',
    if_not_exists => TRUE
);

-- Refresh 5-minute aggregates every 2 minutes
SELECT add_continuous_aggregate_policy(
    'sensor_readings_5min',
    start_offset => INTERVAL '6 hours',
    end_offset => INTERVAL '1 minute',
    schedule_interval => INTERVAL '2 minutes',
    if_not_exists => TRUE
);

-- Refresh 1-hour aggregates every 10 minutes
SELECT add_continuous_aggregate_policy(
    'sensor_readings_1hour',
    start_offset => INTERVAL '1 day',
    end_offset => INTERVAL '5 minutes',
    schedule_interval => INTERVAL '10 minutes',
    if_not_exists => TRUE
);

-- Refresh 1-day aggregates every 1 hour
SELECT add_continuous_aggregate_policy(
    'sensor_readings_1day',
    start_offset => INTERVAL '7 days',
    end_offset => INTERVAL '1 hour',
    schedule_interval => INTERVAL '1 hour',
    if_not_exists => TRUE
);

-- =====================================================
-- HYPERTABLE INDEXES (Time-series optimized)
-- =====================================================

-- Optimized index for stream_id + time queries
CREATE INDEX IF NOT EXISTS idx_sensor_readings_stream_time
    ON sensor_readings (stream_id, time DESC);

-- Unique index for message deduplication
CREATE UNIQUE INDEX IF NOT EXISTS idx_sensor_readings_stream_message
    ON sensor_readings (stream_id, message_id)
    WHERE message_id IS NOT NULL;

-- Index for quality code filtering
CREATE INDEX IF NOT EXISTS idx_sensor_readings_quality
    ON sensor_readings (quality_code)
    WHERE quality_code != 0;

-- =====================================================
-- STATISTICS AND OPTIMIZATION
-- =====================================================

-- Update statistics for query planner
ANALYZE sensor_readings;
ANALYZE sensor_streams;
ANALYZE alert_rules;
ANALYZE alert_instances;

-- =====================================================
-- GRANTS (Continuous aggregates)
-- =====================================================

GRANT SELECT ON sensor_readings_1min TO authenticated;
GRANT SELECT ON sensor_readings_5min TO authenticated;
GRANT SELECT ON sensor_readings_1hour TO authenticated;
GRANT SELECT ON sensor_readings_1day TO authenticated;

-- =====================================================
-- HELPER FUNCTIONS
-- =====================================================

-- Function to get hypertable statistics
CREATE OR REPLACE FUNCTION get_sensor_readings_stats()
RETURNS TABLE (
    hypertable_name TEXT,
    total_chunks INT,
    compressed_chunks INT,
    uncompressed_chunks INT,
    total_size TEXT,
    compressed_size TEXT,
    compression_ratio NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        'sensor_readings'::TEXT,
        COUNT(*)::INT as total_chunks,
        COUNT(*) FILTER (WHERE is_compressed)::INT as compressed_chunks,
        COUNT(*) FILTER (WHERE NOT is_compressed)::INT as uncompressed_chunks,
        pg_size_pretty(SUM(total_bytes)) as total_size,
        pg_size_pretty(SUM(total_bytes) FILTER (WHERE is_compressed)) as compressed_size,
        ROUND(
            SUM(total_bytes) FILTER (WHERE NOT is_compressed)::NUMERIC / 
            NULLIF(SUM(total_bytes) FILTER (WHERE is_compressed), 0),
            2
        ) as compression_ratio
    FROM timescaledb_information.chunks
    WHERE hypertable_name = 'sensor_readings';
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION get_sensor_readings_stats() IS 'Get compression and storage statistics for sensor_readings hypertable';

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Verify hypertable creation
DO $$
DECLARE
    is_hypertable BOOLEAN;
    chunk_count INT;
BEGIN
    -- Check if sensor_readings is a hypertable
    SELECT EXISTS (
        SELECT 1
        FROM timescaledb_information.hypertables
        WHERE hypertable_name = 'sensor_readings'
    ) INTO is_hypertable;
    
    IF is_hypertable THEN
        RAISE NOTICE 'SUCCESS: sensor_readings is now a hypertable';
        
        -- Get chunk count
        SELECT COUNT(*)
        FROM timescaledb_information.chunks
        WHERE hypertable_name = 'sensor_readings'
        INTO chunk_count;
        
        RAISE NOTICE 'Current chunks: %', chunk_count;
    ELSE
        RAISE EXCEPTION 'FAILED: sensor_readings is not a hypertable';
    END IF;
    
    -- Check continuous aggregates
    IF EXISTS (SELECT 1 FROM pg_matviews WHERE matviewname = 'sensor_readings_1min') THEN
        RAISE NOTICE 'SUCCESS: Continuous aggregate sensor_readings_1min created';
    END IF;
    
    IF EXISTS (SELECT 1 FROM pg_matviews WHERE matviewname = 'sensor_readings_5min') THEN
        RAISE NOTICE 'SUCCESS: Continuous aggregate sensor_readings_5min created';
    END IF;
    
    IF EXISTS (SELECT 1 FROM pg_matviews WHERE matviewname = 'sensor_readings_1hour') THEN
        RAISE NOTICE 'SUCCESS: Continuous aggregate sensor_readings_1hour created';
    END IF;
    
    IF EXISTS (SELECT 1 FROM pg_matviews WHERE matviewname = 'sensor_readings_1day') THEN
        RAISE NOTICE 'SUCCESS: Continuous aggregate sensor_readings_1day created';
    END IF;
END $$;

-- =====================================================
-- SUCCESS MESSAGE
-- =====================================================

DO $$
BEGIN
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Migration 002_timescaledb_setup.sql completed successfully';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Features enabled:';
    RAISE NOTICE '  ✓ Hypertable with 1-day chunks';
    RAISE NOTICE '  ✓ Compression (7-day policy)';
    RAISE NOTICE '  ✓ Retention (2-year policy)';
    RAISE NOTICE '  ✓ 4 continuous aggregates (1min, 5min, 1hour, 1day)';
    RAISE NOTICE '  ✓ Automatic refresh policies';
    RAISE NOTICE '  ✓ Optimized indexes';
    RAISE NOTICE '';
    RAISE NOTICE 'Next: Run 003_additional_indexes.sql for query optimization';
    RAISE NOTICE '========================================';
END $$;

