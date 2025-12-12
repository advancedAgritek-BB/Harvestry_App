-- ============================================================================
-- Knowledge: Crop Steering Tables
-- Migration: Create Crop Steering Schema (Profiles, Reference Data, Response Curves)
-- ----------------------------------------------------------------------------
-- This migration creates the relational model for crop steering knowledge:
--   * Crop steering profiles (site defaults + strain-specific overrides)
--   * Reference steering levers (EC, VWC, VPD, Temperature, etc.)
--   * Reference irrigation signals (shot size, intervals, dryback targets)
--   * Cultivar response curves for MPC optimization
-- All tables are scoped by site_id to support RLS policies.
-- ============================================================================

CREATE SCHEMA IF NOT EXISTS knowledge;

-- ---------------------------------------------------------------------------
-- 1) Crop Steering Profiles
-- ---------------------------------------------------------------------------
-- Profiles define steering parameters. When strain_id is NULL, it's a site default.
-- Strain-specific profiles can override site defaults.

CREATE TABLE IF NOT EXISTS knowledge.crop_steering_profiles (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    strain_id UUID REFERENCES genetics.strains(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    target_mode VARCHAR(20) NOT NULL CHECK (target_mode IN ('Vegetative', 'Generative', 'Balanced')),
    
    -- Configuration stored as JSONB (SteeringConfiguration struct)
    -- Contains: levers, signals, dryback_targets, p1_config, p2_config, p3_config
    configuration JSONB NOT NULL DEFAULT '{}'::JSONB,
    
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    
    -- Site can have one default profile (strain_id is null) per steering mode
    CONSTRAINT uq_site_default_profile UNIQUE (site_id, target_mode) 
        WHERE strain_id IS NULL,
    -- Strain can have one profile per site
    CONSTRAINT uq_strain_profile UNIQUE (site_id, strain_id) 
        WHERE strain_id IS NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_crop_steering_profiles_site ON knowledge.crop_steering_profiles(site_id);
CREATE INDEX IF NOT EXISTS ix_crop_steering_profiles_strain ON knowledge.crop_steering_profiles(strain_id) WHERE strain_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_crop_steering_profiles_mode ON knowledge.crop_steering_profiles(site_id, target_mode);

-- ---------------------------------------------------------------------------
-- 2) Reference Steering Levers (Immutable Reference Data)
-- ---------------------------------------------------------------------------
-- These are the canonical steering levers based on crop steering science.
-- Used as defaults when creating new profiles.

CREATE TABLE IF NOT EXISTS knowledge.steering_lever_references (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    metric_name VARCHAR(50) NOT NULL UNIQUE,
    display_name VARCHAR(100) NOT NULL,
    description TEXT,
    vegetative_trend VARCHAR(30) NOT NULL,
    generative_trend VARCHAR(30) NOT NULL,
    vegetative_min_value NUMERIC(10,2),
    vegetative_max_value NUMERIC(10,2),
    generative_min_value NUMERIC(10,2),
    generative_max_value NUMERIC(10,2),
    unit VARCHAR(30) NOT NULL,
    stream_type_id INTEGER, -- Maps to telemetry StreamType enum
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ---------------------------------------------------------------------------
-- 3) Reference Irrigation Signals (Immutable Reference Data)
-- ---------------------------------------------------------------------------
-- Canonical irrigation signals for vegetative vs generative steering.

CREATE TABLE IF NOT EXISTS knowledge.irrigation_signal_references (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    signal_name VARCHAR(50) NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    description TEXT,
    vegetative_value VARCHAR(100) NOT NULL,
    generative_value VARCHAR(100) NOT NULL,
    applicable_phase VARCHAR(30) NOT NULL, -- 'P1', 'P2', 'P3', 'P1,P2', 'All'
    vegetative_min_value NUMERIC(10,2),
    vegetative_max_value NUMERIC(10,2),
    generative_min_value NUMERIC(10,2),
    generative_max_value NUMERIC(10,2),
    unit VARCHAR(50) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT uq_signal_phase UNIQUE (signal_name, applicable_phase)
);

-- ---------------------------------------------------------------------------
-- 4) Cultivar Response Curves (For MPC Foundation)
-- ---------------------------------------------------------------------------
-- Response curves model how a specific strain responds to environmental parameters.
-- Used by Autosteer MPC for optimization decisions.

