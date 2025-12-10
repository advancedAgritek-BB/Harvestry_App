-- Migration: Extend harvests table for full workflow support
-- Version: 001
-- Description: Adds workflow phase tracking, drying, bucking, metrics, and batching support

-- Create harvest phase enum
DO $$ BEGIN
    CREATE TYPE harvest_phase AS ENUM (
        'wet_harvest', 'drying', 'bucking', 'dry_weighed', 'batched', 'lot_created', 'complete'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create harvest batching mode enum
DO $$ BEGIN
    CREATE TYPE harvest_batching_mode AS ENUM (
        'single_strain', 'mixed_strain', 'sub_lot'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Add workflow columns to harvests table
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS phase harvest_phase DEFAULT 'wet_harvest';

-- Drying tracking
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS drying_start_date DATE;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS drying_end_date DATE;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS drying_duration_days INT;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS drying_location_id UUID;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS drying_location_name VARCHAR(200);

-- Bucking tracking
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS bucking_date DATE;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS bucked_flower_weight DECIMAL(12, 4) DEFAULT 0;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS total_stem_waste DECIMAL(12, 4) DEFAULT 0;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS total_leaf_waste DECIMAL(12, 4) DEFAULT 0;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS total_other_waste DECIMAL(12, 4) DEFAULT 0;

-- Calculated metrics
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS moisture_loss_percent DECIMAL(5, 2);
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS dry_to_wet_ratio DECIMAL(5, 4);
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS usable_flower_percent DECIMAL(5, 2);
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS total_waste_percent DECIMAL(5, 2);

-- Weight lock status
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS wet_weight_locked BOOLEAN DEFAULT FALSE;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS dry_weight_locked BOOLEAN DEFAULT FALSE;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS wet_weight_locked_at TIMESTAMPTZ;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS dry_weight_locked_at TIMESTAMPTZ;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS wet_weight_locked_by_user_id UUID;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS dry_weight_locked_by_user_id UUID;

-- Batching
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS batching_mode harvest_batching_mode;
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS parent_harvest_id UUID REFERENCES harvests(id);
ALTER TABLE harvests ADD COLUMN IF NOT EXISTS child_harvest_ids UUID[] DEFAULT '{}';

-- Indexes
CREATE INDEX IF NOT EXISTS idx_harvests_phase ON harvests(phase);
CREATE INDEX IF NOT EXISTS idx_harvests_drying_location ON harvests(drying_location_id) WHERE drying_location_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_harvests_parent ON harvests(parent_harvest_id) WHERE parent_harvest_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_harvests_batching_mode ON harvests(batching_mode) WHERE batching_mode IS NOT NULL;

-- Comments
COMMENT ON COLUMN harvests.phase IS 'Current phase in harvest workflow: wet_harvest → drying → bucking → dry_weighed → batched → lot_created → complete';
COMMENT ON COLUMN harvests.moisture_loss_percent IS 'Calculated: (wet - dry) / wet * 100';
COMMENT ON COLUMN harvests.dry_to_wet_ratio IS 'Calculated: dry / wet';
COMMENT ON COLUMN harvests.usable_flower_percent IS 'Calculated: bucked flower / dry * 100';
COMMENT ON COLUMN harvests.total_waste_percent IS 'Calculated: total waste / wet * 100';
COMMENT ON COLUMN harvests.wet_weight_locked IS 'When true, wet weight requires PIN override to adjust';
COMMENT ON COLUMN harvests.dry_weight_locked IS 'When true, dry weight requires PIN override to adjust';




