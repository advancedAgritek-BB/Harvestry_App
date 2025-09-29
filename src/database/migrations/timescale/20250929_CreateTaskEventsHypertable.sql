-- Migration: Create task_events hypertable
-- Track A: Task lifecycle tracking for p95 < 300ms SLO
-- Author: Workflow & Messaging Squad
-- Date: 2025-09-29

-- ============================================================================
-- UP Migration
-- ============================================================================

BEGIN;

-- Create task_events table
CREATE TABLE IF NOT EXISTS task_events (
    event_id UUID NOT NULL DEFAULT gen_random_uuid(),
    task_id UUID NOT NULL,
    site_id UUID NOT NULL,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    event_type VARCHAR(50) NOT NULL, -- created, assigned, started, completed, blocked, unblocked, cancelled
    user_id UUID,
    previous_state VARCHAR(50),
    new_state VARCHAR(50) NOT NULL,
    previous_assignee_id UUID,
    new_assignee_id UUID,
    duration_ms INT, -- For performance tracking
    evidence_urls TEXT[],
    notes TEXT,
    metadata JSONB,
    slack_thread_ts VARCHAR(50), -- For Slack integration
    created_at TIMESTAMPTZ DEFAULT NOW(),
    PRIMARY KEY (event_id, occurred_at)
);

-- Convert to hypertable (1 day chunks)
SELECT create_hypertable(
    'task_events',
    'occurred_at',
    chunk_time_interval => INTERVAL '1 day',
    if_not_exists => TRUE
);

-- Add compression (compress after 30 days)
ALTER TABLE task_events SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'task_id,site_id,event_type',
    timescaledb.compress_orderby = 'occurred_at DESC'
);

SELECT add_compression_policy('task_events', INTERVAL '30 days');

-- Add retention (keep for 2 years)
SELECT add_retention_policy('task_events', INTERVAL '730 days');

-- Create indexes
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_task_events_site_occurred 
    ON task_events USING BRIN (site_id, occurred_at);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_task_events_task_occurred 
    ON task_events (task_id, occurred_at DESC);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_task_events_user_occurred 
    ON task_events (user_id, occurred_at DESC)
    WHERE user_id IS NOT NULL;

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_task_events_type 
    ON task_events (event_type, occurred_at DESC);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_task_events_slack 
    ON task_events (slack_thread_ts)
    WHERE slack_thread_ts IS NOT NULL;

-- GIN index for metadata
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_task_events_metadata 
    ON task_events USING GIN (metadata);

-- Add RLS
ALTER TABLE task_events ENABLE ROW LEVEL SECURITY;

CREATE POLICY task_events_site_isolation ON task_events
FOR ALL
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
    )
);

CREATE POLICY task_events_service_account ON task_events
FOR ALL
USING (current_setting('app.user_role', TRUE) = 'service_account');

COMMENT ON TABLE task_events IS 'Task lifecycle events for p95 < 300ms round-trip SLO tracking';

COMMIT;

-- ============================================================================
-- Create task performance rollups
-- ============================================================================

BEGIN;

-- 5-minute task event aggregation (for real-time SLO monitoring)
CREATE MATERIALIZED VIEW IF NOT EXISTS task_event_rollups_5m
WITH (timescaledb.continuous) AS
SELECT 
    site_id,
    event_type,
    time_bucket('5 minutes', occurred_at) AS bucket,
    COUNT(*) AS event_count,
    AVG(duration_ms) FILTER (WHERE duration_ms IS NOT NULL) AS avg_duration_ms,
    PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY duration_ms) FILTER (WHERE duration_ms IS NOT NULL) AS p95_duration_ms,
    PERCENTILE_CONT(0.99) WITHIN GROUP (ORDER BY duration_ms) FILTER (WHERE duration_ms IS NOT NULL) AS p99_duration_ms,
    MAX(duration_ms) FILTER (WHERE duration_ms IS NOT NULL) AS max_duration_ms,
    COUNT(*) FILTER (WHERE duration_ms > 300) AS slo_breaches -- SLO: p95 < 300ms
