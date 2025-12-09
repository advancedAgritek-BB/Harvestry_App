-- ============================================================================
-- Super Admin Setup Script for Harvestry
-- 
-- Run this in Supabase SQL Editor to set up the super admin user
-- This creates: organization, site, user record, and site assignment
-- ============================================================================

-- Step 1: Create organizations table if it doesn't exist
-- (This should be part of the identity migration, but may not have been run)
CREATE TABLE IF NOT EXISTS organizations (
    organization_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL,
    status VARCHAR(20) DEFAULT 'active'
        CHECK (status IN ('active', 'inactive', 'pending', 'suspended')),
    metadata JSONB DEFAULT '{}'::JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID,
    updated_by UUID
);

CREATE INDEX IF NOT EXISTS ix_organizations_slug ON organizations (slug);
CREATE INDEX IF NOT EXISTS ix_organizations_status ON organizations (status) WHERE status = 'active';

COMMENT ON TABLE organizations IS 'Top-level business entities that own one or more sites/brands';

-- Enable RLS on organizations if not already enabled
ALTER TABLE organizations ENABLE ROW LEVEL SECURITY;

-- Create RLS policy for organizations (if not exists)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_policies WHERE tablename = 'organizations' AND policyname = 'organizations_admin_access'
    ) THEN
        CREATE POLICY organizations_admin_access ON organizations
        FOR ALL
        USING (current_setting('app.user_role', TRUE) IN ('admin', 'service_account'));
    END IF;
END $$;

-- Grant permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON organizations TO authenticated;
GRANT SELECT, INSERT, UPDATE, DELETE ON organizations TO service_role;

-- ============================================================================
-- Step 2: Create the Harvestry organization
-- ============================================================================
INSERT INTO organizations (organization_id, name, slug, status)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    'Harvestry',
    'harvestry',
    'active'
)
ON CONFLICT (organization_id) DO NOTHING;

-- ============================================================================
-- Step 3: Add org_id to sites table if missing (migration may have failed)
-- ============================================================================
DO $$
BEGIN
    -- Check if org_id column exists
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'sites' AND column_name = 'org_id'
    ) THEN
        ALTER TABLE sites ADD COLUMN org_id UUID REFERENCES organizations(organization_id) ON DELETE CASCADE;
    END IF;
END $$;

-- ============================================================================
-- Step 4: Create the default Harvestry site
-- ============================================================================
INSERT INTO sites (site_id, org_id, site_name, site_code, timezone, status, site_type)
VALUES (
    '22222222-2222-2222-2222-222222222222',
    '11111111-1111-1111-1111-111111111111',
    'Harvestry HQ',
    'HRV-HQ-01',
    'America/Denver',
    'active',
    'cultivation'
)
ON CONFLICT (site_id) DO UPDATE SET
    site_name = EXCLUDED.site_name,
    org_id = EXCLUDED.org_id;

-- ============================================================================
-- Step 5: Create the super admin user record
-- IMPORTANT: The user_id MUST match the Supabase Auth user id
-- Your Supabase Auth user ID: 1c7c28bd-04c2-42fa-a06e-2d3a78421baf
-- ============================================================================
INSERT INTO users (user_id, email, first_name, last_name, display_name, email_verified, status)
VALUES (
    '1c7c28bd-04c2-42fa-a06e-2d3a78421baf',  -- Must match Supabase Auth user ID
    'bburnette@advancedagritek.com',
    'Brandon',
    'Burnette',
    'Brandon Burnette',
    TRUE,
    'active'
)
ON CONFLICT (user_id) DO UPDATE SET
    email = EXCLUDED.email,
    first_name = EXCLUDED.first_name,
    last_name = EXCLUDED.last_name,
    display_name = EXCLUDED.display_name,
    email_verified = EXCLUDED.email_verified;

-- ============================================================================
-- Step 6: Get the admin role ID and assign user to site
-- ============================================================================
DO $$
DECLARE
    v_admin_role_id UUID;
BEGIN
    -- Get the admin role ID
    SELECT role_id INTO v_admin_role_id
    FROM roles
    WHERE role_name = 'admin';

    IF v_admin_role_id IS NULL THEN
        RAISE EXCEPTION 'Admin role not found. Please ensure roles are seeded.';
    END IF;

    -- Create user_sites assignment
    INSERT INTO user_sites (user_id, site_id, role_id, is_primary_site, assigned_by)
    VALUES (
        '1c7c28bd-04c2-42fa-a06e-2d3a78421baf',  -- User ID
        '22222222-2222-2222-2222-222222222222',  -- Site ID
        v_admin_role_id,                         -- Admin role
        TRUE,                                    -- Primary site
        '1c7c28bd-04c2-42fa-a06e-2d3a78421baf'  -- Self-assigned
    )
    ON CONFLICT (user_id, site_id) DO UPDATE SET
        role_id = EXCLUDED.role_id,
        is_primary_site = EXCLUDED.is_primary_site;

    RAISE NOTICE '✓ Super admin user assigned to site with admin role';
END $$;

-- ============================================================================
-- Step 7: Verify the setup
-- ============================================================================
SELECT 
    'Organization' as entity,
    (SELECT COUNT(*) FROM organizations WHERE organization_id = '11111111-1111-1111-1111-111111111111')::TEXT as count
UNION ALL
SELECT 
    'Site' as entity,
    (SELECT COUNT(*) FROM sites WHERE site_id = '22222222-2222-2222-2222-222222222222')::TEXT as count
UNION ALL
SELECT 
    'User' as entity,
    (SELECT COUNT(*) FROM users WHERE user_id = '1c7c28bd-04c2-42fa-a06e-2d3a78421baf')::TEXT as count
UNION ALL
SELECT 
    'User-Site Assignment' as entity,
    (SELECT COUNT(*) FROM user_sites WHERE user_id = '1c7c28bd-04c2-42fa-a06e-2d3a78421baf')::TEXT as count;

-- ============================================================================
-- Setup Complete!
-- 
-- Your super admin user:
--   Email: bburnette@advancedagritek.com
--   User ID: 1c7c28bd-04c2-42fa-a06e-2d3a78421baf
--   Role: admin
--   Site: Harvestry HQ (HRV-HQ-01)
--   Organization: Harvestry
-- ============================================================================
