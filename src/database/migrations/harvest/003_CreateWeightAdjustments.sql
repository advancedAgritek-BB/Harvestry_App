-- Migration: Create weight adjustments table
-- Version: 003
-- Description: Tracks all weight adjustments with full audit trail

-- Create weight type enum
DO $$ BEGIN
    CREATE TYPE weight_type AS ENUM (
        'wet_plant', 'dry_plant', 'bucked_flower', 'stem_waste', 'leaf_waste', 'other_waste'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create weight adjustments table
CREATE TABLE IF NOT EXISTS harvest_weight_adjustments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- What was adjusted
    harvest_id UUID NOT NULL REFERENCES harvests(id) ON DELETE CASCADE,
    harvest_plant_id UUID REFERENCES harvest_plants(id) ON DELETE CASCADE,
    weight_type weight_type NOT NULL,
    
    -- Weight values
    previous_weight DECIMAL(12, 4) NOT NULL,
    new_weight DECIMAL(12, 4) NOT NULL,
    adjustment_amount DECIMAL(12, 4) NOT NULL,
    
    -- Reason and notes
    reason_code VARCHAR(50) NOT NULL,
    notes TEXT,
    
    -- Audit
    adjusted_by_user_id UUID NOT NULL,
    pin_override_used BOOLEAN DEFAULT FALSE,
    adjusted_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Indexes
CREATE INDEX idx_weight_adj_harvest ON harvest_weight_adjustments(harvest_id);
CREATE INDEX idx_weight_adj_plant ON harvest_weight_adjustments(harvest_plant_id) WHERE harvest_plant_id IS NOT NULL;
CREATE INDEX idx_weight_adj_user ON harvest_weight_adjustments(adjusted_by_user_id);
CREATE INDEX idx_weight_adj_date ON harvest_weight_adjustments(adjusted_at);
CREATE INDEX idx_weight_adj_pin ON harvest_weight_adjustments(pin_override_used) WHERE pin_override_used = TRUE;

-- Comments
COMMENT ON TABLE harvest_weight_adjustments IS 'Audit trail for all weight adjustments in harvest workflow';
COMMENT ON COLUMN harvest_weight_adjustments.reason_code IS 'Reason code: SCALE_ERROR, RECOUNTED, DATA_ENTRY_ERROR, SPILLAGE, etc.';
COMMENT ON COLUMN harvest_weight_adjustments.pin_override_used IS 'TRUE if user entered PIN to override a locked weight';




