-- Migration: 001_CreateIrrigationOrchestrationTables.sql
-- Description: Creates tables for FRP-06 Irrigation Orchestration
-- Dependencies: FRP-02 (Spatial/Equipment), FRP-05 (Telemetry)

-- =====================================================
-- IRRIGATION GROUPS
-- =====================================================
CREATE TABLE IF NOT EXISTS irrigation_groups (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    code VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    max_concurrent_valves INT NOT NULL DEFAULT 6,
    pump_equipment_id UUID REFERENCES equipment(id),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by_user_id UUID REFERENCES users(id),
    CONSTRAINT uq_irrigation_groups_site_code UNIQUE (site_id, code),
    CONSTRAINT ck_irrigation_groups_max_valves CHECK (max_concurrent_valves BETWEEN 1 AND 24)
);

CREATE INDEX IF NOT EXISTS idx_irrigation_groups_site ON irrigation_groups(site_id);

-- =====================================================
-- IRRIGATION GROUP ZONES
-- =====================================================
CREATE TABLE IF NOT EXISTS irrigation_group_zones (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    group_id UUID NOT NULL REFERENCES irrigation_groups(id) ON DELETE CASCADE,
    zone_id UUID NOT NULL,
    priority INT NOT NULL DEFAULT 1,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_irrigation_group_zones UNIQUE (group_id, zone_id)
);

CREATE INDEX IF NOT EXISTS idx_irrigation_group_zones_group ON irrigation_group_zones(group_id);

-- =====================================================
-- IRRIGATION RUNS
-- =====================================================
CREATE TABLE IF NOT EXISTS irrigation_runs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    program_id UUID NOT NULL,
    group_id UUID NOT NULL REFERENCES irrigation_groups(id),
    schedule_id UUID,
    status VARCHAR(30) NOT NULL,
    current_step_index INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    total_steps INT NOT NULL DEFAULT 1,
    completed_steps INT NOT NULL DEFAULT 0,
    aborted_by_user_id UUID REFERENCES users(id),
    abort_reason VARCHAR(500),
    interlock_type VARCHAR(50),
    interlock_details VARCHAR(1000),
    fault_message VARCHAR(1000),
    initiated_by_user_id UUID REFERENCES users(id),
    initiated_by VARCHAR(50) NOT NULL DEFAULT 'system'
);

CREATE INDEX IF NOT EXISTS idx_irrigation_runs_site ON irrigation_runs(site_id);
CREATE INDEX IF NOT EXISTS idx_irrigation_runs_status ON irrigation_runs(status);
CREATE INDEX IF NOT EXISTS idx_irrigation_runs_group ON irrigation_runs(group_id);
CREATE INDEX IF NOT EXISTS idx_irrigation_runs_active ON irrigation_runs(site_id, status)
    WHERE status IN ('Queued', 'Running', 'Paused');

-- =====================================================
-- INTERLOCK EVENTS
-- =====================================================
CREATE TABLE IF NOT EXISTS interlock_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    run_id UUID REFERENCES irrigation_runs(id),
    group_id UUID REFERENCES irrigation_groups(id),
    interlock_type VARCHAR(50) NOT NULL,
    details VARCHAR(1000) NOT NULL,
    sensor_stream_id VARCHAR(100),
    sensor_value DECIMAL(10, 4),
    threshold_value DECIMAL(10, 4),
    was_preventive BOOLEAN NOT NULL DEFAULT FALSE,
    tripped_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    cleared_at TIMESTAMPTZ,
    cleared_by_user_id UUID REFERENCES users(id),
    clearance_notes VARCHAR(500),
    requires_acknowledgment BOOLEAN NOT NULL DEFAULT FALSE,
    acknowledged_at TIMESTAMPTZ,
    acknowledged_by_user_id UUID REFERENCES users(id)
);

CREATE INDEX IF NOT EXISTS idx_interlock_events_site ON interlock_events(site_id);
CREATE INDEX IF NOT EXISTS idx_interlock_events_run ON interlock_events(run_id);
CREATE INDEX IF NOT EXISTS idx_interlock_events_active ON interlock_events(site_id, cleared_at)
    WHERE cleared_at IS NULL;

