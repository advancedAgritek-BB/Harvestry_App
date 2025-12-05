-- ============================================================================
-- FRP-04: Tasks, Messaging & Slack
-- Seed Script: SOP, Training, and Task Gating Fixtures for Smoke Tests
--
-- Provides representative training/SOP data so Workflow & Messaging teams
-- can exercise task gating scenarios during Slice 1 smoke tests.
--
-- Idempotent: safe to run multiple times.
-- ============================================================================

DO $$
DECLARE
    v_org_id CONSTANT UUID := '00000000-0000-0000-0000-000000000001';
    v_site_code CONSTANT TEXT := 'DGC-DEN-01';
    v_site_id UUID;
    v_operator_role_id UUID;
    v_supervisor_role_id UUID;
    v_manager_role_id UUID;

    v_training_manager_user_id CONSTANT UUID := '00000000-0000-0000-0000-000000000101';
    v_compliance_user_id     CONSTANT UUID := '00000000-0000-0000-0000-000000000102';
    v_operator_user_id       CONSTANT UUID := '00000000-0000-0000-0000-000000000103';

    v_training_manager_site_link UUID := '00000000-0000-0000-0000-000000000201';
    v_compliance_site_link      UUID := '00000000-0000-0000-0000-000000000202';
    v_operator_site_link        UUID := '00000000-0000-0000-0000-000000000203';

    v_sop_calibration_id UUID;
    v_sop_emergency_id UUID;
    v_module_safety_id UUID;
    v_module_smoke_prep_id UUID;
    v_quiz_smoke_prep_id UUID;

    v_assignment_safety_id CONSTANT UUID := '00000000-0000-0000-0000-000000000301';
    v_assignment_smoke_id  CONSTANT UUID := '00000000-0000-0000-0000-000000000302';

    v_signoff_calibration_id CONSTANT UUID := '00000000-0000-0000-0000-000000000401';

    v_gating_calibration_id CONSTANT UUID := '00000000-0000-0000-0000-000000000501';
    v_gating_smoke_id       CONSTANT UUID := '00000000-0000-0000-0000-000000000502';
