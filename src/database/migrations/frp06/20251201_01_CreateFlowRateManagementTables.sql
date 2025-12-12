-- =====================================================================
-- FRP-06: Flow Rate Management & Intelligent Queue Tables
-- Migration: 20251201_01_CreateFlowRateManagementTables.sql
-- 
-- This migration adds tables for:
-- 1. Site irrigation settings (max flow rate configuration)
-- 2. Zone emitter configurations (emitter details for flow calculation)
-- 3. Queued irrigation events (flow rate-based queue management)
-- =====================================================================

-- =====================================================================
-- 1) Irrigation Settings - Site-level flow rate configuration
-- =====================================================================

CREATE TABLE IF NOT EXISTS irrigation_settings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    
    -- Flow rate limits
    max_system_flow_rate_liters_per_minute DECIMAL(10, 2) NOT NULL,
    flow_rate_safety_margin_percent DECIMAL(5, 2) NOT NULL DEFAULT 5.0,
    
    -- Queuing settings
    enable_flow_rate_queuing BOOLEAN NOT NULL DEFAULT true,
    enable_smart_suggestions BOOLEAN NOT NULL DEFAULT true,
    suggestion_threshold_count INTEGER NOT NULL DEFAULT 3,
    
    -- Audit
    created_by_user_id UUID NOT NULL,
    updated_by_user_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- One settings record per site
    CONSTRAINT uq_irrigation_settings_site UNIQUE (site_id),
    
    -- Validation
    CONSTRAINT chk_max_flow_rate_positive 
        CHECK (max_system_flow_rate_liters_per_minute > 0),
    CONSTRAINT chk_safety_margin_range 
        CHECK (flow_rate_safety_margin_percent >= 0 AND flow_rate_safety_margin_percent <= 50),
    CONSTRAINT chk_suggestion_threshold_positive 
        CHECK (suggestion_threshold_count >= 1)
);

CREATE INDEX idx_irrigation_settings_site ON irrigation_settings(site_id);

COMMENT ON TABLE irrigation_settings IS 'Site-level irrigation system settings for flow rate management';
COMMENT ON COLUMN irrigation_settings.max_system_flow_rate_liters_per_minute IS 'Maximum total flow rate the system can handle (pump, lines, etc.)';
COMMENT ON COLUMN irrigation_settings.flow_rate_safety_margin_percent IS 'Safety margin to stay below max (default 5% = never exceed 95% of max)';

-- =====================================================================
-- 2) Zone Emitter Configurations - Detailed emitter specs per zone
-- =====================================================================

CREATE TABLE IF NOT EXISTS zone_emitter_configurations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    zone_id UUID NOT NULL REFERENCES inventory_locations(id) ON DELETE CASCADE,
    zone_name VARCHAR(255) NOT NULL,
    
    -- Emitter specifications
    emitter_count INTEGER NOT NULL,
    emitter_flow_rate_liters_per_hour DECIMAL(8, 3) NOT NULL,
    emitter_type VARCHAR(100) NOT NULL,
    emitters_per_plant INTEGER NOT NULL DEFAULT 1,
    
    -- Optional specifications
    operating_pressure_kpa DECIMAL(8, 2),
    
    -- Calibration tracking
    last_calibrated_at TIMESTAMPTZ,
    calibrated_by_user_id UUID,
    
    -- Audit
    created_by_user_id UUID NOT NULL,
    updated_by_user_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- One configuration per zone
    CONSTRAINT uq_zone_emitter_config UNIQUE (zone_id),
    
    -- Validation
    CONSTRAINT chk_emitter_count_positive CHECK (emitter_count > 0),
    CONSTRAINT chk_emitter_flow_rate_positive CHECK (emitter_flow_rate_liters_per_hour > 0),
    CONSTRAINT chk_emitters_per_plant_positive CHECK (emitters_per_plant > 0)
);

CREATE INDEX idx_zone_emitter_site ON zone_emitter_configurations(site_id);
CREATE INDEX idx_zone_emitter_zone ON zone_emitter_configurations(zone_id);

COMMENT ON TABLE zone_emitter_configurations IS 'Emitter configuration for each irrigation zone - used for flow rate calculations';
COMMENT ON COLUMN zone_emitter_configurations.emitter_count IS 'Total number of emitters in this zone';
COMMENT ON COLUMN zone_emitter_configurations.emitter_flow_rate_liters_per_hour IS 'Flow rate per emitter in L/h (common: 1.0, 2.0, 4.0)';

-- =====================================================================
-- 3) Queued Irrigation Events - Flow rate-based queue
-- =====================================================================

