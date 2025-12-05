-- =====================================================
-- FRP05: Telemetry Service - Additional Indexes
-- Migration: 003_additional_indexes.sql
-- Description: Performance optimization indexes
-- Author: AI Agent
-- Date: 2025-10-02
-- Dependencies: 001_initial_schema.sql, 002_timescaledb_setup.sql
-- =====================================================

-- =====================================================
-- SENSOR_STREAMS INDEXES
-- =====================================================

-- Index for finding streams by location hierarchy
CREATE INDEX IF NOT EXISTS idx_sensor_streams_location
    ON sensor_streams(location_id)
    WHERE location_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_sensor_streams_room
    ON sensor_streams(room_id)
    WHERE room_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_sensor_streams_zone
    ON sensor_streams(zone_id)
    WHERE zone_id IS NOT NULL;

-- Composite index for finding streams by equipment and channel
CREATE INDEX IF NOT EXISTS idx_sensor_streams_equipment_channel
    ON sensor_streams(equipment_id, equipment_channel_id)
    WHERE equipment_channel_id IS NOT NULL;

-- Index for filtering by stream type
CREATE INDEX IF NOT EXISTS idx_sensor_streams_type
    ON sensor_streams(stream_type);

-- GIN index for JSONB metadata queries
CREATE INDEX IF NOT EXISTS idx_sensor_streams_metadata
    ON sensor_streams USING GIN (metadata)
    WHERE metadata IS NOT NULL;

-- =====================================================
-- SENSOR_READINGS INDEXES
-- =====================================================

-- Index for finding readings in a time range for a site
-- Useful for multi-stream queries per site
CREATE INDEX IF NOT EXISTS idx_sensor_readings_site_time
    ON sensor_readings(time DESC)
    INCLUDE (stream_id, value)
    WHERE time >= NOW() - INTERVAL '30 days';  -- Partial index for recent data

-- Index for ingestion timestamp queries (monitoring)
CREATE INDEX IF NOT EXISTS idx_sensor_readings_ingestion
    ON sensor_readings(ingestion_timestamp DESC)
    WHERE ingestion_timestamp >= NOW() - INTERVAL '7 days';

-- Index for source timestamp queries
CREATE INDEX IF NOT EXISTS idx_sensor_readings_source_timestamp
    ON sensor_readings(source_timestamp DESC)
    WHERE source_timestamp IS NOT NULL;

-- GIN index for JSONB metadata queries on readings
CREATE INDEX IF NOT EXISTS idx_sensor_readings_metadata
    ON sensor_readings USING GIN (metadata)
    WHERE metadata IS NOT NULL;

-- =====================================================
-- ALERT_RULES INDEXES
-- =====================================================

-- GIN index for stream_ids array queries
CREATE INDEX IF NOT EXISTS idx_alert_rules_stream_ids
    ON alert_rules USING GIN (stream_ids);

-- Index for finding rules by type
CREATE INDEX IF NOT EXISTS idx_alert_rules_type
    ON alert_rules(rule_type)
    WHERE is_active = true;

-- Index for finding rules by severity
CREATE INDEX IF NOT EXISTS idx_alert_rules_severity
    ON alert_rules(severity)
    WHERE is_active = true;

-- Index for finding rules by creator/updater
CREATE INDEX IF NOT EXISTS idx_alert_rules_created_by
    ON alert_rules(created_by);

CREATE INDEX IF NOT EXISTS idx_alert_rules_updated_by
    ON alert_rules(updated_by);

-- GIN index for threshold_config JSONB queries
CREATE INDEX IF NOT EXISTS idx_alert_rules_threshold_config
    ON alert_rules USING GIN (threshold_config);

-- GIN index for metadata JSONB queries
CREATE INDEX IF NOT EXISTS idx_alert_rules_metadata
    ON alert_rules USING GIN (metadata)
    WHERE metadata IS NOT NULL;

-- =====================================================
-- ALERT_INSTANCES INDEXES
-- =====================================================

-- Index for finding active alerts by site
CREATE INDEX IF NOT EXISTS idx_alert_instances_site_cleared
    ON alert_instances(site_id, cleared_at)
    WHERE cleared_at IS NULL;

-- Index for finding unacknowledged alerts
CREATE INDEX IF NOT EXISTS idx_alert_instances_unacknowledged
    ON alert_instances(fired_at DESC)
    WHERE acknowledged_at IS NULL;

