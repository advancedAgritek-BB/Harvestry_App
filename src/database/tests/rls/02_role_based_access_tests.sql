-- RLS Test Suite: Role-Based Access Tests
-- These tests verify that different roles have appropriate permissions

-- ============================================================================
-- TEST 1: Operator can read from their site
-- ============================================================================
DO $$
DECLARE
    v_count INT;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_alpha_operator_1_id UUID := 'a2222222-2222-2222-2222-222222222222';
BEGIN
    PERFORM test.set_rls_context(v_alpha_operator_1_id, 'operator', v_alpha_site_1_id);
    
    -- Operators should be able to read rooms
    SELECT COUNT(*) INTO v_count FROM public.rooms;
    
    IF v_count > 0 THEN
        RAISE NOTICE '✓ TEST 1 PASSED: Operator can read rooms (% found)', v_count;
    ELSE
        RAISE WARNING '✗ TEST 1 FAILED: Operator cannot read rooms';
    END IF;
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 2: Operator can create tasks in their site
-- ============================================================================
DO $$
DECLARE
    v_new_id UUID;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_alpha_operator_1_id UUID := 'a2222222-2222-2222-2222-222222222222';
BEGIN
    PERFORM test.set_rls_context(v_alpha_operator_1_id, 'operator', v_alpha_site_1_id);
    
    -- Attempt to insert a new task
    INSERT INTO tasks.tasks (id, site_id, title, status, priority, created_by_user_id, created_at)
    VALUES (gen_random_uuid(), v_alpha_site_1_id, 'Test Task', 'pending', 'medium', v_alpha_operator_1_id, NOW())
    RETURNING id INTO v_new_id;
    
    IF v_new_id IS NOT NULL THEN
        RAISE NOTICE '✓ TEST 2 PASSED: Operator can create tasks in their site';
        -- Clean up
        DELETE FROM tasks.tasks WHERE id = v_new_id;
    ELSE
        RAISE WARNING '✗ TEST 2 FAILED: Operator cannot create tasks';
    END IF;
    
    PERFORM test.clear_rls_context();
EXCEPTION
    WHEN insufficient_privilege THEN
        RAISE WARNING '✗ TEST 2 FAILED: Insufficient privilege to create task';
        PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 3: Operator cannot insert into different site
-- ============================================================================
DO $$
DECLARE
    v_new_id UUID;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_beta_site_1_id UUID := 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb';
    v_alpha_operator_1_id UUID := 'a2222222-2222-2222-2222-222222222222';
BEGIN
    PERFORM test.set_rls_context(v_alpha_operator_1_id, 'operator', v_alpha_site_1_id);
    
    -- Attempt to insert a task into Beta site (should fail due to RLS)
    INSERT INTO tasks.tasks (id, site_id, title, status, priority, created_by_user_id, created_at)
    VALUES (gen_random_uuid(), v_beta_site_1_id, 'Malicious Task', 'pending', 'medium', v_alpha_operator_1_id, NOW())
    RETURNING id INTO v_new_id;
    
    IF v_new_id IS NOT NULL THEN
        RAISE WARNING '✗ TEST 3 FAILED: Cross-site INSERT succeeded (security breach!)';
        -- Clean up
        DELETE FROM tasks.tasks WHERE id = v_new_id;
    ELSE
        RAISE NOTICE '✓ TEST 3 PASSED: Cross-site INSERT blocked';
    END IF;
    
    PERFORM test.clear_rls_context();
EXCEPTION
    WHEN insufficient_privilege OR new_row_violates_row_level_security THEN
        RAISE NOTICE '✓ TEST 3 PASSED: Cross-site INSERT blocked by RLS';
        PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 4: Admin has broader access within organization
-- ============================================================================
DO $$
DECLARE
    v_count INT;
    v_alpha_site_2_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab';
    v_alpha_admin_id UUID := 'a1111111-1111-1111-1111-111111111111';
BEGIN
    -- Admin switches to their second site
    PERFORM test.set_rls_context(v_alpha_admin_id, 'admin', v_alpha_site_2_id);
    
    -- Should be able to see Site 2 rooms
    SELECT COUNT(*) INTO v_count FROM public.rooms WHERE site_id = v_alpha_site_2_id;
    
    IF v_count > 0 THEN
        RAISE NOTICE '✓ TEST 4 PASSED: Admin can access their second site (% rooms)', v_count;
    ELSE
        RAISE WARNING '✗ TEST 4 FAILED: Admin cannot access their second site';
    END IF;
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 5: Service account has service-level access
-- ============================================================================
DO $$
DECLARE
    v_count INT;
    v_service_account_id UUID := '00000000-0000-0000-0000-000000000000';
BEGIN
    -- Service accounts should have broader access for background jobs
    PERFORM test.set_rls_context(v_service_account_id, 'service_account', NULL);
    
    -- Note: The actual behavior depends on your RLS policy implementation
    -- Some implementations give service_account full read access
    SELECT COUNT(*) INTO v_count FROM public.rooms;
    
    RAISE NOTICE 'TEST 5 INFO: Service account can see % rooms', v_count;
    RAISE NOTICE '(Service account access policy is implementation-specific)';
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 6: Role cannot be escalated via header manipulation
-- ============================================================================
DO $$
DECLARE
    v_count INT;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_alpha_operator_1_id UUID := 'a2222222-2222-2222-2222-222222222222';
BEGIN
    -- Attempt to set admin role for an operator user
    -- The backend should validate this against user_sites table
    PERFORM test.set_rls_context(v_alpha_operator_1_id, 'admin', v_alpha_site_1_id);
    
    -- If RLS policies properly check user's actual role from DB, this should 
    -- still behave as operator. This test documents the expected behavior.
    SELECT COUNT(*) INTO v_count FROM public.rooms;
    
    RAISE NOTICE 'TEST 6 INFO: Role escalation test - user sees % rooms', v_count;
    RAISE NOTICE '(Backend should validate roles against database, not trust headers in production)';
    
    PERFORM test.clear_rls_context();
END $$;

-- ============================================================================
-- TEST 7: Operator can update their own tasks
-- ============================================================================
DO $$
DECLARE
    v_affected INT;
    v_alpha_site_1_id UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_alpha_operator_1_id UUID := 'a2222222-2222-2222-2222-222222222222';
    v_task_id UUID := 'ta111111-1111-1111-1111-111111111111';
BEGIN
    PERFORM test.set_rls_context(v_alpha_operator_1_id, 'operator', v_alpha_site_1_id);
    
    -- Update a task in their site
    UPDATE tasks.tasks 
    SET title = 'Updated Task Title'
    WHERE id = v_task_id;
    
    GET DIAGNOSTICS v_affected = ROW_COUNT;
    
    IF v_affected = 1 THEN
        RAISE NOTICE '✓ TEST 7 PASSED: Operator can update tasks in their site';
        -- Restore original
        UPDATE tasks.tasks SET title = 'Alpha1 Task 1' WHERE id = v_task_id;
    ELSE
        RAISE WARNING '✗ TEST 7 FAILED: Operator cannot update tasks (% rows)', v_affected;
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
    RAISE NOTICE 'Role-Based Access Test Suite Complete';
    RAISE NOTICE '===========================================';
    RAISE NOTICE 'Review output above for PASSED/FAILED status';
END $$;



