-- =====================================================================
-- FRP-02: Equipment Registry Tables
-- File: 20250930_02_CreateEquipmentTables.sql
-- Description: Equipment management including type registry, channels,
--              calibrations, and valve-zone mappings
-- =====================================================================

-- =====================================================================
-- 1) Core Equipment Types (Stable Enum)
-- =====================================================================

CREATE TYPE core_equipment_type AS ENUM (
    'controller',
    'sensor',
    'actuator',
    'injector',
    'pump',
    'valve',
    'meter',
    'ec_ph_controller',
    'mix_tank'
);

-- Equipment status enum
CREATE TYPE equipment_status AS ENUM (
    'Active',
    'Inactive',
    'Maintenance',
    'Faulty'
);

-- =====================================================================
-- 2) Equipment Type Registry (Per-Org Custom Types)
-- =====================================================================

CREATE TABLE equipment_type_registry (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    org_id UUID NOT NULL REFERENCES organizations(organization_id) ON DELETE CASCADE,
    type_code VARCHAR(100) NOT NULL,  -- e.g., 'uvc_chamber', 'co2_generator'
    core_enum core_equipment_type NOT NULL,  -- Maps to core type for governance
    display_name VARCHAR(200) NOT NULL,
    description TEXT,
    schema_json JSONB,  -- JSON schema for type-specific fields
    icon VARCHAR(50),  -- UI icon identifier
    
    -- Template metadata (for built-in types like HSES12, HydroCore, etc.)
    is_template BOOLEAN NOT NULL DEFAULT false,
    template_name VARCHAR(100),  -- e.g., 'HSES12', 'HSEA24', 'HydroCore'
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    
    CONSTRAINT equipment_types_code_org_unique UNIQUE (org_id, type_code)
);

CREATE INDEX idx_equipment_types_org_id ON equipment_type_registry(org_id);
CREATE INDEX idx_equipment_types_core_enum ON equipment_type_registry(core_enum);
CREATE INDEX idx_equipment_types_template ON equipment_type_registry(is_template, template_name);

COMMENT ON TABLE equipment_type_registry IS 'Per-organization equipment type registry with core enum mapping';
COMMENT ON COLUMN equipment_type_registry.core_enum IS 'Maps custom type to core enum for policy/UI grouping';

-- =====================================================================
-- 3) Equipment Registry
-- =====================================================================

CREATE TABLE equipment (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    
    -- Identification
    code VARCHAR(100) NOT NULL,  -- Site-unique identifier
    type_code VARCHAR(100) NOT NULL,  -- References equipment_type_registry
    core_type core_equipment_type NOT NULL,  -- Denormalized for fast queries
    
    -- Status
    status equipment_status NOT NULL DEFAULT 'Active',
    installed_at TIMESTAMPTZ,
    decommissioned_at TIMESTAMPTZ,
    
    -- Location
    location_id UUID REFERENCES inventory_locations(id) ON DELETE SET NULL,
    
    -- Hardware details
    manufacturer VARCHAR(200),
    model VARCHAR(200),
    serial_number VARCHAR(200),
    firmware_version VARCHAR(50),
    
    -- Network attributes (nullable - discovered/assigned)
    ip_address INET,
    mac_address MACADDR,
    mqtt_topic VARCHAR(500),
    
    -- Device twin (capabilities, channel map, discovered metadata)
    device_twin_json JSONB,
    
    -- Calibration tracking
    last_calibration_at TIMESTAMPTZ,
    next_calibration_due_at TIMESTAMPTZ,
    calibration_interval_days INTEGER,
    
    -- Health (current snapshot - history in equipment_health_history)
    last_heartbeat_at TIMESTAMPTZ,
    online BOOLEAN NOT NULL DEFAULT FALSE,
    signal_strength_dbm INTEGER,
    battery_percent INTEGER,
    error_count INTEGER DEFAULT 0,
    uptime_seconds BIGINT,
    
    -- Metadata
    notes TEXT,
    metadata_json JSONB,
    
    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    
    CONSTRAINT equipment_code_site_unique UNIQUE (site_id, code),
    CONSTRAINT equipment_battery_pct_range CHECK (battery_percent BETWEEN 0 AND 100),
    CONSTRAINT equipment_decommissioned_check CHECK (
        decommissioned_at IS NULL OR decommissioned_at >= installed_at
    )
);

CREATE INDEX idx_equipment_site_id ON equipment(site_id);
CREATE INDEX idx_equipment_location_id ON equipment(location_id);
CREATE INDEX idx_equipment_type_code ON equipment(type_code);
CREATE INDEX idx_equipment_core_type ON equipment(core_type);
CREATE INDEX idx_equipment_status ON equipment(status);
CREATE INDEX idx_equipment_online ON equipment(online);
CREATE INDEX idx_equipment_calibration_due ON equipment(next_calibration_due_at) 
    WHERE next_calibration_due_at IS NOT NULL;
