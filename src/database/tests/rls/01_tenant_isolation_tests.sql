-- RLS Test Suite: Tenant Isolation Tests
-- These tests verify that users can only access data from their assigned sites

-- Test Helper Functions
CREATE OR REPLACE FUNCTION test.set_rls_context(p_user_id UUID, p_role TEXT, p_site_id UUID DEFAULT NULL)
RETURNS VOID AS $$
BEGIN
    PERFORM set_config('app.current_user_id', p_user_id::TEXT, true);
    PERFORM set_config('app.user_role', p_role, true);
    IF p_site_id IS NOT NULL THEN
        PERFORM set_config('app.current_site_id', p_site_id::TEXT, true);
    END IF;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION test.clear_rls_context()
RETURNS VOID AS $$
BEGIN
    PERFORM set_config('app.current_user_id', '', true);
    PERFORM set_config('app.user_role', '', true);
    PERFORM set_config('app.current_site_id', '', true);
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- TEST 1: User can only see rooms from their assigned site
-- ============================================================================
DO $$
DECLARE
    v_count INT;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_beta_site_1_id UUID := 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb';
    v_alpha_operator_1_id UUID := 'a2222222-2222-2222-2222-222222222222';
BEGIN
    -- Set context as Alpha Operator 1 with Alpha Site 1
    PERFORM test.set_rls_context(v_alpha_operator_1_id, 'operator', v_alpha_site_1_id);
    
    -- Count rooms visible to this user
    SELECT COUNT(*) INTO v_count FROM public.rooms;
    
    -- Should only see Alpha Site 1 rooms (2 rooms)
    IF v_count = 2 THEN
        RAISE NOTICE '✓ TEST 1 PASSED: Alpha Operator 1 sees only Alpha Site 1 rooms (%)', v_count;
    ELSE
        RAISE WARNING '✗ TEST 1 FAILED: Expected 2 rooms, got %', v_count;
    END IF;
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 2: User cannot access rooms from other organization
-- ============================================================================
DO $$
DECLARE
    v_count INT;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_beta_room_id UUID := 'rb111111-1111-1111-1111-111111111111';
    v_alpha_operator_1_id UUID := 'a2222222-2222-2222-2222-222222222222';
BEGIN
    -- Set context as Alpha Operator 1
    PERFORM test.set_rls_context(v_alpha_operator_1_id, 'operator', v_alpha_site_1_id);
    
    -- Try to access a specific Beta room
    SELECT COUNT(*) INTO v_count 
    FROM public.rooms 
    WHERE id = v_beta_room_id;
    
    -- Should not see any Beta rooms
    IF v_count = 0 THEN
        RAISE NOTICE '✓ TEST 2 PASSED: Alpha user cannot see Beta room';
    ELSE
        RAISE WARNING '✗ TEST 2 FAILED: Alpha user can see Beta room (cross-tenant leak!)';
    END IF;
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 3: Admin can see rooms from all their assigned sites
-- ============================================================================
DO $$
DECLARE
    v_count INT;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_alpha_admin_id UUID := 'a1111111-1111-1111-1111-111111111111';
BEGIN
    -- Set context as Alpha Admin (assigned to both Alpha sites)
    -- Note: Testing site 1 context first
    PERFORM test.set_rls_context(v_alpha_admin_id, 'admin', v_alpha_site_1_id);
    
    -- Count rooms for Alpha Site 1
    SELECT COUNT(*) INTO v_count FROM public.rooms;
    
    -- Should see Alpha Site 1 rooms (2 rooms with site context)
    IF v_count = 2 THEN
        RAISE NOTICE '✓ TEST 3 PASSED: Alpha Admin sees Alpha Site 1 rooms (%)', v_count;
    ELSE
        RAISE WARNING '✗ TEST 3 FAILED: Expected 2 rooms for site context, got %', v_count;
    END IF;
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 4: Genetics strains are site-isolated
-- ============================================================================
DO $$
DECLARE
    v_count INT;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_alpha_operator_1_id UUID := 'a2222222-2222-2222-2222-222222222222';
