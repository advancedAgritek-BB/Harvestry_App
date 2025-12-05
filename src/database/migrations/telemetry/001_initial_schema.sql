-- =====================================================
-- FRP05: Telemetry Service - Initial Schema
-- Migration: 001_initial_schema.sql
-- Description: Creates base tables for telemetry service
-- Author: AI Agent
-- Date: 2025-10-02
-- =====================================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "timescaledb";

-- =====================================================
-- TABLE: sensor_streams
-- Description: Configuration for sensor data streams
-- =====================================================
CREATE TABLE IF NOT EXISTS sensor_streams (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    site_id UUID NOT NULL,
    equipment_id UUID NOT NULL,
    equipment_channel_id UUID,
    stream_type VARCHAR(50) NOT NULL,
    unit VARCHAR(50) NOT NULL,
    display_name VARCHAR(200) NOT NULL,
    location_id UUID,
    room_id UUID,
    zone_id UUID,
    is_active BOOLEAN NOT NULL DEFAULT true,
    metadata JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT chk_display_name CHECK (LENGTH(display_name) > 0)
);

COMMENT ON TABLE sensor_streams IS 'Configuration for sensor data streams (channels)';
COMMENT ON COLUMN sensor_streams.stream_type IS 'Type of sensor data: Temperature, Humidity, CO2, VPD, etc.';
COMMENT ON COLUMN sensor_streams.unit IS 'Unit of measurement for canonical storage';
COMMENT ON COLUMN sensor_streams.metadata IS 'Additional configuration as JSON';

-- =====================================================
-- TABLE: sensor_readings
-- Description: Time-series sensor readings (hypertable)
-- Note: Will be converted to hypertable in next migration
-- =====================================================
CREATE TABLE IF NOT EXISTS sensor_readings (
    time TIMESTAMPTZ NOT NULL,
    stream_id UUID NOT NULL,
    value DOUBLE PRECISION NOT NULL,
    quality_code SMALLINT NOT NULL DEFAULT 0,
    source_timestamp TIMESTAMPTZ,
    ingestion_timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    message_id VARCHAR(100),
    metadata JSONB,
    
    -- Composite primary key for hypertable
    PRIMARY KEY (time, stream_id),
    
    -- Foreign key to sensor_streams
    CONSTRAINT fk_sensor_readings_stream
        FOREIGN KEY (stream_id)
        REFERENCES sensor_streams(id)
        ON DELETE CASCADE
);

COMMENT ON TABLE sensor_readings IS 'Time-series sensor readings (TimescaleDB hypertable)';
COMMENT ON COLUMN sensor_readings.quality_code IS 'OPC UA quality code (0=Good, 128=Uncertain, 192=Bad)';
COMMENT ON COLUMN sensor_readings.message_id IS 'Idempotency key for deduplication';
COMMENT ON COLUMN sensor_readings.ingestion_timestamp IS 'When the reading was ingested by the system';

-- =====================================================
-- TABLE: alert_rules
-- Description: Alert rule definitions
-- =====================================================
CREATE TABLE IF NOT EXISTS alert_rules (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    site_id UUID NOT NULL,
    rule_name VARCHAR(200) NOT NULL,
    rule_type VARCHAR(50) NOT NULL,
    stream_ids UUID[] NOT NULL,
    threshold_config JSONB NOT NULL,
    evaluation_window_minutes INT NOT NULL DEFAULT 5,
    cooldown_minutes INT NOT NULL DEFAULT 15,
    severity VARCHAR(20) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    notify_channels TEXT[] NOT NULL DEFAULT '{}',
    metadata JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    
    CONSTRAINT chk_rule_name CHECK (LENGTH(rule_name) > 0),
    CONSTRAINT chk_evaluation_window CHECK (evaluation_window_minutes > 0),
    CONSTRAINT chk_cooldown CHECK (cooldown_minutes >= 0),
    CONSTRAINT chk_stream_ids_not_empty CHECK (cardinality(stream_ids) > 0)
);

COMMENT ON TABLE alert_rules IS 'Alert rule definitions for threshold and deviation detection';
COMMENT ON COLUMN alert_rules.rule_type IS 'Threshold, Deviation, Range, RateOfChange';
COMMENT ON COLUMN alert_rules.threshold_config IS 'Rule-specific configuration as JSON';
COMMENT ON COLUMN alert_rules.evaluation_window_minutes IS 'Time window for rule evaluation';
COMMENT ON COLUMN alert_rules.cooldown_minutes IS 'Minimum time between consecutive alerts';

-- =====================================================
-- TABLE: alert_instances
-- Description: Fired alert instances
-- =====================================================
CREATE TABLE IF NOT EXISTS alert_instances (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    site_id UUID NOT NULL,
    rule_id UUID,
    stream_id UUID NOT NULL,
    fired_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    cleared_at TIMESTAMPTZ,
    severity VARCHAR(20) NOT NULL,
    current_value DOUBLE PRECISION,
    threshold_value DOUBLE PRECISION,
    message TEXT NOT NULL,
    acknowledged_at TIMESTAMPTZ,
    acknowledged_by UUID,
    acknowledgment_notes TEXT,
    metadata JSONB,
    
    CONSTRAINT fk_alert_instances_rule
        FOREIGN KEY (rule_id)
        REFERENCES alert_rules(id)
        ON DELETE SET NULL,
    CONSTRAINT fk_alert_instances_stream
        FOREIGN KEY (stream_id)
        REFERENCES sensor_streams(id)
        ON DELETE CASCADE,
    CONSTRAINT chk_cleared_after_fired CHECK (cleared_at IS NULL OR cleared_at >= fired_at),
    CONSTRAINT chk_ack_after_fired CHECK (acknowledged_at IS NULL OR acknowledged_at >= fired_at)
);

