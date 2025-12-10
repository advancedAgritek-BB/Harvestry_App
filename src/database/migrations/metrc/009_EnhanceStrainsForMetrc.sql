-- Migration: Enhance strains table with METRC fields
-- Version: 009
-- Description: Adds METRC compliance fields to existing strains table

-- Create genetic classification enum
DO $$ BEGIN
    CREATE TYPE genetic_classification AS ENUM ('indica', 'sativa', 'hybrid', 'unspecified');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create strain testing status enum
DO $$ BEGIN
    CREATE TYPE strain_testing_status AS ENUM ('none', 'in_progress', 'complete');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Add METRC-specific columns to strains table
ALTER TABLE strains ADD COLUMN IF NOT EXISTS genetic_classification genetic_classification DEFAULT 'unspecified';
ALTER TABLE strains ADD COLUMN IF NOT EXISTS testing_status strain_testing_status DEFAULT 'none';
ALTER TABLE strains ADD COLUMN IF NOT EXISTS nominal_thc_percent DECIMAL(6, 3);
ALTER TABLE strains ADD COLUMN IF NOT EXISTS nominal_cbd_percent DECIMAL(6, 3);
ALTER TABLE strains ADD COLUMN IF NOT EXISTS metrc_strain_id BIGINT;
ALTER TABLE strains ADD COLUMN IF NOT EXISTS metrc_last_sync_at TIMESTAMPTZ;
ALTER TABLE strains ADD COLUMN IF NOT EXISTS metrc_sync_status TEXT;

-- Add indexes
CREATE INDEX IF NOT EXISTS idx_strains_genetic_classification 
    ON strains(genetic_classification);

CREATE INDEX IF NOT EXISTS idx_strains_metrc_strain_id 
    ON strains(metrc_strain_id) 
    WHERE metrc_strain_id IS NOT NULL;

COMMENT ON COLUMN strains.genetic_classification IS 'Indica, Sativa, Hybrid classification';
COMMENT ON COLUMN strains.testing_status IS 'Internal testing status';
COMMENT ON COLUMN strains.nominal_thc_percent IS 'Expected/nominal THC percentage';
COMMENT ON COLUMN strains.nominal_cbd_percent IS 'Expected/nominal CBD percentage';
COMMENT ON COLUMN strains.metrc_strain_id IS 'METRC internal strain identifier';








