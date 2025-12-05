-- =====================================================
-- FRP05: Telemetry Service - Row-Level Security (RLS)
-- Migration: 004_rls_policies.sql
-- Description: Multi-tenant security policies
-- Author: AI Agent
-- Date: 2025-10-02
-- Dependencies: 001_initial_schema.sql
-- =====================================================

-- =====================================================
-- ENABLE RLS ON ALL TABLES
-- =====================================================

ALTER TABLE sensor_streams ENABLE ROW LEVEL SECURITY;
ALTER TABLE sensor_readings ENABLE ROW LEVEL SECURITY;
ALTER TABLE alert_rules ENABLE ROW LEVEL SECURITY;
ALTER TABLE alert_instances ENABLE ROW LEVEL SECURITY;
ALTER TABLE ingestion_sessions ENABLE ROW LEVEL SECURITY;
ALTER TABLE ingestion_errors ENABLE ROW LEVEL SECURITY;

-- =====================================================
-- HELPER FUNCTION: Get user's accessible site IDs
-- =====================================================

CREATE OR REPLACE FUNCTION auth.get_user_site_ids()
RETURNS UUID[] AS $$
DECLARE
    user_sites UUID[];
BEGIN
    -- Get site IDs the current user has access to
    -- This assumes you have a user_sites or user_roles table
    -- Adjust based on your actual auth schema
    
    SELECT ARRAY_AGG(site_id)
    INTO user_sites
    FROM user_sites
    WHERE user_id = auth.uid();
    
    -- If user_sites is empty, return empty array (no access)
    RETURN COALESCE(user_sites, ARRAY[]::UUID[]);
EXCEPTION
    WHEN OTHERS THEN
        -- If table doesn't exist or error occurs, return empty array
        RETURN ARRAY[]::UUID[];
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

COMMENT ON FUNCTION auth.get_user_site_ids() IS 'Returns array of site IDs the current user can access';

-- =====================================================
-- HELPER FUNCTION: Check if user is admin
-- =====================================================

CREATE OR REPLACE FUNCTION auth.is_admin()
RETURNS BOOLEAN AS $$
DECLARE
    user_id UUID;
BEGIN
    -- Get current user ID
    user_id := auth.uid();

    -- Return FALSE if no authenticated user
    IF user_id IS NULL THEN
        RETURN FALSE;
    END IF;

    -- Check if current user has admin role
    -- Adjust based on your actual auth schema
    RETURN EXISTS (
        SELECT 1
        FROM user_roles
        WHERE user_id = user_id
          AND role = 'admin'
    );
EXCEPTION
    WHEN OTHERS THEN
        RETURN FALSE;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

COMMENT ON FUNCTION auth.is_admin() IS 'Returns true if current user is an admin';

-- =====================================================
-- RLS POLICIES: sensor_streams
-- =====================================================

-- Policy: Users can select streams for their sites
CREATE POLICY sensor_streams_select_policy
    ON sensor_streams
    FOR SELECT
    USING (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    );

-- Policy: Users can insert streams for their sites
CREATE POLICY sensor_streams_insert_policy
    ON sensor_streams
    FOR INSERT
    WITH CHECK (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    );

-- Policy: Users can update streams for their sites
CREATE POLICY sensor_streams_update_policy
    ON sensor_streams
    FOR UPDATE
    USING (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    )
    WITH CHECK (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    );

-- Policy: Only admins can delete streams
CREATE POLICY sensor_streams_delete_policy
    ON sensor_streams
    FOR DELETE
    USING (auth.is_admin());

-- =====================================================
-- RLS POLICIES: sensor_readings
-- =====================================================

-- Policy: Users can select readings for their sites
-- Uses a join to sensor_streams to get site_id
CREATE POLICY sensor_readings_select_policy
    ON sensor_readings
    FOR SELECT
    USING (
        auth.is_admin()
        OR EXISTS (
            SELECT 1
            FROM sensor_streams ss
            WHERE ss.id = sensor_readings.stream_id
              AND ss.site_id = ANY(auth.get_user_site_ids())
        )
    );

-- Policy: Users can insert readings for their sites
CREATE POLICY sensor_readings_insert_policy
    ON sensor_readings
    FOR INSERT
    WITH CHECK (
        auth.is_admin()
        OR EXISTS (
            SELECT 1
            FROM sensor_streams ss
            WHERE ss.id = sensor_readings.stream_id
              AND ss.site_id = ANY(auth.get_user_site_ids())
        )
    );

-- Policy: Only admins can update/delete readings
-- (readings should be immutable for normal users)
CREATE POLICY sensor_readings_update_policy
    ON sensor_readings
    FOR UPDATE
    USING (auth.is_admin());

CREATE POLICY sensor_readings_delete_policy
    ON sensor_readings
    FOR DELETE
    USING (auth.is_admin());

-- =====================================================
-- RLS POLICIES: alert_rules
-- =====================================================

-- Policy: Users can select alert rules for their sites
CREATE POLICY alert_rules_select_policy
    ON alert_rules
    FOR SELECT
    USING (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    );

-- Policy: Users can insert alert rules for their sites
CREATE POLICY alert_rules_insert_policy
    ON alert_rules
    FOR INSERT
    WITH CHECK (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    );

-- Policy: Users can update alert rules for their sites
CREATE POLICY alert_rules_update_policy
    ON alert_rules
    FOR UPDATE
    USING (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    )
    WITH CHECK (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    );

-- Policy: Users can delete alert rules for their sites
CREATE POLICY alert_rules_delete_policy
    ON alert_rules
    FOR DELETE
    USING (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    );

-- =====================================================
-- RLS POLICIES: alert_instances
-- =====================================================

