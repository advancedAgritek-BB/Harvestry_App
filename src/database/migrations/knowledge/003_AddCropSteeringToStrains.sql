-- ============================================================================
-- Knowledge: Add Crop Steering Profile Reference to Strains
-- Migration: Extends strains table with crop steering profile reference
-- ----------------------------------------------------------------------------
-- Adds an optional foreign key from strains to crop_steering_profiles,
-- allowing strain-specific steering configurations to override site defaults.
-- ============================================================================

-- Add crop_steering_profile_id column to strains
ALTER TABLE genetics.strains 
ADD COLUMN IF NOT EXISTS crop_steering_profile_id UUID 
    REFERENCES knowledge.crop_steering_profiles(id) 
    ON DELETE SET NULL;

-- Create index for profile lookups
CREATE INDEX IF NOT EXISTS ix_strains_crop_steering_profile 
    ON genetics.strains(crop_steering_profile_id) 
    WHERE crop_steering_profile_id IS NOT NULL;

-- Add comment
COMMENT ON COLUMN genetics.strains.crop_steering_profile_id IS 
    'Optional reference to a strain-specific crop steering profile. If null, site default profile applies.';