COMMENT ON TABLE alert_instances IS 'Fired alert instances with lifecycle tracking';
COMMENT ON COLUMN alert_instances.cleared_at IS 'When the alert condition was resolved';
COMMENT ON COLUMN alert_instances.acknowledged_at IS 'When a user acknowledged the alert';

-- =====================================================
-- TABLE: ingestion_sessions
-- Description: Device connection session tracking
-- =====================================================
CREATE TABLE IF NOT EXISTS ingestion_sessions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    site_id UUID NOT NULL,
    equipment_id UUID NOT NULL,
    protocol VARCHAR(20) NOT NULL,
    started_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    last_heartbeat_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ended_at TIMESTAMPTZ,
    message_count BIGINT NOT NULL DEFAULT 0,
    error_count BIGINT NOT NULL DEFAULT 0,
    metadata JSONB,
    
    CONSTRAINT chk_protocol CHECK (UPPER(protocol) IN ('HTTP','MQTT','OPC','MODBUS')),
    CONSTRAINT chk_message_count CHECK (message_count >= 0),
    CONSTRAINT chk_error_count CHECK (error_count >= 0),
    CONSTRAINT chk_ended_after_started CHECK (ended_at IS NULL OR ended_at >= started_at)
);

COMMENT ON TABLE ingestion_sessions IS 'Device connection session tracking for monitoring';
COMMENT ON COLUMN ingestion_sessions.last_heartbeat_at IS 'Last activity from the device';
COMMENT ON COLUMN ingestion_sessions.message_count IS 'Total messages received in this session';

-- =====================================================
-- TABLE: ingestion_errors
-- Description: Ingestion error log
-- =====================================================
CREATE TABLE IF NOT EXISTS ingestion_errors (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    site_id UUID NOT NULL,
    session_id UUID,
    equipment_id UUID,
    protocol VARCHAR(20) NOT NULL,
    error_type VARCHAR(50) NOT NULL,
    error_message TEXT NOT NULL,
    raw_payload JSONB,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_ingestion_errors_session
        FOREIGN KEY (session_id)
        REFERENCES ingestion_sessions(id)
        ON DELETE SET NULL
);

COMMENT ON TABLE ingestion_errors IS 'Log of ingestion errors for debugging and monitoring';
COMMENT ON COLUMN ingestion_errors.raw_payload IS 'The problematic payload that caused the error';

-- =====================================================
-- INDEXES (Basic indexes, more in next migration)
-- =====================================================

-- sensor_streams indexes
CREATE INDEX IF NOT EXISTS idx_sensor_streams_site_id 
    ON sensor_streams(site_id);

CREATE INDEX IF NOT EXISTS idx_sensor_streams_equipment_id 
    ON sensor_streams(equipment_id);

CREATE INDEX IF NOT EXISTS idx_sensor_streams_site_active 
    ON sensor_streams(site_id, is_active);

-- alert_rules indexes
CREATE INDEX IF NOT EXISTS idx_alert_rules_site_active 
    ON alert_rules(site_id, is_active);

-- alert_instances indexes
CREATE INDEX IF NOT EXISTS idx_alert_instances_rule_fired 
    ON alert_instances(rule_id, fired_at DESC);

CREATE INDEX IF NOT EXISTS idx_alert_instances_stream_fired 
    ON alert_instances(stream_id, fired_at DESC);

CREATE INDEX IF NOT EXISTS idx_alert_instances_site_active 
    ON alert_instances(site_id, fired_at DESC)
    WHERE cleared_at IS NULL;

-- ingestion_sessions indexes
CREATE INDEX IF NOT EXISTS idx_ingestion_sessions_equipment 
    ON ingestion_sessions(equipment_id, started_at DESC);

CREATE INDEX IF NOT EXISTS idx_ingestion_sessions_site_active 
    ON ingestion_sessions(site_id, last_heartbeat_at DESC)
    WHERE ended_at IS NULL;

-- ingestion_errors indexes
CREATE INDEX IF NOT EXISTS idx_ingestion_errors_site_occurred 
    ON ingestion_errors(site_id, occurred_at DESC);

-- =====================================================
-- TRIGGERS
-- =====================================================

-- Update updated_at timestamp automatically
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_sensor_streams_updated_at
    BEFORE UPDATE ON sensor_streams
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trg_alert_rules_updated_at
    BEFORE UPDATE ON alert_rules
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- =====================================================
-- GRANTS (Basic permissions)
-- =====================================================

-- Grant access to authenticated role (adjust as needed)
GRANT SELECT, INSERT, UPDATE ON sensor_streams TO authenticated;
GRANT SELECT, INSERT ON sensor_readings TO authenticated;
GRANT SELECT, INSERT, UPDATE ON alert_rules TO authenticated;
GRANT SELECT, INSERT, UPDATE ON alert_instances TO authenticated;
GRANT SELECT, INSERT, UPDATE ON ingestion_sessions TO authenticated;
GRANT SELECT, INSERT ON ingestion_errors TO authenticated;

-- =====================================================
-- SUCCESS MESSAGE
-- =====================================================

DO $$
BEGIN
    RAISE NOTICE 'Migration 001_initial_schema.sql completed successfully';
    RAISE NOTICE 'Created 6 tables: sensor_streams, sensor_readings, alert_rules, alert_instances, ingestion_sessions, ingestion_errors';
    RAISE NOTICE 'Next: Run 002_timescaledb_setup.sql to enable hypertables and compression';
END $$;