FROM task_events
WHERE event_type IN ('created', 'assigned', 'started', 'completed')
GROUP BY site_id, event_type, bucket
WITH NO DATA;

SELECT add_continuous_aggregate_policy(
    'task_event_rollups_5m',
    start_offset => INTERVAL '12 hours',
    end_offset => INTERVAL '5 minutes',
    schedule_interval => INTERVAL '5 minutes'
);

ALTER MATERIALIZED VIEW task_event_rollups_5m SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'site_id,event_type',
    timescaledb.compress_orderby = 'bucket DESC'
);

SELECT add_compression_policy('task_event_rollups_5m', INTERVAL '60 days');
SELECT add_retention_policy('task_event_rollups_5m', INTERVAL '365 days');

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_task_event_rollups_5m_site_bucket 
    ON task_event_rollups_5m (site_id, bucket DESC);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_task_event_rollups_5m_type 
    ON task_event_rollups_5m (event_type, bucket DESC);

COMMENT ON MATERIALIZED VIEW task_event_rollups_5m IS '5-minute task performance metrics for SLO burn-rate monitoring';

COMMIT;

-- ============================================================================
-- Create hourly task summary
-- ============================================================================

BEGIN;

CREATE MATERIALIZED VIEW IF NOT EXISTS task_summary_1h
WITH (timescaledb.continuous) AS
SELECT 
    site_id,
    time_bucket('1 hour', occurred_at) AS bucket,
    COUNT(DISTINCT task_id) AS unique_tasks,
    COUNT(*) FILTER (WHERE event_type = 'created') AS tasks_created,
    COUNT(*) FILTER (WHERE event_type = 'completed') AS tasks_completed,
    COUNT(*) FILTER (WHERE event_type = 'blocked') AS tasks_blocked,
    COUNT(*) FILTER (WHERE event_type = 'cancelled') AS tasks_cancelled,
    COUNT(DISTINCT user_id) FILTER (WHERE user_id IS NOT NULL) AS active_users
FROM task_events
GROUP BY site_id, bucket
WITH NO DATA;

SELECT add_continuous_aggregate_policy(
    'task_summary_1h',
    start_offset => INTERVAL '1 day',
    end_offset => INTERVAL '1 hour',
    schedule_interval => INTERVAL '1 hour'
);

ALTER MATERIALIZED VIEW task_summary_1h SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'site_id',
    timescaledb.compress_orderby = 'bucket DESC'
);

SELECT add_compression_policy('task_summary_1h', INTERVAL '90 days');
SELECT add_retention_policy('task_summary_1h', INTERVAL '730 days');

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_task_summary_1h_site_bucket 
    ON task_summary_1h (site_id, bucket DESC);

COMMENT ON MATERIALIZED VIEW task_summary_1h IS 'Hourly task summary for operational dashboards';

COMMIT;

-- ============================================================================
-- DOWN Migration
-- ============================================================================

BEGIN;

-- Remove hourly summary
SELECT remove_retention_policy('task_summary_1h', if_exists => TRUE);
SELECT remove_compression_policy('task_summary_1h', if_exists => TRUE);
SELECT remove_continuous_aggregate_policy('task_summary_1h', if_exists => TRUE);
DROP MATERIALIZED VIEW IF EXISTS task_summary_1h CASCADE;

-- Remove 5m rollups
SELECT remove_retention_policy('task_event_rollups_5m', if_exists => TRUE);
SELECT remove_compression_policy('task_event_rollups_5m', if_exists => TRUE);
SELECT remove_continuous_aggregate_policy('task_event_rollups_5m', if_exists => TRUE);
DROP MATERIALIZED VIEW IF EXISTS task_event_rollups_5m CASCADE;

-- Remove hypertable
DROP POLICY IF EXISTS task_events_site_isolation ON task_events;
DROP POLICY IF EXISTS task_events_service_account ON task_events;
SELECT remove_retention_policy('task_events', if_exists => TRUE);
SELECT remove_compression_policy('task_events', if_exists => TRUE);
DROP TABLE IF EXISTS task_events CASCADE;

COMMIT;