-- Policy: Users can select alert instances for their sites
CREATE POLICY alert_instances_select_policy
    ON alert_instances
    FOR SELECT
    USING (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    );

-- Policy: Service/admin can insert alert instances
-- Alert instances are created by the system
CREATE POLICY alert_instances_insert_policy
    ON alert_instances
    FOR INSERT
    WITH CHECK (
        current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
        OR auth.is_admin()
    );

-- Policy: Users can update alert instances for their sites
-- (for acknowledgment)
CREATE POLICY alert_instances_update_policy
    ON alert_instances
    FOR UPDATE
    USING (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    )
    WITH CHECK (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    );

-- Policy: Only admins can delete alert instances
CREATE POLICY alert_instances_delete_policy
    ON alert_instances
    FOR DELETE
    USING (auth.is_admin());

-- =====================================================
-- RLS POLICIES: ingestion_sessions
-- =====================================================

-- Policy: Users can select sessions for their sites
CREATE POLICY ingestion_sessions_select_policy
    ON ingestion_sessions
    FOR SELECT
    USING (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    );

-- Policy: Service/admin can insert sessions
CREATE POLICY ingestion_sessions_insert_policy
    ON ingestion_sessions
    FOR INSERT
    WITH CHECK (
        current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
        OR auth.is_admin()
    );

-- Policy: Service/admin can update sessions
CREATE POLICY ingestion_sessions_update_policy
    ON ingestion_sessions
    FOR UPDATE
    USING (
        current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
        OR auth.is_admin()
    );

-- Policy: Only admins can delete sessions
CREATE POLICY ingestion_sessions_delete_policy
    ON ingestion_sessions
    FOR DELETE
    USING (auth.is_admin());

-- =====================================================
-- RLS POLICIES: ingestion_errors
-- =====================================================

-- Policy: Users can select errors for their sites
CREATE POLICY ingestion_errors_select_policy
    ON ingestion_errors
    FOR SELECT
    USING (
        auth.is_admin()
        OR site_id = ANY(auth.get_user_site_ids())
    );

-- Policy: System/service can insert errors
CREATE POLICY ingestion_errors_insert_policy
    ON ingestion_errors
    FOR INSERT
    WITH CHECK (
        current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
        OR auth.is_admin()
    );

-- Policy: Only admins can update/delete errors
CREATE POLICY ingestion_errors_update_policy
    ON ingestion_errors
    FOR UPDATE
    USING (auth.is_admin());

CREATE POLICY ingestion_errors_delete_policy
    ON ingestion_errors
    FOR DELETE
    USING (auth.is_admin());

-- =====================================================
-- BYPASS RLS FOR SERVICE ROLE
-- =====================================================

-- Grant bypass RLS to service role for background workers
-- Adjust role name based on your setup
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'service_role') THEN
        GRANT ALL ON sensor_streams TO service_role;
        GRANT ALL ON sensor_readings TO service_role;
        GRANT ALL ON alert_rules TO service_role;
        GRANT ALL ON alert_instances TO service_role;
        GRANT ALL ON ingestion_sessions TO service_role;
        GRANT ALL ON ingestion_errors TO service_role;
        
        -- Allow service role to bypass RLS
        ALTER ROLE service_role BYPASSRLS;
        
        RAISE NOTICE 'Granted permissions to service_role';
    ELSE
        RAISE NOTICE 'service_role not found, skipping grants';
    END IF;
END $$;

-- =====================================================
-- VERIFICATION FUNCTIONS
-- =====================================================

-- Function to test RLS policies
CREATE OR REPLACE FUNCTION test_rls_policies()
RETURNS TABLE (
    table_name TEXT,
    rls_enabled BOOLEAN,
    policy_count INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        c.relname::TEXT,
        c.relrowsecurity,
        COUNT(p.polname)::INT
    FROM pg_class c
    LEFT JOIN pg_policy p ON p.polrelid = c.oid
    WHERE c.relnamespace = 'public'::regnamespace
      AND c.relname IN (
          'sensor_streams', 'sensor_readings', 'alert_rules',
          'alert_instances', 'ingestion_sessions', 'ingestion_errors'
      )
    GROUP BY c.relname, c.relrowsecurity
    ORDER BY c.relname;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION test_rls_policies() IS 'Check RLS status and policy count for telemetry tables';

-- =====================================================
-- VERIFICATION
-- =====================================================

DO $$
DECLARE
    table_rec RECORD;
    total_policies INT := 0;
BEGIN
    RAISE NOTICE '========================================';
    RAISE NOTICE 'RLS STATUS:';
    RAISE NOTICE '========================================';
    
    FOR table_rec IN
        SELECT * FROM test_rls_policies()
    LOOP
        RAISE NOTICE '% - RLS: %, Policies: %',
            table_rec.table_name,
            CASE WHEN table_rec.rls_enabled THEN '✓' ELSE '✗' END,
            table_rec.policy_count;
        total_policies := total_policies + table_rec.policy_count;
    END LOOP;
    
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Total policies created: %', total_policies;
END $$;

-- =====================================================
-- SUCCESS MESSAGE
-- =====================================================

DO $$
BEGIN
    RAISE NOTICE '';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Migration 004_rls_policies.sql completed successfully';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'Security features added:';
    RAISE NOTICE '  ✓ RLS enabled on all 6 tables';
    RAISE NOTICE '  ✓ Multi-tenant isolation by site_id';
    RAISE NOTICE '  ✓ Admin override capabilities';
    RAISE NOTICE '  ✓ Service role bypass for workers';
    RAISE NOTICE '  ✓ Separate policies for SELECT/INSERT/UPDATE/DELETE';
    RAISE NOTICE '';
    RAISE NOTICE 'Next: Run 005_seed_data.sql for test data';
    RAISE NOTICE '========================================';
END $$;

