-- Migration: Create alerts hypertable
-- Track A: Alert tracking with burn-rate monitoring
-- Author: DevOps/SRE/Security Squad
-- Date: 2025-09-29

-- ============================================================================
-- UP Migration
-- ============================================================================

BEGIN;

-- Create alerts table
CREATE TABLE IF NOT EXISTS alerts (
    alert_id UUID NOT NULL DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    triggered_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    resolved_at TIMESTAMPTZ,
    severity VARCHAR(20) NOT NULL, -- info, warning, error, critical
    category VARCHAR(50) NOT NULL, -- environment, equipment, compliance, security, slo
    source_type VARCHAR(50) NOT NULL, -- sensor, device, system, integration
    source_id UUID,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    current_value DECIMAL(10,4),
    threshold_value DECIMAL(10,4),
    metadata JSONB,
    acknowledged_at TIMESTAMPTZ,
    acknowledged_by UUID,
    notification_sent BOOLEAN DEFAULT FALSE,
    notification_channels TEXT[], -- ['slack', 'email', 'sms']
    created_at TIMESTAMPTZ DEFAULT NOW(),
    PRIMARY KEY (alert_id, triggered_at)
);

-- Convert to hypertable (1 day chunks)
SELECT create_hypertable(
    'alerts',
    'triggered_at',
    chunk_time_interval => INTERVAL '1 day',
    if_not_exists => TRUE
);

-- Add compression (compress after 30 days)
ALTER TABLE alerts SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'site_id,severity,category',
    timescaledb.compress_orderby = 'triggered_at DESC'
);

SELECT add_compression_policy('alerts', INTERVAL '30 days');

-- Add retention (keep for 2 years)
SELECT add_retention_policy('alerts', INTERVAL '730 days');

-- Create indexes
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_alerts_site_triggered 
    ON alerts USING BRIN (site_id, triggered_at);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_alerts_severity_triggered 
    ON alerts (severity, triggered_at DESC);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_alerts_category_triggered 
    ON alerts (category, triggered_at DESC);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_alerts_unresolved 
    ON alerts (site_id, triggered_at DESC)
    WHERE resolved_at IS NULL;

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_alerts_unacknowledged 
    ON alerts (site_id, triggered_at DESC)
    WHERE acknowledged_at IS NULL AND resolved_at IS NULL;

-- GIN index for metadata JSONB queries
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_alerts_metadata 
    ON alerts USING GIN (metadata);

-- Add RLS
ALTER TABLE alerts ENABLE ROW LEVEL SECURITY;

CREATE POLICY alerts_site_isolation ON alerts
FOR ALL
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
    )
);

CREATE POLICY alerts_service_account ON alerts
FOR ALL
USING (current_setting('app.user_role', TRUE) = 'service_account');

COMMENT ON TABLE alerts IS 'Alert tracking for burn-rate SLO monitoring and operational incidents';

COMMIT;

-- ============================================================================
-- Create alert rollups
-- ============================================================================

BEGIN;

-- 1-hour alert aggregation
CREATE MATERIALIZED VIEW IF NOT EXISTS alert_rollups_1h
WITH (timescaledb.continuous) AS
SELECT 
    site_id,
    severity,
    category,
    time_bucket('1 hour', triggered_at) AS bucket,
    COUNT(*) AS alert_count,
    COUNT(*) FILTER (WHERE resolved_at IS NOT NULL) AS resolved_count,
    COUNT(*) FILTER (WHERE resolved_at IS NULL) AS unresolved_count,
    COUNT(*) FILTER (WHERE acknowledged_at IS NOT NULL) AS acknowledged_count,
    AVG(EXTRACT(EPOCH FROM (COALESCE(resolved_at, NOW()) - triggered_at))) FILTER (WHERE resolved_at IS NOT NULL) AS avg_resolution_time_seconds,
    MAX(EXTRACT(EPOCH FROM (COALESCE(resolved_at, NOW()) - triggered_at))) FILTER (WHERE resolved_at IS NOT NULL) AS max_resolution_time_seconds
FROM alerts
GROUP BY site_id, severity, category, bucket
WITH NO DATA;

SELECT add_continuous_aggregate_policy(
    'alert_rollups_1h',
    start_offset => INTERVAL '2 days',
    end_offset => INTERVAL '1 hour',
    schedule_interval => INTERVAL '1 hour'
);

ALTER MATERIALIZED VIEW alert_rollups_1h SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'site_id,severity,category',
    timescaledb.compress_orderby = 'bucket DESC'
);

SELECT add_compression_policy('alert_rollups_1h', INTERVAL '90 days');
SELECT add_retention_policy('alert_rollups_1h', INTERVAL '730 days');

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_alert_rollups_1h_site_bucket 
    ON alert_rollups_1h (site_id, bucket DESC);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_alert_rollups_1h_severity 
    ON alert_rollups_1h (severity, bucket DESC);

COMMENT ON MATERIALIZED VIEW alert_rollups_1h IS 'Hourly alert metrics for MTTR and SLO burn-rate dashboards';

COMMIT;

-- ============================================================================
-- DOWN Migration
-- ============================================================================

BEGIN;

-- Remove rollups
SELECT remove_retention_policy('alert_rollups_1h', if_exists => TRUE);
SELECT remove_compression_policy('alert_rollups_1h', if_exists => TRUE);
SELECT remove_continuous_aggregate_policy('alert_rollups_1h', if_exists => TRUE);
DROP MATERIALIZED VIEW IF EXISTS alert_rollups_1h CASCADE;

-- Remove hypertable
DROP POLICY IF EXISTS alerts_site_isolation ON alerts;
DROP POLICY IF EXISTS alerts_service_account ON alerts;
SELECT remove_retention_policy('alerts', if_exists => TRUE);
SELECT remove_compression_policy('alerts', if_exists => TRUE);
DROP TABLE IF EXISTS alerts CASCADE;

COMMIT;
