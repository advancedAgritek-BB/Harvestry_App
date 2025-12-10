-- Migration: Enable RLS for Inventory Tables
-- Version: 005
-- Description: Row-level security policies for inventory movements and related tables

-- ============================================================================
-- ENABLE RLS ON INVENTORY_MOVEMENTS
-- ============================================================================
ALTER TABLE inventory_movements ENABLE ROW LEVEL SECURITY;

-- Policy for site-scoped access
CREATE POLICY inventory_movements_site_isolation ON inventory_movements
    FOR ALL
    USING (site_id = COALESCE(
        current_setting('app.current_site_id', true)::uuid,
        '00000000-0000-0000-0000-000000000000'::uuid
    ));

-- Policy for insert
CREATE POLICY inventory_movements_insert ON inventory_movements
    FOR INSERT
    WITH CHECK (site_id = COALESCE(
        current_setting('app.current_site_id', true)::uuid,
        '00000000-0000-0000-0000-000000000000'::uuid
    ));

COMMENT ON POLICY inventory_movements_site_isolation ON inventory_movements IS 'Ensures users can only access movements for their site';
COMMENT ON POLICY inventory_movements_insert ON inventory_movements IS 'Ensures users can only create movements for their site';

-- ============================================================================
-- ENABLE RLS ON BATCH_MOVEMENTS
-- ============================================================================
ALTER TABLE batch_movements ENABLE ROW LEVEL SECURITY;

CREATE POLICY batch_movements_site_isolation ON batch_movements
    FOR ALL
    USING (site_id = COALESCE(
        current_setting('app.current_site_id', true)::uuid,
        '00000000-0000-0000-0000-000000000000'::uuid
    ));

CREATE POLICY batch_movements_insert ON batch_movements
    FOR INSERT
    WITH CHECK (site_id = COALESCE(
        current_setting('app.current_site_id', true)::uuid,
        '00000000-0000-0000-0000-000000000000'::uuid
    ));

-- ============================================================================
-- ENSURE RLS IS ENABLED ON PACKAGES (if not already)
-- ============================================================================
DO $$
BEGIN
    -- Check if RLS is already enabled
    IF NOT EXISTS (
        SELECT 1 FROM pg_tables 
        WHERE tablename = 'packages' 
        AND rowsecurity = true
    ) THEN
        ALTER TABLE packages ENABLE ROW LEVEL SECURITY;
        
        -- Create policy if it doesn't exist
        IF NOT EXISTS (
            SELECT 1 FROM pg_policies 
            WHERE tablename = 'packages' 
            AND policyname = 'packages_site_isolation'
        ) THEN
            EXECUTE 'CREATE POLICY packages_site_isolation ON packages
                FOR ALL
                USING (site_id = COALESCE(
                    current_setting(''app.current_site_id'', true)::uuid,
                    ''00000000-0000-0000-0000-000000000000''::uuid
                ))';
        END IF;
    END IF;
END $$;

-- ============================================================================
-- ENSURE RLS IS ENABLED ON ITEMS (if not already)
-- ============================================================================
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_tables 
        WHERE tablename = 'items' 
        AND rowsecurity = true
    ) THEN
        ALTER TABLE items ENABLE ROW LEVEL SECURITY;
        
        IF NOT EXISTS (
            SELECT 1 FROM pg_policies 
            WHERE tablename = 'items' 
            AND policyname = 'items_site_isolation'
        ) THEN
            EXECUTE 'CREATE POLICY items_site_isolation ON items
                FOR ALL
                USING (site_id = COALESCE(
                    current_setting(''app.current_site_id'', true)::uuid,
                    ''00000000-0000-0000-0000-000000000000''::uuid
                ))';
        END IF;
    END IF;
END $$;

-- ============================================================================
-- GRANT PERMISSIONS
-- ============================================================================
-- These would be granted to specific roles in production

-- Grant select on views to authenticated users
GRANT SELECT ON inventory_value_by_category TO authenticated;
GRANT SELECT ON inventory_value_summary TO authenticated;
GRANT SELECT ON low_stock_alerts TO authenticated;
GRANT SELECT ON expiring_inventory TO authenticated;
GRANT SELECT ON value_at_risk TO authenticated;
GRANT SELECT ON cogs_by_period TO authenticated;
GRANT SELECT ON inventory_aging TO authenticated;
GRANT SELECT ON inventory_aging_summary TO authenticated;
GRANT SELECT ON inventory_turnover TO authenticated;
GRANT SELECT ON movement_summary_by_type TO authenticated;
GRANT SELECT ON holds_summary TO authenticated;

-- Grant table access
GRANT SELECT, INSERT, UPDATE ON inventory_movements TO authenticated;
GRANT SELECT, INSERT, UPDATE ON batch_movements TO authenticated;

COMMENT ON TABLE inventory_movements IS 'Movement audit trail with site-level RLS';
COMMENT ON TABLE batch_movements IS 'Batch movement groups with site-level RLS';



