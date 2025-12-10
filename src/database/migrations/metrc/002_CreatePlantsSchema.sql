-- Migration: Create plants schema for individual tagged plants
-- Version: 002
-- Description: Creates plants table for METRC individual plant tracking

-- Create plant growth phase enum type
DO $$ BEGIN
    CREATE TYPE plant_growth_phase AS ENUM (
        'immature', 'vegetative', 'flowering', 'mother', 'harvested', 'destroyed', 'inactive'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create plant status enum type
DO $$ BEGIN
    CREATE TYPE plant_status AS ENUM (
        'active', 'on_hold', 'harvested', 'destroyed', 'inactive'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create plant destroy reason enum type
DO $$ BEGIN
    CREATE TYPE plant_destroy_reason AS ENUM (
        'disease', 'quality_failure', 'regulatory_compliance', 'plant_death',
        'male_plant', 'hermaphrodite', 'contamination', 'culling', 'failed_lab_test', 'other'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create waste method enum type
DO $$ BEGIN
    CREATE TYPE waste_method AS ENUM (
        'grinder', 'compost', 'incinerator', 'mixed_waste', 'other'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create plants table
CREATE TABLE IF NOT EXISTS plants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    plant_tag VARCHAR(30) NOT NULL,
    batch_id UUID NOT NULL,
    strain_id UUID NOT NULL,
    strain_name VARCHAR(200) NOT NULL,
    
    -- Growth tracking
    growth_phase plant_growth_phase NOT NULL DEFAULT 'immature',
    status plant_status NOT NULL DEFAULT 'active',
    planted_date DATE NOT NULL,
    vegetative_date DATE,
    flowering_date DATE,
    
    -- Location
    location_id UUID,
    sublocation_name VARCHAR(100),
    
    -- Medical tracking
    patient_license_number VARCHAR(50),
    
    -- Harvest tracking
    harvest_id UUID,
    harvest_date DATE,
    harvest_wet_weight DECIMAL(12, 4),
    harvest_weight_unit VARCHAR(20),
    
    -- Destruction tracking
    destroyed_date DATE,
    destroy_reason plant_destroy_reason,
    destroy_reason_note TEXT,
    waste_weight DECIMAL(12, 4),
    waste_weight_unit VARCHAR(20),
    waste_method waste_method,
    destroyed_by_user_id UUID,
    destroy_witness_user_id UUID,
    
    -- METRC sync
    metrc_plant_id BIGINT,
    metrc_last_sync_at TIMESTAMPTZ,
    metrc_sync_status TEXT,
    
    -- Notes and metadata
    notes TEXT,
    metadata_json JSONB DEFAULT '{}',
    
    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL,
    
    CONSTRAINT uq_plants_tag_site UNIQUE(site_id, plant_tag)
);

-- Indexes for plants
CREATE INDEX IF NOT EXISTS idx_plants_site_id ON plants(site_id);
CREATE INDEX IF NOT EXISTS idx_plants_batch_id ON plants(batch_id);
CREATE INDEX IF NOT EXISTS idx_plants_strain_id ON plants(strain_id);
CREATE INDEX IF NOT EXISTS idx_plants_status ON plants(status);
CREATE INDEX IF NOT EXISTS idx_plants_growth_phase ON plants(growth_phase);
CREATE INDEX IF NOT EXISTS idx_plants_location_id ON plants(location_id) WHERE location_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_plants_harvest_id ON plants(harvest_id) WHERE harvest_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_plants_metrc_plant_id ON plants(metrc_plant_id) WHERE metrc_plant_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_plants_plant_tag ON plants(plant_tag);

-- Create plant events table
CREATE TABLE IF NOT EXISTS plant_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    plant_id UUID NOT NULL REFERENCES plants(id) ON DELETE CASCADE,
    event_type SMALLINT NOT NULL,
    user_id UUID NOT NULL,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    event_data JSONB DEFAULT '{}'
);

CREATE INDEX IF NOT EXISTS idx_plant_events_plant_id ON plant_events(plant_id);
CREATE INDEX IF NOT EXISTS idx_plant_events_site_id ON plant_events(site_id);
CREATE INDEX IF NOT EXISTS idx_plant_events_occurred_at ON plant_events(occurred_at);

COMMENT ON TABLE plants IS 'Individual tagged cannabis plants for METRC tracking';
COMMENT ON TABLE plant_events IS 'Audit trail for plant lifecycle events';








