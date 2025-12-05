-- Migration: Create harvests schema
-- Version: 004
-- Description: Creates harvest tables for METRC harvest tracking

-- Create harvest type enum
DO $$ BEGIN
    CREATE TYPE harvest_type AS ENUM ('whole_plant', 'manicure');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create harvest status enum
DO $$ BEGIN
    CREATE TYPE harvest_status AS ENUM ('active', 'on_hold', 'finished');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create harvest waste type enum
DO $$ BEGIN
    CREATE TYPE harvest_waste_type AS ENUM (
        'plant_material', 'fibrous_material', 'trim', 'roots', 'stems', 'other'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create harvests table
CREATE TABLE IF NOT EXISTS harvests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    harvest_name VARCHAR(200) NOT NULL,
    harvest_type harvest_type NOT NULL DEFAULT 'whole_plant',
    strain_id UUID NOT NULL,
    strain_name VARCHAR(200) NOT NULL,
    
    -- Location
    location_id UUID,
    location_name VARCHAR(200),
    sublocation_name VARCHAR(100),
    
    -- Dates
    harvest_start_date DATE NOT NULL,
    harvest_end_date DATE,
    drying_date DATE,
    
    -- Weights
    total_wet_weight DECIMAL(12, 4) NOT NULL DEFAULT 0,
    total_dry_weight DECIMAL(12, 4) NOT NULL DEFAULT 0,
    current_weight DECIMAL(12, 4) NOT NULL DEFAULT 0,
    total_waste_weight DECIMAL(12, 4) NOT NULL DEFAULT 0,
    unit_of_weight VARCHAR(20) NOT NULL DEFAULT 'Grams',
    
    -- Status
    status harvest_status NOT NULL DEFAULT 'active',
    notes TEXT,
    
    -- METRC sync
    metrc_harvest_id BIGINT,
    metrc_last_sync_at TIMESTAMPTZ,
    metrc_sync_status TEXT,
    
    -- Metadata
    metadata_json JSONB DEFAULT '{}',
    
    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL
);

-- Create harvest_plants junction table
CREATE TABLE IF NOT EXISTS harvest_plants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    harvest_id UUID NOT NULL REFERENCES harvests(id) ON DELETE CASCADE,
    plant_id UUID NOT NULL,
    plant_tag VARCHAR(30) NOT NULL,
    wet_weight DECIMAL(12, 4) NOT NULL,
    unit_of_weight VARCHAR(20) NOT NULL DEFAULT 'Grams',
    harvested_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_harvest_plants UNIQUE(harvest_id, plant_id)
);

-- Create harvest_waste table
CREATE TABLE IF NOT EXISTS harvest_waste (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    harvest_id UUID NOT NULL REFERENCES harvests(id) ON DELETE CASCADE,
    waste_type harvest_waste_type NOT NULL,
    waste_weight DECIMAL(12, 4) NOT NULL,
    unit_of_weight VARCHAR(20) NOT NULL DEFAULT 'Grams',
    waste_method waste_method NOT NULL,
    actual_date DATE NOT NULL,
    recorded_by_user_id UUID NOT NULL,
    notes TEXT,
    recorded_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    metrc_waste_id BIGINT
);

-- Indexes for harvests
CREATE INDEX IF NOT EXISTS idx_harvests_site_id ON harvests(site_id);
CREATE INDEX IF NOT EXISTS idx_harvests_strain_id ON harvests(strain_id);
CREATE INDEX IF NOT EXISTS idx_harvests_status ON harvests(status);
CREATE INDEX IF NOT EXISTS idx_harvests_harvest_start_date ON harvests(harvest_start_date);
CREATE INDEX IF NOT EXISTS idx_harvests_metrc_harvest_id ON harvests(metrc_harvest_id) WHERE metrc_harvest_id IS NOT NULL;

-- Indexes for harvest_plants
CREATE INDEX IF NOT EXISTS idx_harvest_plants_harvest_id ON harvest_plants(harvest_id);
CREATE INDEX IF NOT EXISTS idx_harvest_plants_plant_id ON harvest_plants(plant_id);

-- Indexes for harvest_waste
CREATE INDEX IF NOT EXISTS idx_harvest_waste_harvest_id ON harvest_waste(harvest_id);
CREATE INDEX IF NOT EXISTS idx_harvest_waste_actual_date ON harvest_waste(actual_date);

COMMENT ON TABLE harvests IS 'Cannabis harvest records for METRC tracking';
COMMENT ON TABLE harvest_plants IS 'Plants included in each harvest';
COMMENT ON TABLE harvest_waste IS 'Waste recorded during harvest processing';