BEGIN
    -- Ensure core organization exists
    INSERT INTO organizations (organization_id, name, slug, status)
    VALUES (v_org_id, 'Harvestry System', 'harvestry-system', 'active')
    ON CONFLICT (organization_id) DO UPDATE
        SET name = EXCLUDED.name,
            status = EXCLUDED.status;

    -- Create or lookup canonical cultivation site
    INSERT INTO sites (site_id, org_id, site_name, site_code, city, state_province, country, timezone, license_number, license_type, status)
    VALUES (
        '00000000-0000-0000-0000-000000000010',
        v_org_id,
        'Denver Cultivation Center',
        v_site_code,
        'Denver',
        'CO',
        'US',
        'America/Denver',
        'CO-CLT-2025-0915',
        'cultivation',
        'active'
    )
    ON CONFLICT (site_code) DO UPDATE
        SET site_name = EXCLUDED.site_name,
            status = 'active';

    SELECT site_id INTO v_site_id FROM sites WHERE site_code = v_site_code;

    -- Resolve role identifiers
    SELECT role_id INTO v_operator_role_id FROM roles WHERE role_name = 'operator';
    SELECT role_id INTO v_supervisor_role_id FROM roles WHERE role_name = 'supervisor';
    SELECT role_id INTO v_manager_role_id FROM roles WHERE role_name = 'manager';

    IF v_operator_role_id IS NULL OR v_supervisor_role_id IS NULL OR v_manager_role_id IS NULL THEN
        RAISE EXCEPTION 'Core roles missing; run FRP-01 migrations first.';
    END IF;

    -- Upsert training manager (supervisor)
    INSERT INTO users (user_id, email, email_verified, first_name, last_name, display_name, status, timezone)
    VALUES (
        v_training_manager_user_id,
        'training.manager@harvestry.dev',
        TRUE,
        'Casey',
        'Quinn',
        'Casey Quinn',
        'active',
        'America/Denver'
    )
    ON CONFLICT (email) DO UPDATE
        SET first_name = EXCLUDED.first_name,
            last_name = EXCLUDED.last_name,
            status = 'active',
            updated_at = NOW()
    RETURNING users.user_id INTO v_training_manager_user_id;

    -- Upsert compliance officer (manager)
    INSERT INTO users (user_id, email, email_verified, first_name, last_name, display_name, status, timezone)
    VALUES (
        v_compliance_user_id,
        'compliance.officer@harvestry.dev',
        TRUE,
        'Morgan',
        'Lee',
        'Morgan Lee',
        'active',
        'America/Denver'
    )
    ON CONFLICT (email) DO UPDATE
        SET first_name = EXCLUDED.first_name,
            last_name = EXCLUDED.last_name,
            status = 'active',
            updated_at = NOW();

    -- Upsert operator user
    INSERT INTO users (user_id, email, email_verified, first_name, last_name, display_name, status, timezone)
    VALUES (
        v_operator_user_id,
        'operator.one@harvestry.dev',
        TRUE,
        'Riley',
        'Nguyen',
        'Riley Nguyen',
        'active',
        'America/Denver'
    )
    ON CONFLICT (email) DO UPDATE
        SET first_name = EXCLUDED.first_name,
            last_name = EXCLUDED.last_name,
            status = 'active',
            updated_at = NOW();

    -- Link users to the site with appropriate roles
    INSERT INTO user_sites (user_site_id, user_id, site_id, role_id, is_primary_site)
    VALUES (v_training_manager_site_link, v_training_manager_user_id, v_site_id, v_supervisor_role_id, TRUE)
    ON CONFLICT (user_id, site_id) DO UPDATE
        SET role_id = EXCLUDED.role_id,
            updated_at = NOW();

    INSERT INTO user_sites (user_site_id, user_id, site_id, role_id, is_primary_site)
    VALUES (v_compliance_site_link, v_compliance_user_id, v_site_id, v_manager_role_id, TRUE)
    ON CONFLICT (user_id, site_id) DO UPDATE
        SET role_id = EXCLUDED.role_id,
            updated_at = NOW();

    INSERT INTO user_sites (user_site_id, user_id, site_id, role_id, is_primary_site)
    VALUES (v_operator_site_link, v_operator_user_id, v_site_id, v_operator_role_id, TRUE)
    ON CONFLICT (user_id, site_id) DO UPDATE
        SET role_id = EXCLUDED.role_id,
            updated_at = NOW();

    -- Seed SOPs
    INSERT INTO sops (
        sop_id,
        org_id,
        sop_code,
        title,
        description,
        category,
        version,
        status,
        content_markdown,
        requires_quiz,
        requires_signoff,
        applies_to_roles,
        effective_date,
        review_frequency_days,
        created_by,
        approved_by,
        approved_at
    ) VALUES (
        '00000000-0000-0000-0000-000000000601',
        v_org_id,
        'SOP-201-EQUIPMENT-CAL',
        'Equipment Calibration & Verification',
        'Step-by-step calibration checklist for irrigation controllers and sensors.',
        'Operations',
        '1.1',
        'active',
        '## Calibration Flow\n1. Lock out zone.\n2. Run dry calibration.\n3. Record EC baseline.',
        TRUE,
        TRUE,
        jsonb_build_array('operator', 'supervisor'),
        CURRENT_DATE - 14,
        180,
        v_training_manager_user_id,
        v_compliance_user_id,
        NOW()
    )
    ON CONFLICT (sop_code) DO UPDATE
        SET title = EXCLUDED.title,
            description = EXCLUDED.description,
            version = EXCLUDED.version,
            status = 'active',
            updated_at = NOW()
    RETURNING sops.sop_id INTO v_sop_calibration_id;

    INSERT INTO sops (
        sop_id,
        org_id,
        sop_code,
        title,
        description,
        category,
        version,
        status,
        content_markdown,
        requires_quiz,
        requires_signoff,
        applies_to_roles,
        effective_date,
        review_frequency_days,
        created_by,
        approved_by,
        approved_at
    ) VALUES (
        '00000000-0000-0000-0000-000000000602',
        v_org_id,
        'SOP-305-EMERGENCY-RESPONSE',
        'Day 2 Smoke Test Emergency Response',
        'Escalation and recovery steps for Slack smoke tests and automation rollbacks.',
        'Safety',
        '1.0',
        'active',
        '## Emergency Channel Activation\n- Notify #ops-bridge.\n- Rotate Slack bot tokens.\n- Fall back to manual paging.',
        TRUE,
        TRUE,
        jsonb_build_array('operator', 'supervisor', 'manager'),
        CURRENT_DATE,
        365,
        v_compliance_user_id,
        v_compliance_user_id,
        NOW()
    )
    ON CONFLICT (sop_code) DO UPDATE
        SET title = EXCLUDED.title,
            description = EXCLUDED.description,
            status = 'active',
            updated_at = NOW()
    RETURNING sops.sop_id INTO v_sop_emergency_id;

    -- Seed training modules
    INSERT INTO training_modules (
        module_id,
        org_id,
        module_code,
        title,
        description,
        category,
        module_type,
        content_url,
        duration_minutes,
        passing_score_pct,
        status,
        applies_to_roles,
        version,
        effective_date,
        created_by
    ) VALUES (
        '00000000-0000-0000-0000-000000000701',
        v_org_id,
        'TRN-101-SAFETY-FOUNDATION',
        'Cultivation Safety Foundations',
        'Baseline safety expectations for cultivation operators working day and night shifts.',
        'Safety',
        'video',
        'https://training.harvestry.dev/modules/safety-foundation',
        35,
        80,
        'active',
        jsonb_build_array('operator', 'supervisor'),
        '1.0',
        CURRENT_DATE - 30,
        v_training_manager_user_id
    )
    ON CONFLICT (module_code) DO UPDATE
        SET title = EXCLUDED.title,
            description = EXCLUDED.description,
            status = 'active',
            updated_at = NOW()
    RETURNING training_modules.module_id INTO v_module_safety_id;

    INSERT INTO training_modules (
        module_id,
        org_id,
        module_code,
        title,
        description,
        category,
        module_type,
        content_url,
        duration_minutes,
        passing_score_pct,
        status,
        applies_to_roles,
        prerequisites,
        version,
        effective_date,
        expiration_period_days,
        created_by
    ) VALUES (
        '00000000-0000-0000-0000-000000000702',
        v_org_id,
        'TRN-210-SMOKE-READINESS',
        'Day 2 Smoke-Test Readiness',
        'Pre-flight checklist and communications workflow for FRP-04/05 Slack smoke tests.',
        'Operations',
        'interactive',
        'https://training.harvestry.dev/modules/smoke-readiness',
        25,
        90,
        'active',
        jsonb_build_array('operator', 'supervisor', 'manager'),
        jsonb_build_array(v_module_safety_id),
        '1.0',
        CURRENT_DATE,
        180,
        v_training_manager_user_id
    )
    ON CONFLICT (module_code) DO UPDATE
        SET title = EXCLUDED.title,
            description = EXCLUDED.description,
            status = 'active',
            updated_at = NOW(),
            prerequisites = EXCLUDED.prerequisites
    RETURNING training_modules.module_id INTO v_module_smoke_prep_id;

    -- Quiz for smoke readiness module
    INSERT INTO quizzes (
        quiz_id,
        module_id,
        title,
        description,
        questions,
        passing_score_pct,
        time_limit_minutes,
        allow_retakes,
        status
    ) VALUES (
        '00000000-0000-0000-0000-000000000801',
        v_module_smoke_prep_id,
        'Slack Smoke-Test Scenario Quiz',
        'Confirms operators can recover the Slack worker during token rotation.',
        jsonb_build_array(
            jsonb_build_object(
                'question', 'Which secret stores the Slack bot refresh token?',
                'type', 'multiple_choice',
                'options', jsonb_build_array('slack_tasks_dev', 'harvestry_slack_bot', 'slack_refresh_prod'),
                'correct_answer', 'slack_tasks_dev'
            ),
            jsonb_build_object(
                'question', 'How soon before expiry must the Slack worker refresh its token?',
                'type', 'multiple_choice',
                'options', jsonb_build_array('1 minute', '10-15% of lifetime', 'Only after failure'),
                'correct_answer', '10-15% of lifetime'
            )
        ),
        90,
        15,
        TRUE,
        'active'
    )
    ON CONFLICT (quiz_id) DO UPDATE
        SET questions = EXCLUDED.questions,
            passing_score_pct = EXCLUDED.passing_score_pct,
            updated_at = NOW();

    -- Training assignments for operator
    INSERT INTO training_assignments (
        assignment_id,
        user_id,
        module_id,
        assigned_by,
        assigned_at,
        due_date,
        status,
        started_at,
        completed_at,
        score_pct,
        passed,
        attempts,
        time_spent_minutes,
        metadata
    ) VALUES (
        v_assignment_safety_id,
        v_operator_user_id,
        v_module_safety_id,
        v_training_manager_user_id,
        NOW() - INTERVAL '7 days',
        CURRENT_DATE - 1,
        'completed',
        NOW() - INTERVAL '7 days',
        NOW() - INTERVAL '6 days',
        92,
        TRUE,
        1,
        40,
        jsonb_build_object('seed_source', 'frp04')
    )
    ON CONFLICT (assignment_id) DO UPDATE
        SET status = EXCLUDED.status,
            completed_at = EXCLUDED.completed_at,
            score_pct = EXCLUDED.score_pct,
            passed = EXCLUDED.passed,
            metadata = EXCLUDED.metadata,
            updated_at = NOW();

    INSERT INTO training_assignments (
        assignment_id,
        user_id,
        module_id,
        assigned_by,
        assigned_at,
        due_date,
        status,
        started_at,
        metadata
    ) VALUES (
        v_assignment_smoke_id,
        v_operator_user_id,
        v_module_smoke_prep_id,
        v_training_manager_user_id,
        NOW() - INTERVAL '1 days',
        CURRENT_DATE + 1,
        'assigned',
        NULL,
        jsonb_build_object('seed_source', 'frp04', 'notes', 'Complete before Day 2 smoke tests')
    )
    ON CONFLICT (assignment_id) DO UPDATE
        SET status = EXCLUDED.status,
            due_date = EXCLUDED.due_date,
            metadata = EXCLUDED.metadata,
            updated_at = NOW();

    -- SOP signoff for calibration SOP (completed)
    INSERT INTO sop_signoffs (
        signoff_id,
        user_id,
        sop_id,
        sop_version,
        signed_at,
        user_agent,
        attestation_text,
        witness_user_id,
        witness_signed_at
    ) VALUES (
        v_signoff_calibration_id,
        v_operator_user_id,
        v_sop_calibration_id,
        '1.1',
        NOW() - INTERVAL '5 days',
        'SeedScript/1.0',
        'I have reviewed and understand the calibration SOP.',
        v_compliance_user_id,
        NOW() - INTERVAL '5 days'
    )
    ON CONFLICT (user_id, sop_id, sop_version) DO UPDATE
        SET signed_at = EXCLUDED.signed_at,
            witness_user_id = EXCLUDED.witness_user_id,
            witness_signed_at = EXCLUDED.witness_signed_at;

    -- Task gating requirements: one satisfied (calibration), one outstanding (smoke test readiness)
    INSERT INTO task_gating_requirements (
        gating_id,
        task_type,
        required_sop_id,
        required_module_id,
        required_permission_action,
        site_id,
        is_active,
        created_by
    )
    VALUES (
        v_gating_calibration_id,
        'equipment_calibration',
        v_sop_calibration_id,
        NULL,
        NULL,
        v_site_id,
        TRUE,
        v_training_manager_user_id
    )
    ON CONFLICT (gating_id) DO UPDATE
        SET required_sop_id = EXCLUDED.required_sop_id,
            site_id = EXCLUDED.site_id,
            is_active = TRUE;

    IF NOT EXISTS (
        SELECT 1 FROM task_gating_requirements
        WHERE gating_id = v_gating_smoke_id
    ) THEN
        INSERT INTO task_gating_requirements (
            gating_id,
            task_type,
            required_sop_id,
            required_module_id,
            required_permission_action,
            site_id,
            is_active,
            created_by
        ) VALUES (
            v_gating_smoke_id,
            'smoke_test_day_2',
            v_sop_emergency_id,
            v_module_smoke_prep_id,
            NULL,
            v_site_id,
            TRUE,
            v_training_manager_user_id
        );
    ELSE
        UPDATE task_gating_requirements
        SET required_sop_id = v_sop_emergency_id,
            required_module_id = v_module_smoke_prep_id,
            site_id = v_site_id,
            is_active = TRUE
        WHERE gating_id = v_gating_smoke_id;
    END IF;
END;
$$;