BEGIN
    -- Set context as Alpha Operator 1
    PERFORM test.set_rls_context(v_alpha_operator_1_id, 'operator', v_alpha_site_1_id);
    
    -- Count strains visible
    SELECT COUNT(*) INTO v_count FROM genetics.strains;
    
    -- Should only see Alpha Site 1 strains (2 strains)
    IF v_count = 2 THEN
        RAISE NOTICE '✓ TEST 4 PASSED: Strains are site-isolated (%)', v_count;
    ELSE
        RAISE WARNING '✗ TEST 4 FAILED: Expected 2 strains, got %', v_count;
    END IF;
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 5: Beta user cannot see Alpha strains
-- ============================================================================
DO $$
DECLARE
    v_count INT;
    v_beta_site_1_id UUID := 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb';
    v_alpha_strain_id UUID := 'sa111111-1111-1111-1111-111111111111';
    v_beta_operator_id UUID := 'b2222222-2222-2222-2222-222222222222';
BEGIN
    -- Set context as Beta Operator
    PERFORM test.set_rls_context(v_beta_operator_id, 'operator', v_beta_site_1_id);
    
    -- Try to access Alpha strain
    SELECT COUNT(*) INTO v_count 
    FROM genetics.strains 
    WHERE id = v_alpha_strain_id;
    
    -- Should not see any Alpha strains
    IF v_count = 0 THEN
        RAISE NOTICE '✓ TEST 5 PASSED: Beta user cannot see Alpha strains';
    ELSE
        RAISE WARNING '✗ TEST 5 FAILED: Cross-tenant strain access detected!';
    END IF;
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 6: Tasks are site-isolated
-- ============================================================================
DO $$
DECLARE
    v_count INT;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_alpha_operator_1_id UUID := 'a2222222-2222-2222-2222-222222222222';
BEGIN
    -- Set context as Alpha Operator 1
    PERFORM test.set_rls_context(v_alpha_operator_1_id, 'operator', v_alpha_site_1_id);
    
    -- Count tasks visible
    SELECT COUNT(*) INTO v_count FROM tasks.tasks;
    
    -- Should only see Alpha Site 1 tasks (2 tasks)
    IF v_count = 2 THEN
        RAISE NOTICE '✓ TEST 6 PASSED: Tasks are site-isolated (%)', v_count;
    ELSE
        RAISE WARNING '✗ TEST 6 FAILED: Expected 2 tasks, got %', v_count;
    END IF;
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 7: User cannot modify data from other site via direct UPDATE
-- ============================================================================
DO $$
DECLARE
    v_affected INT;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_beta_room_id UUID := 'rb111111-1111-1111-1111-111111111111';
    v_alpha_operator_1_id UUID := 'a2222222-2222-2222-2222-222222222222';
BEGIN
    -- Set context as Alpha Operator 1
    PERFORM test.set_rls_context(v_alpha_operator_1_id, 'operator', v_alpha_site_1_id);
    
    -- Attempt to update a Beta room
    UPDATE public.rooms 
    SET name = 'Hacked Room' 
    WHERE id = v_beta_room_id;
    
    GET DIAGNOSTICS v_affected = ROW_COUNT;
    
    -- Should affect 0 rows
    IF v_affected = 0 THEN
        RAISE NOTICE '✓ TEST 7 PASSED: Cross-tenant UPDATE blocked (0 rows affected)';
    ELSE
        RAISE WARNING '✗ TEST 7 FAILED: Cross-tenant UPDATE succeeded (% rows affected)!', v_affected;
        -- Rollback the change
        ROLLBACK;
    END IF;
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 8: User cannot delete data from other site
-- ============================================================================
DO $$
DECLARE
    v_affected INT;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_beta_task_id UUID := 'tb111111-1111-1111-1111-111111111111';
    v_alpha_admin_id UUID := 'a1111111-1111-1111-1111-111111111111';
BEGIN
    -- Set context as Alpha Admin
    PERFORM test.set_rls_context(v_alpha_admin_id, 'admin', v_alpha_site_1_id);
    
    -- Attempt to delete a Beta task
    DELETE FROM tasks.tasks WHERE id = v_beta_task_id;
    
    GET DIAGNOSTICS v_affected = ROW_COUNT;
    
    -- Should affect 0 rows
    IF v_affected = 0 THEN
        RAISE NOTICE '✓ TEST 8 PASSED: Cross-tenant DELETE blocked (0 rows affected)';
    ELSE
        RAISE WARNING '✗ TEST 8 FAILED: Cross-tenant DELETE succeeded (% rows affected)!', v_affected;
    END IF;
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- Summary
-- ============================================================================
DO $$
BEGIN
    RAISE NOTICE '';
    RAISE NOTICE '===========================================';
    RAISE NOTICE 'Tenant Isolation Test Suite Complete';
    RAISE NOTICE '===========================================';
    RAISE NOTICE 'Review output above for PASSED/FAILED status';
END $$;








