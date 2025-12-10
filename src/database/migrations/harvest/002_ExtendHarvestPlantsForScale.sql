-- Migration: Extend harvest_plants table for scale integration
-- Version: 002
-- Description: Adds scale reading link, weight source, and weight locking to harvest plants

-- Add scale and lock columns to harvest_plants table
ALTER TABLE harvest_plants ADD COLUMN IF NOT EXISTS scale_reading_id UUID;
ALTER TABLE harvest_plants ADD COLUMN IF NOT EXISTS weight_source VARCHAR(20) DEFAULT 'manual';
ALTER TABLE harvest_plants ADD COLUMN IF NOT EXISTS is_weight_locked BOOLEAN DEFAULT FALSE;
ALTER TABLE harvest_plants ADD COLUMN IF NOT EXISTS weight_locked_at TIMESTAMPTZ;
ALTER TABLE harvest_plants ADD COLUMN IF NOT EXISTS weight_locked_by_user_id UUID;

-- Indexes
CREATE INDEX IF NOT EXISTS idx_harvest_plants_scale_reading 
    ON harvest_plants(scale_reading_id) 
    WHERE scale_reading_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_harvest_plants_locked 
    ON harvest_plants(is_weight_locked) 
    WHERE is_weight_locked = TRUE;

-- Comments
COMMENT ON COLUMN harvest_plants.scale_reading_id IS 'Reference to scale_readings table if weight came from scale';
COMMENT ON COLUMN harvest_plants.weight_source IS 'Source of weight: scale or manual';
COMMENT ON COLUMN harvest_plants.is_weight_locked IS 'When true, weight requires PIN override to adjust';




