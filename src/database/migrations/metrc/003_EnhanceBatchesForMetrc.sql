-- Migration: Enhance batches table with METRC-specific fields
-- Version: 003
-- Description: Adds METRC compliance fields to existing batches table

-- Create propagation type enum
DO $$ BEGIN
    CREATE TYPE propagation_type AS ENUM ('clone', 'seed', 'plant_material');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Add METRC-specific columns to batches table
ALTER TABLE batches ADD COLUMN IF NOT EXISTS metrc_plant_batch_id BIGINT;
ALTER TABLE batches ADD COLUMN IF NOT EXISTS propagation_type propagation_type DEFAULT 'clone';
ALTER TABLE batches ADD COLUMN IF NOT EXISTS patient_license_number VARCHAR(50);
ALTER TABLE batches ADD COLUMN IF NOT EXISTS sublocation_name VARCHAR(100);
ALTER TABLE batches ADD COLUMN IF NOT EXISTS source_package_label VARCHAR(30);
ALTER TABLE batches ADD COLUMN IF NOT EXISTS source_package_adjustment_amount DECIMAL(12, 4);
ALTER TABLE batches ADD COLUMN IF NOT EXISTS source_package_adjustment_uom VARCHAR(20);
ALTER TABLE batches ADD COLUMN IF NOT EXISTS actual_date DATE;
ALTER TABLE batches ADD COLUMN IF NOT EXISTS strain_name VARCHAR(200);
ALTER TABLE batches ADD COLUMN IF NOT EXISTS location_name VARCHAR(200);
ALTER TABLE batches ADD COLUMN IF NOT EXISTS metrc_last_sync_at TIMESTAMPTZ;
ALTER TABLE batches ADD COLUMN IF NOT EXISTS metrc_sync_status TEXT;

-- Add indexes
CREATE INDEX IF NOT EXISTS idx_batches_metrc_plant_batch_id 
    ON batches(metrc_plant_batch_id) 
    WHERE metrc_plant_batch_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_batches_propagation_type 
    ON batches(propagation_type);

CREATE INDEX IF NOT EXISTS idx_batches_source_package_label 
    ON batches(source_package_label) 
    WHERE source_package_label IS NOT NULL;

COMMENT ON COLUMN batches.metrc_plant_batch_id IS 'METRC internal plant batch identifier';
COMMENT ON COLUMN batches.propagation_type IS 'Propagation method: clone, seed, or plant_material';
COMMENT ON COLUMN batches.patient_license_number IS 'Patient license for medical batches';
COMMENT ON COLUMN batches.sublocation_name IS 'Sub-location within primary location';
COMMENT ON COLUMN batches.source_package_label IS 'Source package label when created from package';
COMMENT ON COLUMN batches.source_package_adjustment_amount IS 'Quantity used from source package';
COMMENT ON COLUMN batches.source_package_adjustment_uom IS 'Unit of measure for source adjustment';
COMMENT ON COLUMN batches.actual_date IS 'Actual date for METRC reporting';
COMMENT ON COLUMN batches.strain_name IS 'Cached strain name for METRC';
COMMENT ON COLUMN batches.location_name IS 'Cached location name for METRC';