CREATE TABLE IF NOT EXISTS queued_irrigation_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    program_id UUID NOT NULL,
    schedule_id UUID,
    target_zone_ids UUID[] NOT NULL,
    
    -- Timing
    original_scheduled_time TIMESTAMPTZ NOT NULL,
    expected_execution_time TIMESTAMPTZ NOT NULL,
    
    -- Queue info
    queue_reason TEXT NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    
    -- Execution tracking
    executed_at TIMESTAMPTZ,
    irrigation_run_id UUID,
    failure_message TEXT,
    
    -- Audit
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    
    -- Validation
    CONSTRAINT chk_status_valid 
        CHECK (status IN ('Pending', 'Executed', 'Failed', 'Cancelled')),
    CONSTRAINT chk_target_zones_not_empty 
        CHECK (array_length(target_zone_ids, 1) > 0),
    CONSTRAINT chk_expected_after_original 
        CHECK (expected_execution_time >= original_scheduled_time)
);

CREATE INDEX idx_queued_events_site ON queued_irrigation_events(site_id);
CREATE INDEX idx_queued_events_status ON queued_irrigation_events(status);
CREATE INDEX idx_queued_events_pending ON queued_irrigation_events(site_id, expected_execution_time) 
    WHERE status = 'Pending';
CREATE INDEX idx_queued_events_program ON queued_irrigation_events(program_id);
CREATE INDEX idx_queued_events_scheduled_time ON queued_irrigation_events(original_scheduled_time);

COMMENT ON TABLE queued_irrigation_events IS 'Irrigation events queued due to flow rate constraints';
COMMENT ON COLUMN queued_irrigation_events.queue_reason IS 'Human-readable reason why this event was queued';

-- =====================================================================
-- 4) Queue Statistics View - For analysis and suggestions
-- =====================================================================

CREATE OR REPLACE VIEW queue_statistics_daily AS
SELECT 
    site_id,
    DATE(original_scheduled_time) AS queue_date,
    EXTRACT(HOUR FROM original_scheduled_time) AS hour_of_day,
    COUNT(*) AS queued_count,
    AVG(EXTRACT(EPOCH FROM (expected_execution_time - original_scheduled_time)) / 60) AS avg_delay_minutes,
    MAX(EXTRACT(EPOCH FROM (expected_execution_time - original_scheduled_time)) / 60) AS max_delay_minutes,
    COUNT(*) FILTER (WHERE status = 'Executed') AS executed_count,
    COUNT(*) FILTER (WHERE status = 'Failed') AS failed_count
FROM queued_irrigation_events
GROUP BY site_id, DATE(original_scheduled_time), EXTRACT(HOUR FROM original_scheduled_time);

COMMENT ON VIEW queue_statistics_daily IS 'Daily queue statistics for smart suggestion analysis';

-- =====================================================================
-- 5) RLS Policies
-- =====================================================================

-- Enable RLS on all new tables
ALTER TABLE irrigation_settings ENABLE ROW LEVEL SECURITY;
ALTER TABLE zone_emitter_configurations ENABLE ROW LEVEL SECURITY;
ALTER TABLE queued_irrigation_events ENABLE ROW LEVEL SECURITY;

-- Site isolation policies
CREATE POLICY irrigation_settings_site_isolation ON irrigation_settings
    FOR ALL
    USING (site_id = current_setting('app.site_id', true)::UUID);

CREATE POLICY zone_emitter_site_isolation ON zone_emitter_configurations
    FOR ALL
    USING (site_id = current_setting('app.site_id', true)::UUID);

CREATE POLICY queued_events_site_isolation ON queued_irrigation_events
    FOR ALL
    USING (site_id = current_setting('app.site_id', true)::UUID);

-- Service account bypass policies
CREATE POLICY irrigation_settings_service_bypass ON irrigation_settings
    FOR ALL
    USING (current_setting('app.user_role', true) = 'service_account');

CREATE POLICY zone_emitter_service_bypass ON zone_emitter_configurations
    FOR ALL
    USING (current_setting('app.user_role', true) = 'service_account');

CREATE POLICY queued_events_service_bypass ON queued_irrigation_events
    FOR ALL
    USING (current_setting('app.user_role', true) = 'service_account');

-- =====================================================================
-- 6) Helper Functions
-- =====================================================================

-- Calculate total zone flow rate in L/min
CREATE OR REPLACE FUNCTION calculate_zone_flow_rate_lpm(p_zone_id UUID)
RETURNS DECIMAL AS $$
BEGIN
    RETURN (
        SELECT (emitter_count * emitter_flow_rate_liters_per_hour) / 60.0
        FROM zone_emitter_configurations
        WHERE zone_id = p_zone_id
    );
END;
$$ LANGUAGE plpgsql STABLE;

COMMENT ON FUNCTION calculate_zone_flow_rate_lpm IS 'Calculate total flow rate for a zone in liters per minute';

-- Calculate effective max flow rate for a site
CREATE OR REPLACE FUNCTION calculate_effective_max_flow_rate(p_site_id UUID)
RETURNS DECIMAL AS $$
BEGIN
    RETURN (
        SELECT max_system_flow_rate_liters_per_minute * (1 - flow_rate_safety_margin_percent / 100)
        FROM irrigation_settings
        WHERE site_id = p_site_id
    );
END;
$$ LANGUAGE plpgsql STABLE;

COMMENT ON FUNCTION calculate_effective_max_flow_rate IS 'Calculate effective max flow rate after applying safety margin';