-- Index for alert history by severity
CREATE INDEX IF NOT EXISTS idx_alert_instances_severity_fired
    ON alert_instances(severity, fired_at DESC);

-- Index for finding alerts by acknowledging user
CREATE INDEX IF NOT EXISTS idx_alert_instances_acknowledged_by
    ON alert_instances(acknowledged_by, acknowledged_at DESC)
    WHERE acknowledged_by IS NOT NULL;

-- Index for alert duration queries
CREATE INDEX IF NOT EXISTS idx_alert_instances_duration
    ON alert_instances(fired_at, cleared_at)
    WHERE cleared_at IS NOT NULL;

-- GIN index for metadata JSONB queries
CREATE INDEX IF NOT EXISTS idx_alert_instances_metadata
    ON alert_instances USING GIN (metadata)
    WHERE metadata IS NOT NULL;

-- =====================================================
-- INGESTION_SESSIONS INDEXES
-- =====================================================

-- Index for finding sessions by protocol
CREATE INDEX IF NOT EXISTS idx_ingestion_sessions_protocol
    ON ingestion_sessions(protocol, started_at DESC);

-- Index for finding active sessions with low heartbeat
-- (for detecting stale connections)
CREATE INDEX IF NOT EXISTS idx_ingestion_sessions_stale
    ON ingestion_sessions(last_heartbeat_at)
    WHERE ended_at IS NULL
      AND last_heartbeat_at < NOW() - INTERVAL '5 minutes';

-- Index for session duration queries
CREATE INDEX IF NOT EXISTS idx_ingestion_sessions_duration
    ON ingestion_sessions(started_at, ended_at)
    WHERE ended_at IS NOT NULL;

-- Index for finding sessions with errors
CREATE INDEX IF NOT EXISTS idx_ingestion_sessions_errors
    ON ingestion_sessions(error_count DESC, started_at DESC)
    WHERE error_count > 0;

-- GIN index for metadata JSONB queries
CREATE INDEX IF NOT EXISTS idx_ingestion_sessions_metadata
    ON ingestion_sessions USING GIN (metadata)
    WHERE metadata IS NOT NULL;

-- =====================================================
-- INGESTION_ERRORS INDEXES
-- =====================================================

-- Index for finding errors by type
CREATE INDEX IF NOT EXISTS idx_ingestion_errors_type
    ON ingestion_errors(error_type, occurred_at DESC);

-- Index for finding errors by protocol
CREATE INDEX IF NOT EXISTS idx_ingestion_errors_protocol
    ON ingestion_errors(protocol, occurred_at DESC);

-- Index for finding errors by equipment
CREATE INDEX IF NOT EXISTS idx_ingestion_errors_equipment
    ON ingestion_errors(equipment_id, occurred_at DESC)
    WHERE equipment_id IS NOT NULL;

-- Index for finding errors by session
CREATE INDEX IF NOT EXISTS idx_ingestion_errors_session
    ON ingestion_errors(session_id, occurred_at DESC)
    WHERE session_id IS NOT NULL;

-- GIN index for raw_payload JSONB queries
CREATE INDEX IF NOT EXISTS idx_ingestion_errors_raw_payload
    ON ingestion_errors USING GIN (raw_payload)
    WHERE raw_payload IS NOT NULL;

-- =====================================================
-- CONTINUOUS AGGREGATE INDEXES
-- =====================================================

-- Indexes on continuous aggregates for faster queries
CREATE INDEX IF NOT EXISTS idx_sensor_readings_1min_bucket
    ON sensor_readings_1min(bucket DESC, stream_id);

CREATE INDEX IF NOT EXISTS idx_sensor_readings_5min_bucket
    ON sensor_readings_5min(bucket DESC, stream_id);

CREATE INDEX IF NOT EXISTS idx_sensor_readings_1hour_bucket
    ON sensor_readings_1hour(bucket DESC, stream_id);

CREATE INDEX IF NOT EXISTS idx_sensor_readings_1day_bucket
    ON sensor_readings_1day(bucket DESC, stream_id);

-- Indexes for quality metric queries
CREATE INDEX IF NOT EXISTS idx_sensor_readings_1min_quality
    ON sensor_readings_1min(stream_id, bucket DESC)
    WHERE bad_count > 0;