CREATE INDEX idx_equipment_mqtt_topic ON equipment(mqtt_topic) 
    WHERE mqtt_topic IS NOT NULL;

COMMENT ON TABLE equipment IS 'Equipment registry with network attributes and device twin';
COMMENT ON COLUMN equipment.device_twin_json IS 'Discovered capabilities, channel map, and runtime metadata';
COMMENT ON COLUMN equipment.online IS 'Flag indicating if device is considered online (managed by application logic)';

-- =====================================================================
-- 4) Equipment Channels (Multi-Channel Device Support)
-- =====================================================================

CREATE TABLE equipment_channels (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_id UUID NOT NULL REFERENCES equipment(id) ON DELETE CASCADE,
    
    -- Channel identification
    channel_code VARCHAR(100) NOT NULL,  -- e.g., 'CH01', 'VALVE_13'
    role VARCHAR(200),  -- e.g., 'zone_substrate_ec', 'valve_ch_13_HL'
    
    -- Port/address metadata
    port_meta_json JSONB,  -- Hardware port config (e.g., DI/DO pin, relay index)
    
    -- Status
    enabled BOOLEAN NOT NULL DEFAULT true,
    
    -- Assignment (links to zones, etc.)
    assigned_zone_id UUID REFERENCES inventory_locations(id) ON DELETE SET NULL,
    
    -- Metadata
    notes TEXT,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT equipment_channels_code_unique UNIQUE (equipment_id, channel_code)
);

CREATE INDEX idx_equipment_channels_equipment_id ON equipment_channels(equipment_id);
CREATE INDEX idx_equipment_channels_zone_id ON equipment_channels(assigned_zone_id);
CREATE INDEX idx_equipment_channels_role ON equipment_channels(role);

COMMENT ON TABLE equipment_channels IS 'Multi-channel device support (e.g., 12-channel EC sensor, 24-valve controller)';
COMMENT ON COLUMN equipment_channels.role IS 'Semantic role for channel (e.g., substrate_ec, valve_HL_zone_A)';

-- =====================================================================
-- 5) Equipment Calibrations
-- =====================================================================

-- Calibration method enum
CREATE TYPE calibration_method AS ENUM (
    'Single',    -- Single-point calibration
    'TwoPoint',  -- Two-point calibration (4-20mA, pH, etc.)
    'MultiPoint' -- Multi-point calibration curve
);

-- Calibration result enum
CREATE TYPE calibration_result AS ENUM (
    'Pass',
    'Fail',
    'WithinTolerance',
    'OutOfTolerance'
);

CREATE TABLE equipment_calibrations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    equipment_id UUID NOT NULL REFERENCES equipment(id) ON DELETE CASCADE,
    channel_code VARCHAR(100),  -- NULL if whole equipment, specific channel otherwise
    
    -- Calibration details
    method calibration_method NOT NULL,
    reference_value DECIMAL(12,4),  -- Known good reference
    measured_value DECIMAL(12,4),   -- Device reading
    
    -- Calibration coefficients (for multi-point or correction curves)
    coefficients_json JSONB,
    
    -- Result
    result calibration_result NOT NULL,
    deviation DECIMAL(12,4),  -- abs(measured - reference)
    deviation_pct DECIMAL(6,2),  -- (deviation / reference) * 100
    
    -- Metadata
    performed_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    performed_by_user_id UUID NOT NULL,
    next_due_at TIMESTAMPTZ,
    
    -- Documentation
    notes TEXT,
    attachment_url TEXT,  -- Link to certificate/documentation
    
    -- Equipment state at time of calibration
    firmware_version_at_calibration VARCHAR(50),
    
    CONSTRAINT calibration_deviation_pct_check CHECK (deviation_pct >= 0)
);

CREATE INDEX idx_calibrations_equipment_id ON equipment_calibrations(equipment_id);
CREATE INDEX idx_calibrations_performed_at ON equipment_calibrations(performed_at DESC);
CREATE INDEX idx_calibrations_result ON equipment_calibrations(result);
CREATE INDEX idx_calibrations_next_due ON equipment_calibrations(next_due_at) 
    WHERE next_due_at IS NOT NULL;

COMMENT ON TABLE equipment_calibrations IS 'Equipment calibration history with multi-point support';
COMMENT ON COLUMN equipment_calibrations.coefficients_json IS 'Calibration curve coefficients for correction formulas';

-- =====================================================================
-- 6) Valve-Zone Mappings (Many-to-Many)
-- =====================================================================