CREATE TABLE IF NOT EXISTS knowledge.cultivar_response_curves (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    strain_id UUID NOT NULL REFERENCES genetics.strains(id) ON DELETE CASCADE,
    
    -- Growth phase this curve applies to (vegetative, flowering, etc.)
    growth_phase VARCHAR(30) NOT NULL,
    
    -- Type of response curve (VPD, EC, Temperature, VWC, etc.)
    curve_type VARCHAR(30) NOT NULL,
    
    -- Response curve data points (x, y pairs)
    -- Example: [{"x": 1.0, "y": 80}, {"x": 1.2, "y": 95}, {"x": 1.4, "y": 100}, {"x": 1.6, "y": 90}]
    data_points JSONB NOT NULL,
    
    -- Optimal value for this metric (peak of curve)
    optimal_value NUMERIC(10,3),
    
    -- Vegetative steering zone
    vegetative_zone_min NUMERIC(10,3),
    vegetative_zone_max NUMERIC(10,3),
    
    -- Generative steering zone
    generative_zone_min NUMERIC(10,3),
    generative_zone_max NUMERIC(10,3),
    
    -- Metadata
    source VARCHAR(100), -- Where this data came from (e.g., 'user_observed', 'research', 'inferred')
    confidence_score NUMERIC(3,2), -- 0.00 to 1.00
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    
    CONSTRAINT uq_response_curve UNIQUE (site_id, strain_id, growth_phase, curve_type)
);

CREATE INDEX IF NOT EXISTS ix_cultivar_response_curves_strain ON knowledge.cultivar_response_curves(strain_id);
CREATE INDEX IF NOT EXISTS ix_cultivar_response_curves_site ON knowledge.cultivar_response_curves(site_id);
CREATE INDEX IF NOT EXISTS ix_cultivar_response_curves_type ON knowledge.cultivar_response_curves(curve_type);

-- ---------------------------------------------------------------------------
-- 5) Phase Configuration History (Audit Trail)
-- ---------------------------------------------------------------------------
-- Track changes to phase configurations for compliance and analysis.

CREATE TABLE IF NOT EXISTS knowledge.steering_profile_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    profile_id UUID NOT NULL REFERENCES knowledge.crop_steering_profiles(id) ON DELETE CASCADE,
    previous_configuration JSONB NOT NULL,
    new_configuration JSONB NOT NULL,
    change_reason TEXT,
    changed_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    changed_by_user_id UUID NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_steering_profile_history_profile ON knowledge.steering_profile_history(profile_id);
CREATE INDEX IF NOT EXISTS ix_steering_profile_history_date ON knowledge.steering_profile_history(changed_at);

-- ---------------------------------------------------------------------------
-- Comments
-- ---------------------------------------------------------------------------
COMMENT ON TABLE knowledge.crop_steering_profiles IS 'Crop steering profiles defining irrigation and environmental targets for vegetative or generative growth';
COMMENT ON COLUMN knowledge.crop_steering_profiles.strain_id IS 'NULL indicates site-wide default; non-null indicates strain-specific override';
COMMENT ON COLUMN knowledge.crop_steering_profiles.configuration IS 'Complete SteeringConfiguration JSONB: levers, signals, dryback_targets, p1/p2/p3_config';

COMMENT ON TABLE knowledge.steering_lever_references IS 'Reference data for steering levers (EC, VWC, VPD, Temperature, etc.)';
COMMENT ON COLUMN knowledge.steering_lever_references.stream_type_id IS 'Maps to telemetry StreamType enum for sensor data correlation';

COMMENT ON TABLE knowledge.irrigation_signal_references IS 'Reference data for irrigation signals (shot size, intervals, dryback)';
COMMENT ON COLUMN knowledge.irrigation_signal_references.applicable_phase IS 'Daily irrigation phase(s): P1 (Ramp), P2 (Maintenance), P3 (Dryback), or All';

COMMENT ON TABLE knowledge.cultivar_response_curves IS 'Strain-specific response curves for MPC optimization';
COMMENT ON COLUMN knowledge.cultivar_response_curves.data_points IS 'Array of {x, y} points defining the response curve';

