-- Migration: Create irrigation_step_runs hypertable
-- Track A: Irrigation control telemetry with SLO monitoring
-- Author: Telemetry & Controls Squad
-- Date: 2025-09-29

-- ============================================================================
-- UP Migration
-- ============================================================================

BEGIN;

-- Create irrigation_step_runs table
CREATE TABLE IF NOT EXISTS irrigation_step_runs (
    run_id UUID NOT NULL,
    step_number SMALLINT NOT NULL,
    site_id UUID NOT NULL,
    program_id UUID NOT NULL,
    group_id UUID NOT NULL,
    started_at TIMESTAMPTZ NOT NULL,
    completed_at TIMESTAMPTZ,
    target_duration_seconds INT NOT NULL,
    actual_duration_seconds INT,
    target_volume_liters DECIMAL(10,2),
    actual_volume_liters DECIMAL(10,2),
    valve_ids UUID[] NOT NULL,
    ec_before DECIMAL(5,2),
    ec_after DECIMAL(5,2),
    ph_before DECIMAL(5,2),
    ph_after DECIMAL(5,2),
    status VARCHAR(20) NOT NULL DEFAULT 'running', -- running, completed, aborted, failed
    abort_reason TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    PRIMARY KEY (run_id, step_number, started_at)
);

-- Convert to hypertable (1 day chunks)
SELECT create_hypertable(
    'irrigation_step_runs',
    'started_at',
    chunk_time_interval => INTERVAL '1 day',
    if_not_exists => TRUE
);

-- Add compression (compress after 30 days)
ALTER TABLE irrigation_step_runs SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'run_id,site_id,program_id',
    timescaledb.compress_orderby = 'started_at DESC'
);

SELECT add_compression_policy('irrigation_step_runs', INTERVAL '30 days');

-- Add retention (keep for 2 years)
SELECT add_retention_policy('irrigation_step_runs', INTERVAL '730 days');

-- Create indexes
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_irrigation_step_runs_site_started 
    ON irrigation_step_runs USING BRIN (site_id, started_at);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_irrigation_step_runs_program 
    ON irrigation_step_runs (program_id, started_at DESC);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_irrigation_step_runs_group 
    ON irrigation_step_runs (group_id, started_at DESC);

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_irrigation_step_runs_status 
    ON irrigation_step_runs (status, started_at DESC)
    WHERE status IN ('running', 'failed');

-- Add RLS
ALTER TABLE irrigation_step_runs ENABLE ROW LEVEL SECURITY;

CREATE POLICY irrigation_step_runs_site_isolation ON irrigation_step_runs
FOR ALL
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
    )
);

CREATE POLICY irrigation_step_runs_service_account ON irrigation_step_runs
FOR ALL
USING (current_setting('app.user_role', TRUE) = 'service_account');

COMMENT ON TABLE irrigation_step_runs IS 'Irrigation program execution steps for p95 < 800ms SLO tracking';

COMMIT;

-- ============================================================================
-- Create hourly irrigation rollups
-- ============================================================================

BEGIN;

CREATE MATERIALIZED VIEW IF NOT EXISTS irrigation_rollups_1h
WITH (timescaledb.continuous) AS
SELECT 
    site_id,
    program_id,
    group_id,
    time_bucket('1 hour', started_at) AS bucket,
    COUNT(*) AS run_count,
    COUNT(*) FILTER (WHERE status = 'completed') AS completed_count,
    COUNT(*) FILTER (WHERE status = 'failed') AS failed_count,
    COUNT(*) FILTER (WHERE status = 'aborted') AS aborted_count,
    AVG(actual_duration_seconds) FILTER (WHERE status = 'completed') AS avg_duration_seconds,
    AVG(actual_volume_liters) FILTER (WHERE status = 'completed') AS avg_volume_liters,
    SUM(actual_volume_liters) FILTER (WHERE status = 'completed') AS total_volume_liters,
    AVG(ec_after - ec_before) FILTER (WHERE ec_before IS NOT NULL AND ec_after IS NOT NULL) AS avg_ec_delta,
    AVG(ph_after - ph_before) FILTER (WHERE ph_before IS NOT NULL AND ph_after IS NOT NULL) AS avg_ph_delta
FROM irrigation_step_runs
GROUP BY site_id, program_id, group_id, bucket
WITH NO DATA;

SELECT add_continuous_aggregate_policy(
    'irrigation_rollups_1h',
    start_offset => INTERVAL '1 day',
    end_offset => INTERVAL '1 hour',
    schedule_interval => INTERVAL '1 hour'
);

ALTER MATERIALIZED VIEW irrigation_rollups_1h SET (
    timescaledb.compress,
    timescaledb.compress_segmentby = 'site_id,program_id',
    timescaledb.compress_orderby = 'bucket DESC'
);

SELECT add_compression_policy('irrigation_rollups_1h', INTERVAL '90 days');
SELECT add_retention_policy('irrigation_rollups_1h', INTERVAL '730 days');

CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_irrigation_rollups_1h_site_bucket 
    ON irrigation_rollups_1h (site_id, bucket DESC);

COMMENT ON MATERIALIZED VIEW irrigation_rollups_1h IS 'Hourly irrigation performance metrics for dashboard';

COMMIT;

-- ============================================================================
-- DOWN Migration
-- ============================================================================

BEGIN;

-- Remove rollups
SELECT remove_retention_policy('irrigation_rollups_1h', if_exists => TRUE);
SELECT remove_compression_policy('irrigation_rollups_1h', if_exists => TRUE);
SELECT remove_continuous_aggregate_policy('irrigation_rollups_1h', if_exists => TRUE);
DROP MATERIALIZED VIEW IF EXISTS irrigation_rollups_1h CASCADE;

-- Remove hypertable
DROP POLICY IF EXISTS irrigation_step_runs_site_isolation ON irrigation_step_runs;
DROP POLICY IF EXISTS irrigation_step_runs_service_account ON irrigation_step_runs;
SELECT remove_retention_policy('irrigation_step_runs', if_exists => TRUE);
SELECT remove_compression_policy('irrigation_step_runs', if_exists => TRUE);
DROP TABLE IF EXISTS irrigation_step_runs CASCADE;

COMMIT;
