-- Migration: Add METRC fields to sites table
-- Version: 001
-- Description: Enhances sites table with METRC facility integration fields

-- Add METRC-specific columns to sites table
ALTER TABLE sites ADD COLUMN IF NOT EXISTS metrc_facility_id BIGINT;
ALTER TABLE sites ADD COLUMN IF NOT EXISTS facility_type SMALLINT DEFAULT 0;
ALTER TABLE sites ADD COLUMN IF NOT EXISTS metrc_api_key_encrypted TEXT;
ALTER TABLE sites ADD COLUMN IF NOT EXISTS metrc_user_key_encrypted TEXT;
ALTER TABLE sites ADD COLUMN IF NOT EXISTS state_code VARCHAR(2);
ALTER TABLE sites ADD COLUMN IF NOT EXISTS is_metrc_enabled BOOLEAN DEFAULT FALSE;
ALTER TABLE sites ADD COLUMN IF NOT EXISTS metrc_last_sync_at TIMESTAMPTZ;
ALTER TABLE sites ADD COLUMN IF NOT EXISTS metrc_sync_status TEXT;

-- Add index for METRC-enabled sites
CREATE INDEX IF NOT EXISTS idx_sites_metrc_enabled 
    ON sites(is_metrc_enabled) 
    WHERE is_metrc_enabled = TRUE;

-- Add index for state code (for jurisdiction-specific queries)
CREATE INDEX IF NOT EXISTS idx_sites_state_code 
    ON sites(state_code) 
    WHERE state_code IS NOT NULL;

COMMENT ON COLUMN sites.metrc_facility_id IS 'METRC internal facility identifier';
COMMENT ON COLUMN sites.facility_type IS 'Facility type: 0=Cultivator, 1=Processor, 2=CultivatorProcessor, 3=Lab';
COMMENT ON COLUMN sites.metrc_api_key_encrypted IS 'Encrypted METRC vendor API key';
COMMENT ON COLUMN sites.metrc_user_key_encrypted IS 'Encrypted METRC user API key';
COMMENT ON COLUMN sites.state_code IS 'Two-letter state code (IL, CO, NY, etc.)';
COMMENT ON COLUMN sites.is_metrc_enabled IS 'Whether METRC integration is enabled for this site';
COMMENT ON COLUMN sites.metrc_last_sync_at IS 'Last successful METRC sync timestamp';
COMMENT ON COLUMN sites.metrc_sync_status IS 'Current METRC sync status message';









