-- RLS Policy Template: Site-Scoped Table
-- Apply this template to any table that contains site-specific data
-- 
-- Usage:
--   1. Replace YOUR_TABLE_NAME with actual table name
--   2. Verify table has site_id UUID column
--   3. Apply this migration
--   4. Test with different user roles

-- Enable RLS
ALTER TABLE YOUR_TABLE_NAME ENABLE ROW LEVEL SECURITY;

-- Policy 1: Site isolation for regular users
-- Users can only access rows for sites they're assigned to
CREATE POLICY YOUR_TABLE_NAME_site_isolation ON YOUR_TABLE_NAME
FOR ALL
USING (
    site_id IN (
        SELECT us.site_id 
        FROM user_sites us
        WHERE us.user_id = current_setting('app.current_user_id', TRUE)::UUID
    )
);

-- Policy 2: Service account bypass
-- Background workers need access to all sites
CREATE POLICY YOUR_TABLE_NAME_service_account ON YOUR_TABLE_NAME
FOR ALL
USING (current_setting('app.user_role', TRUE) = 'service_account');

-- Policy 3 (Optional): Cross-site read-only for admins
-- Uncomment if admins need visibility across all sites
-- CREATE POLICY YOUR_TABLE_NAME_admin_readonly ON YOUR_TABLE_NAME
-- FOR SELECT
-- USING (current_setting('app.user_role', TRUE) IN ('admin', 'executive'));

-- Add table comment
COMMENT ON TABLE YOUR_TABLE_NAME IS 'Site-scoped table with RLS policies enforcing data isolation';

-- Test queries (run in transaction, then rollback)
-- 
-- -- Test 1: User with site access
-- BEGIN;
-- SET app.current_user_id = 'user-uuid-here';
-- SET app.user_role = 'operator';
-- SELECT COUNT(*) FROM YOUR_TABLE_NAME; -- Should return only user's sites
-- ROLLBACK;
-- 
-- -- Test 2: Service account
-- BEGIN;
-- SET app.user_role = 'service_account';
-- SELECT COUNT(*) FROM YOUR_TABLE_NAME; -- Should return all rows
-- ROLLBACK;
-- 
-- -- Test 3: User without site access
-- BEGIN;
-- SET app.current_user_id = 'user-without-sites';
-- SET app.user_role = 'operator';
-- SELECT COUNT(*) FROM YOUR_TABLE_NAME; -- Should return 0
-- ROLLBACK;