CREATE TABLE valve_zone_mappings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    
    -- Valve (equipment or channel)
    valve_equipment_id UUID NOT NULL REFERENCES equipment(id) ON DELETE CASCADE,
    valve_channel_code VARCHAR(100),  -- NULL if single-valve equipment
    
    -- Zone (target location)
    zone_location_id UUID NOT NULL REFERENCES inventory_locations(id) ON DELETE CASCADE,
    
    -- Routing priority (for matrix routing)
    priority INTEGER NOT NULL DEFAULT 1,
    
    -- Control mode
    normally_open BOOLEAN NOT NULL DEFAULT false,
    interlock_group VARCHAR(100),  -- Mutual exclusion group
    
    -- Status
    enabled BOOLEAN NOT NULL DEFAULT true,
    
    -- Metadata
    notes TEXT,
    
    -- Audit
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    
    CONSTRAINT valve_zone_unique UNIQUE (valve_equipment_id, valve_channel_code, zone_location_id)
);

CREATE INDEX idx_valve_zone_site_id ON valve_zone_mappings(site_id);
CREATE INDEX idx_valve_zone_equipment_id ON valve_zone_mappings(valve_equipment_id);
CREATE INDEX idx_valve_zone_location_id ON valve_zone_mappings(zone_location_id);
CREATE INDEX idx_valve_zone_interlock ON valve_zone_mappings(interlock_group) 
    WHERE interlock_group IS NOT NULL;

COMMENT ON TABLE valve_zone_mappings IS 'Many-to-many valve-to-zone routing matrix';
COMMENT ON COLUMN valve_zone_mappings.interlock_group IS 'Mutual exclusion group for safety interlocks';

-- =====================================================================
-- 7) Fault Reason Codes (Reference Data)
-- =====================================================================

CREATE TABLE fault_reason_codes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) NOT NULL UNIQUE,
    category VARCHAR(100) NOT NULL,  -- e.g., 'Network', 'Sensor', 'Actuator', 'Safety'
    description TEXT NOT NULL,
    severity VARCHAR(20) NOT NULL,  -- 'Critical', 'High', 'Medium', 'Low'
    recommended_action TEXT,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_fault_codes_category ON fault_reason_codes(category);
CREATE INDEX idx_fault_codes_severity ON fault_reason_codes(severity);

COMMENT ON TABLE fault_reason_codes IS 'Reference data for standardized fault codes';

-- =====================================================================
-- 8) Row-Level Security (RLS) Policies
-- =====================================================================

-- Enable RLS
ALTER TABLE equipment_type_registry ENABLE ROW LEVEL SECURITY;
ALTER TABLE equipment ENABLE ROW LEVEL SECURITY;
ALTER TABLE equipment_channels ENABLE ROW LEVEL SECURITY;
ALTER TABLE equipment_calibrations ENABLE ROW LEVEL SECURITY;
ALTER TABLE valve_zone_mappings ENABLE ROW LEVEL SECURITY;

-- Equipment Type Registry: Org-scoped
CREATE POLICY equipment_types_org_isolation ON equipment_type_registry
    USING (
        org_id IN (
            SELECT org_id 
            FROM sites 
            WHERE site_id IN (
                SELECT site_id 
                FROM user_sites 
                WHERE user_id = current_setting('app.current_user_id', true)::UUID
            )
        )
    );

-- Equipment: Site-scoped
CREATE POLICY equipment_site_isolation ON equipment
    USING (
        site_id IN (
            SELECT site_id 
            FROM user_sites 
            WHERE user_id = current_setting('app.current_user_id', true)::UUID
        )
    );

-- Equipment Channels: Via equipment site
CREATE POLICY equipment_channels_site_isolation ON equipment_channels
    USING (
        equipment_id IN (
            SELECT id 
            FROM equipment 
            WHERE site_id IN (
                SELECT site_id 
                FROM user_sites 
                WHERE user_id = current_setting('app.current_user_id', true)::UUID
            )
        )
    );

-- Equipment Calibrations: Via equipment site
CREATE POLICY equipment_calibrations_site_isolation ON equipment_calibrations
    USING (
        equipment_id IN (
            SELECT id 
            FROM equipment 
            WHERE site_id IN (
                SELECT site_id 
                FROM user_sites 
                WHERE user_id = current_setting('app.current_user_id', true)::UUID
            )
        )
    );

-- Valve-Zone Mappings: Site-scoped
CREATE POLICY valve_zone_site_isolation ON valve_zone_mappings
    USING (
        site_id IN (
            SELECT site_id 
            FROM user_sites 
            WHERE user_id = current_setting('app.current_user_id', true)::UUID
        )
    );

-- =====================================================================
-- 9) Grants
-- =====================================================================

GRANT SELECT, INSERT, UPDATE, DELETE ON equipment_type_registry TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON equipment TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON equipment_channels TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON equipment_calibrations TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON valve_zone_mappings TO harvestry_app;
GRANT SELECT ON fault_reason_codes TO harvestry_app;

GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO harvestry_app;

-- =====================================================================
-- End of Equipment Tables Migration
-- =====================================================================