CREATE INDEX IF NOT EXISTS idx_sensor_readings_5min_quality
    ON sensor_readings_5min(stream_id, bucket DESC)
    WHERE bad_count > 0;

-- =====================================================
-- STATISTICS UPDATE
-- =====================================================

-- Update statistics for query planner
ANALYZE sensor_streams;
ANALYZE sensor_readings;
ANALYZE alert_rules;
ANALYZE alert_instances;
ANALYZE ingestion_sessions;
ANALYZE ingestion_errors;
ANALYZE sensor_readings_1min;
ANALYZE sensor_readings_5min;
ANALYZE sensor_readings_1hour;
ANALYZE sensor_readings_1day;

-- =====================================================
-- INDEX MAINTENANCE FUNCTIONS
-- =====================================================

-- Function to reindex all telemetry tables (for maintenance)
CREATE OR REPLACE FUNCTION reindex_telemetry_tables()
RETURNS void AS $$
BEGIN
    REINDEX TABLE sensor_streams;
    REINDEX TABLE alert_rules;
    REINDEX TABLE alert_instances;
    REINDEX TABLE ingestion_sessions;
    REINDEX TABLE ingestion_errors;
    -- Note: Don't reindex hypertables directly, use TimescaleDB commands
    RAISE NOTICE 'Reindexed all telemetry tables (except hypertables)';
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION reindex_telemetry_tables() IS 'Reindex all telemetry tables for maintenance';

-- Function to get index usage statistics
CREATE OR REPLACE FUNCTION get_index_usage_stats()
RETURNS TABLE (
    schemaname TEXT,
    tablename TEXT,
    indexname TEXT,
    idx_scan BIGINT,
    idx_tup_read BIGINT,
    idx_tup_fetch BIGINT,
    size TEXT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        s.schemaname::TEXT,
        s.tablename::TEXT,
        s.indexrelname::TEXT,
        s.idx_scan,
        s.idx_tup_read,
        s.idx_tup_fetch,
        pg_size_pretty(pg_relation_size(s.indexrelid))
    FROM pg_stat_user_indexes s
    JOIN pg_index i ON s.indexrelid = i.indexrelid
    WHERE s.schemaname = 'public'
      AND s.tablename IN (
          'sensor_streams', 'sensor_readings', 'alert_rules',
          'alert_instances', 'ingestion_sessions', 'ingestion_errors'
      )
    ORDER BY s.idx_scan DESC;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION get_index_usage_stats() IS 'Get index usage statistics for telemetry tables';

-- =====================================================
-- VERIFICATION
-- =====================================================

DO $$
DECLARE
    index_count INT;
BEGIN
    -- Count indexes created
    SELECT COUNT(*)
    INTO index_count
    FROM pg_indexes
    WHERE schemaname = 'public'
      AND tablename IN (
          'sensor_streams', 'sensor_readings', 'alert_rules',
          'alert_instances', 'ingestion_sessions', 'ingestion_errors',
          'sensor_readings_1min', 'sensor_readings_5min',
          'sensor_readings_1hour', 'sensor_readings_1day'
      );
    
    RAISE NOTICE 'Total indexes on telemetry tables: %', index_count;
    
    -- Check for GIN indexes (important for JSONB queries)
    SELECT COUNT(*)
    INTO index_count
    FROM pg_indexes
    WHERE schemaname = 'public'
      AND indexdef LIKE '%USING gin%'
      AND tablename IN (
          'sensor_streams', 'sensor_readings', 'alert_rules',
          'alert_instances', 'ingestion_sessions', 'ingestion_errors'
      );
    
    RAISE NOTICE 'GIN indexes created: %', index_count;
END $$;

-- =====================================================
-- SUCCESS MESSAGE
-- =====================================================

DO $$
BEGIN
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Migration 003_additional_indexes.sql completed successfully';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Performance features added:';
    RAISE NOTICE '  ✓ Location hierarchy indexes';
    RAISE NOTICE '  ✓ JSONB GIN indexes for metadata queries';
    RAISE NOTICE '  ✓ Partial indexes for recent data';
    RAISE NOTICE '  ✓ Array indexes for stream_ids';
    RAISE NOTICE '  ✓ Continuous aggregate indexes';
    RAISE NOTICE '  ✓ Index maintenance functions';
    RAISE NOTICE '';
    RAISE NOTICE 'Next: Run 004_rls_policies.sql for row-level security';
    RAISE NOTICE '========================================';
END $$;