-- =====================================================
-- DEVICE COMMANDS (OUTBOX)
-- =====================================================
CREATE TABLE IF NOT EXISTS device_commands (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    run_id UUID REFERENCES irrigation_runs(id),
    equipment_id UUID NOT NULL REFERENCES equipment(id),
    command_type VARCHAR(50) NOT NULL,
    status VARCHAR(30) NOT NULL,
    priority VARCHAR(20) NOT NULL DEFAULT 'Normal',
    payload_json JSONB NOT NULL,
    correlation_id VARCHAR(100),
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    sent_at TIMESTAMPTZ,
    acknowledged_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    retry_count INT NOT NULL DEFAULT 0,
    max_retries INT NOT NULL DEFAULT 3,
    error_message VARCHAR(1000),
    response_json JSONB,
    timeout_seconds INT NOT NULL DEFAULT 30
);

CREATE INDEX IF NOT EXISTS idx_device_commands_site ON device_commands(site_id);
CREATE INDEX IF NOT EXISTS idx_device_commands_run ON device_commands(run_id);
CREATE INDEX IF NOT EXISTS idx_device_commands_status ON device_commands(status);
CREATE INDEX IF NOT EXISTS idx_device_commands_pending ON device_commands(site_id, status, priority, created_at)
    WHERE status IN ('Pending', 'Failed');
CREATE INDEX IF NOT EXISTS idx_device_commands_correlation ON device_commands(correlation_id);

-- =====================================================
-- RLS POLICIES
-- =====================================================
ALTER TABLE irrigation_groups ENABLE ROW LEVEL SECURITY;
ALTER TABLE irrigation_group_zones ENABLE ROW LEVEL SECURITY;
ALTER TABLE irrigation_runs ENABLE ROW LEVEL SECURITY;
ALTER TABLE interlock_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE device_commands ENABLE ROW LEVEL SECURITY;

-- Irrigation Groups: Site-scoped
CREATE POLICY irrigation_groups_site_isolation ON irrigation_groups
    FOR ALL
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID
           OR current_setting('app.current_user_id', TRUE) = 'service_account');

-- Group Zones: Through group
CREATE POLICY irrigation_group_zones_site_isolation ON irrigation_group_zones
    FOR ALL
    USING (group_id IN (
        SELECT id FROM irrigation_groups 
        WHERE site_id = current_setting('app.current_site_id', TRUE)::UUID
    ) OR current_setting('app.current_user_id', TRUE) = 'service_account');

-- Runs: Site-scoped
CREATE POLICY irrigation_runs_site_isolation ON irrigation_runs
    FOR ALL
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID
           OR current_setting('app.current_user_id', TRUE) = 'service_account');

-- Interlock Events: Site-scoped
CREATE POLICY interlock_events_site_isolation ON interlock_events
    FOR ALL
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID
           OR current_setting('app.current_user_id', TRUE) = 'service_account');

-- Device Commands: Site-scoped
CREATE POLICY device_commands_site_isolation ON device_commands
    FOR ALL
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID
           OR current_setting('app.current_user_id', TRUE) = 'service_account');

-- =====================================================
-- TRIGGERS
-- =====================================================
CREATE OR REPLACE FUNCTION update_irrigation_groups_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_irrigation_groups_updated_at
    BEFORE UPDATE ON irrigation_groups
    FOR EACH ROW
    EXECUTE FUNCTION update_irrigation_groups_updated_at();

-- =====================================================
-- COMMENTS
-- =====================================================
COMMENT ON TABLE irrigation_groups IS 'FRP-06: Groups of zones sharing pump/flow constraints';
COMMENT ON TABLE irrigation_group_zones IS 'FRP-06: Zone membership in irrigation groups';
COMMENT ON TABLE irrigation_runs IS 'FRP-06: Irrigation program execution instances';
COMMENT ON TABLE interlock_events IS 'FRP-06: Safety interlock trip records';
COMMENT ON TABLE device_commands IS 'FRP-06: Outbox for device commands (MQTT)';

COMMENT ON COLUMN irrigation_runs.status IS 'Queued, Running, Completed, Aborted, InterlockTripped, Faulted, Paused';
COMMENT ON COLUMN interlock_events.interlock_type IS 'EmergencyStop, DoorOpen, TankLevelLow, EcOutOfBounds, PhOutOfBounds, Co2Lockout, MaxRuntimeExceeded, etc.';
COMMENT ON COLUMN device_commands.command_type IS 'ValveOpen, ValveClose, PumpStart, PumpStop, InjectorStart, InjectorStop, EmergencyCloseAll';
COMMENT ON COLUMN device_commands.priority IS 'Low, Normal, High, Emergency';
