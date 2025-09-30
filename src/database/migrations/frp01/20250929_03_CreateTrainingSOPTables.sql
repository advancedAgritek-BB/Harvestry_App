-- ============================================================================
-- FRP-01: Training, SOPs & Task Gating
-- Migration: Create SOP, Training, and Assignment Tables
--
-- Description: SOP management and training tracking for task gating logic
-- Dependencies: 20250929_01_CreateIdentityTables.sql
-- ============================================================================

-- ============================================================================
-- SOPS (Standard Operating Procedures) TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS sops (
    sop_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    org_id UUID NOT NULL,
    sop_code VARCHAR(50) UNIQUE NOT NULL, -- e.g., "SOP-001-HARVEST"
    title VARCHAR(300) NOT NULL,
    description TEXT,
    category VARCHAR(100), -- e.g., "Safety", "Harvest", "Processing"
    version VARCHAR(20) NOT NULL DEFAULT '1.0',
    status VARCHAR(20) DEFAULT 'draft'
        CHECK (status IN ('draft', 'active', 'archived', 'deprecated')),
    content_markdown TEXT, -- Full SOP content
    content_url TEXT, -- External PDF or video link
    estimated_read_time_minutes INT DEFAULT 10,
    requires_quiz BOOLEAN DEFAULT FALSE,
    requires_signoff BOOLEAN DEFAULT TRUE,
    applies_to_roles JSONB DEFAULT '[]'::JSONB, -- Array of role names
    effective_date DATE,
    review_frequency_days INT DEFAULT 365, -- Annual review by default
    next_review_date DATE,
    supersedes_sop_id UUID REFERENCES sops(sop_id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID REFERENCES users(user_id),
    approved_by UUID REFERENCES users(user_id),
    approved_at TIMESTAMPTZ
);

CREATE INDEX ix_sops_org ON sops (org_id);
CREATE INDEX ix_sops_code ON sops (sop_code);
CREATE INDEX ix_sops_status ON sops (status) WHERE status = 'active';
CREATE INDEX ix_sops_review ON sops (next_review_date) WHERE status = 'active';

COMMENT ON TABLE sops IS 'Standard Operating Procedures for task execution and compliance';
COMMENT ON COLUMN sops.applies_to_roles IS 'JSON array of role names this SOP applies to, e.g., ["operator", "supervisor"]';

-- ============================================================================
-- TRAINING_MODULES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS training_modules (
    module_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    org_id UUID NOT NULL,
    module_code VARCHAR(50) UNIQUE NOT NULL, -- e.g., "TRN-101-SAFETY"
    title VARCHAR(300) NOT NULL,
    description TEXT,
    category VARCHAR(100), -- e.g., "Safety", "Equipment Operation"
    module_type VARCHAR(50) DEFAULT 'document'
        CHECK (module_type IN ('document', 'video', 'interactive', 'hands_on', 'quiz')),
    content_url TEXT,
    duration_minutes INT DEFAULT 30,
    passing_score_pct INT DEFAULT 80, -- For quizzes
    status VARCHAR(20) DEFAULT 'active'
        CHECK (status IN ('active', 'inactive', 'archived')),
    applies_to_roles JSONB DEFAULT '[]'::JSONB,
    prerequisites JSONB DEFAULT '[]'::JSONB, -- Array of prerequisite module_ids
    version VARCHAR(20) DEFAULT '1.0',
    effective_date DATE,
    expiration_period_days INT, -- If set, training expires and must be renewed
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID REFERENCES users(user_id)
);

CREATE INDEX ix_training_modules_org ON training_modules (org_id);
CREATE INDEX ix_training_modules_code ON training_modules (module_code);
CREATE INDEX ix_training_modules_status ON training_modules (status) WHERE status = 'active';

COMMENT ON TABLE training_modules IS 'Training courses and certification modules';
COMMENT ON COLUMN training_modules.expiration_period_days IS 'If set, user must renew training after this many days';

-- ============================================================================
-- QUIZZES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS quizzes (
    quiz_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    module_id UUID REFERENCES training_modules(module_id) ON DELETE CASCADE,
    sop_id UUID REFERENCES sops(sop_id) ON DELETE CASCADE,
    title VARCHAR(300) NOT NULL,
    description TEXT,
    questions JSONB NOT NULL, -- Array of question objects
    passing_score_pct INT DEFAULT 80,
    time_limit_minutes INT,
    randomize_questions BOOLEAN DEFAULT TRUE,
    allow_retakes BOOLEAN DEFAULT TRUE,
    max_attempts INT,
    status VARCHAR(20) DEFAULT 'active'
        CHECK (status IN ('active', 'inactive', 'archived')),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    CONSTRAINT quiz_belongs_to_module_or_sop CHECK (
        (module_id IS NOT NULL AND sop_id IS NULL) OR 
        (module_id IS NULL AND sop_id IS NOT NULL)
    )
);

CREATE INDEX ix_quizzes_module ON quizzes (module_id);
CREATE INDEX ix_quizzes_sop ON quizzes (sop_id);

COMMENT ON TABLE quizzes IS 'Assessment quizzes for training modules and SOPs';
COMMENT ON COLUMN quizzes.questions IS 'JSON array of {question, type, options, correct_answer}';

-- ============================================================================
-- TRAINING_ASSIGNMENTS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS training_assignments (
    assignment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    module_id UUID REFERENCES training_modules(module_id) ON DELETE CASCADE,
    sop_id UUID REFERENCES sops(sop_id) ON DELETE CASCADE,
    assigned_by UUID REFERENCES users(user_id),
    assigned_at TIMESTAMPTZ DEFAULT NOW(),
    due_date DATE,
    status VARCHAR(20) DEFAULT 'assigned'
        CHECK (status IN ('assigned', 'in_progress', 'completed', 'expired', 'cancelled')),
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    score_pct DECIMAL(5,2) CHECK (score_pct IS NULL OR (score_pct >= 0 AND score_pct <= 100)), -- Quiz score if applicable
    passed BOOLEAN,
    attempts INT DEFAULT 0,
    time_spent_minutes INT,
    certificate_url TEXT, -- Link to certificate of completion
    expires_at TIMESTAMPTZ, -- If training requires renewal
    reminder_sent_at TIMESTAMPTZ,
    metadata JSONB DEFAULT '{}'::JSONB,
    CONSTRAINT assignment_has_module_or_sop CHECK (
        (module_id IS NOT NULL AND sop_id IS NULL) OR 
        (module_id IS NULL AND sop_id IS NOT NULL)
    )
);

CREATE INDEX ix_training_assignments_user ON training_assignments (user_id, status);
CREATE INDEX ix_training_assignments_due ON training_assignments (due_date) 
    WHERE status IN ('assigned', 'in_progress');
CREATE INDEX ix_training_assignments_expires ON training_assignments (expires_at) 
    WHERE status = 'completed' AND expires_at IS NOT NULL;
CREATE INDEX ix_training_assignments_module ON training_assignments (module_id);
CREATE INDEX ix_training_assignments_sop ON training_assignments (sop_id);

COMMENT ON TABLE training_assignments IS 'Tracks training assignments and completion for users';
COMMENT ON COLUMN training_assignments.expires_at IS 'When this training expires and must be renewed';

-- ============================================================================
-- SOP_SIGNOFFS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS sop_signoffs (
    signoff_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    sop_id UUID NOT NULL REFERENCES sops(sop_id) ON DELETE CASCADE,
    sop_version VARCHAR(20) NOT NULL, -- Version signed off
    signed_at TIMESTAMPTZ DEFAULT NOW(),
    ip_address INET,
    user_agent TEXT,
    attestation_text TEXT, -- "I have read and understood..."
    witness_user_id UUID REFERENCES users(user_id), -- Optional witness signature
    witness_signed_at TIMESTAMPTZ,
    expires_at TIMESTAMPTZ, -- If SOP requires periodic re-signoff
    revoked_at TIMESTAMPTZ,
    revoke_reason TEXT,
    UNIQUE (user_id, sop_id, sop_version)
);

CREATE INDEX ix_sop_signoffs_user ON sop_signoffs (user_id) WHERE revoked_at IS NULL;
CREATE INDEX ix_sop_signoffs_sop ON sop_signoffs (sop_id, sop_version);
CREATE INDEX ix_sop_signoffs_expires ON sop_signoffs (expires_at) 
    WHERE expires_at IS NOT NULL AND revoked_at IS NULL;

COMMENT ON TABLE sop_signoffs IS 'User attestations that they have read and understood SOPs';
COMMENT ON COLUMN sop_signoffs.witness_user_id IS 'Optional co-signer for critical SOPs';

-- ============================================================================
-- TASK_GATING_REQUIREMENTS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS task_gating_requirements (
    gating_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_type VARCHAR(100) NOT NULL, -- e.g., "harvest", "processing", "destruction"
    required_sop_id UUID REFERENCES sops(sop_id) ON DELETE CASCADE,
    required_module_id UUID REFERENCES training_modules(module_id) ON DELETE CASCADE,
    required_permission_action VARCHAR(100), -- ABAC action required
    site_id UUID REFERENCES sites(site_id) ON DELETE CASCADE, -- NULL = applies to all sites
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID REFERENCES users(user_id),
    CONSTRAINT gating_has_requirement CHECK (
        required_sop_id IS NOT NULL OR 
        required_module_id IS NOT NULL OR 
        required_permission_action IS NOT NULL
    )
);

CREATE INDEX ix_task_gating_task_type ON task_gating_requirements (task_type) WHERE is_active = TRUE;
CREATE INDEX ix_task_gating_site ON task_gating_requirements (site_id);

COMMENT ON TABLE task_gating_requirements IS 'Defines what SOPs/training/permissions are required to perform specific task types';
COMMENT ON COLUMN task_gating_requirements.task_type IS 'Type of task this requirement applies to';

-- ============================================================================
-- TASK GATING EVALUATION FUNCTION
-- ============================================================================
CREATE OR REPLACE FUNCTION check_task_gating(
    p_user_id UUID,
    p_task_type VARCHAR,
    p_site_id UUID
)
RETURNS TABLE (
    is_allowed BOOLEAN,
    missing_requirements JSONB
) AS $$
DECLARE
    v_missing JSONB := '[]'::JSONB;
    v_requirement RECORD;
    v_has_sop BOOLEAN;
    v_has_training BOOLEAN;
    v_has_permission BOOLEAN;
BEGIN
    -- Check each gating requirement for this task type
    FOR v_requirement IN 
        SELECT * FROM task_gating_requirements
        WHERE task_type = p_task_type
          AND is_active = TRUE
          AND (site_id IS NULL OR site_id = p_site_id)
    LOOP
        -- Check SOP requirement
        IF v_requirement.required_sop_id IS NOT NULL THEN
            SELECT EXISTS (
                SELECT 1 FROM sop_signoffs
                WHERE user_id = p_user_id
                  AND sop_id = v_requirement.required_sop_id
                  AND revoked_at IS NULL
                  AND (expires_at IS NULL OR expires_at > NOW())
            ) INTO v_has_sop;
            
            IF NOT v_has_sop THEN
                v_missing := v_missing || jsonb_build_object(
                    'type', 'sop',
                    'sop_id', v_requirement.required_sop_id,
                    'reason', 'SOP signoff required'
                );
            END IF;
        END IF;
        
        -- Check training requirement
        IF v_requirement.required_module_id IS NOT NULL THEN
            SELECT EXISTS (
                SELECT 1 FROM training_assignments
                WHERE user_id = p_user_id
                  AND module_id = v_requirement.required_module_id
                  AND status = 'completed'
                  AND passed = TRUE
                  AND (expires_at IS NULL OR expires_at > NOW())
            ) INTO v_has_training;
            
            IF NOT v_has_training THEN
                v_missing := v_missing || jsonb_build_object(
                    'type', 'training',
                    'module_id', v_requirement.required_module_id,
                    'reason', 'Training completion required'
                );
            END IF;
        END IF;
        
        -- Check ABAC permission requirement
        IF v_requirement.required_permission_action IS NOT NULL THEN
            SELECT granted FROM check_abac_permission(
                p_user_id,
                v_requirement.required_permission_action,
                'task', -- resource_type
                p_site_id,
                '{}'::JSONB,
                FALSE -- Don't log this check
            ) INTO v_has_permission;
            
            IF NOT v_has_permission THEN
                v_missing := v_missing || jsonb_build_object(
                    'type', 'permission',
                    'action', v_requirement.required_permission_action,
                    'reason', 'Required permission not granted'
                );
            END IF;
        END IF;
    END LOOP;
    
    RETURN QUERY SELECT 
        (jsonb_array_length(v_missing) = 0)::BOOLEAN,
        v_missing;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION check_task_gating IS 'Evaluates if user meets all requirements to perform a task type';

-- ============================================================================
-- RLS POLICIES
-- ============================================================================

-- SOPs: Organization-scoped (read) + admin modify
ALTER TABLE sops ENABLE ROW LEVEL SECURITY;

CREATE POLICY sops_read_all ON sops
FOR SELECT
USING (TRUE); -- All users can read SOPs

CREATE POLICY sops_admin_modify ON sops
FOR INSERT, UPDATE, DELETE
USING (current_setting('app.user_role', TRUE) IN ('admin', 'manager'))
WITH CHECK (current_setting('app.user_role', TRUE) IN ('admin', 'manager'));

-- Training modules: Same as SOPs
ALTER TABLE training_modules ENABLE ROW LEVEL SECURITY;

CREATE POLICY training_modules_read_all ON training_modules
FOR SELECT
USING (TRUE);

CREATE POLICY training_modules_admin_modify ON training_modules
FOR INSERT, UPDATE, DELETE
USING (current_setting('app.user_role', TRUE) IN ('admin', 'manager'))
WITH CHECK (current_setting('app.user_role', TRUE) IN ('admin', 'manager'));

-- Quizzes: Same as parent resources
ALTER TABLE quizzes ENABLE ROW LEVEL SECURITY;

CREATE POLICY quizzes_read_all ON quizzes
FOR SELECT
USING (TRUE);

CREATE POLICY quizzes_admin_modify ON quizzes
FOR INSERT, UPDATE, DELETE
USING (current_setting('app.user_role', TRUE) IN ('admin', 'manager'))
WITH CHECK (current_setting('app.user_role', TRUE) IN ('admin', 'manager'));

-- Training assignments: User-scoped
ALTER TABLE training_assignments ENABLE ROW LEVEL SECURITY;

CREATE POLICY training_assignments_self_access ON training_assignments
FOR ALL
USING (
    user_id = current_setting('app.current_user_id', TRUE)::UUID
    OR current_setting('app.user_role', TRUE) IN ('admin', 'manager', 'service_account')
);

-- SOP signoffs: User-scoped
ALTER TABLE sop_signoffs ENABLE ROW LEVEL SECURITY;

CREATE POLICY sop_signoffs_self_access ON sop_signoffs
FOR ALL
USING (
    user_id = current_setting('app.current_user_id', TRUE)::UUID
    OR current_setting('app.user_role', TRUE) IN ('admin', 'manager', 'service_account')
);

-- Task gating requirements: Read-all, admin-modify
ALTER TABLE task_gating_requirements ENABLE ROW LEVEL SECURITY;

CREATE POLICY task_gating_read_all ON task_gating_requirements
FOR SELECT
USING (TRUE);

CREATE POLICY task_gating_admin_modify ON task_gating_requirements
FOR INSERT, UPDATE, DELETE
USING (current_setting('app.user_role', TRUE) = 'admin')
WITH CHECK (current_setting('app.user_role', TRUE) = 'admin');

-- ============================================================================
-- AUDIT TRIGGERS
-- ============================================================================

CREATE TRIGGER sops_updated_at BEFORE UPDATE ON sops
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER training_modules_updated_at BEFORE UPDATE ON training_modules
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER quizzes_updated_at BEFORE UPDATE ON quizzes
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ============================================================================
-- COMPLETION
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE '✓ FRP-01 Training/SOP tables created successfully';
    RAISE NOTICE '  - sops: Standard Operating Procedures';
    RAISE NOTICE '  - training_modules: Training courses and certifications';
    RAISE NOTICE '  - quizzes: Assessment quizzes';
    RAISE NOTICE '  - training_assignments: User training tracking';
    RAISE NOTICE '  - sop_signoffs: User attestations';
    RAISE NOTICE '  - task_gating_requirements: Task prerequisite definitions';
    RAISE NOTICE '  - check_task_gating(): Function for prerequisite validation';
END $$;
