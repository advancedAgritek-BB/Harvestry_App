-- ============================================================================
-- FRP-01: Attribute-Based Access Control (ABAC)
-- Migration: Create ABAC Permission and Audit Tables
--
-- Description: ABAC policy engine for high-risk operation gating
-- Dependencies: 20250929_01_CreateIdentityTables.sql
-- ============================================================================

-- ============================================================================
-- ABAC_PERMISSIONS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS abac_permissions (
    permission_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    role_name VARCHAR(100) NOT NULL REFERENCES roles(role_name) ON DELETE CASCADE,
    action VARCHAR(100) NOT NULL, -- e.g., "approve_destruction", "override_interlock"
    resource_type VARCHAR(100) NOT NULL, -- e.g., "inventory_lot", "irrigation_program"
    conditions JSONB DEFAULT '{}'::JSONB, -- Additional context requirements
    requires_two_person BOOLEAN DEFAULT FALSE,
    requires_reason BOOLEAN DEFAULT FALSE,
    requires_attestation BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE (role_name, action, resource_type)
);

CREATE INDEX ix_abac_permissions_action ON abac_permissions (action, resource_type);
CREATE INDEX ix_abac_permissions_role ON abac_permissions (role_name);

COMMENT ON TABLE abac_permissions IS 'Fine-grained permissions for high-risk operations';
COMMENT ON COLUMN abac_permissions.conditions IS 'JSONB constraints, e.g., {"amount_lte": 10000, "promotion_checklist_completed": true}';
COMMENT ON COLUMN abac_permissions.requires_two_person IS 'If true, two different users with this permission must approve';

-- ============================================================================
-- AUTHORIZATION_AUDIT TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS authorization_audit (
    audit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
    site_id UUID REFERENCES sites(site_id) ON DELETE SET NULL,
    action VARCHAR(100) NOT NULL,
    resource_type VARCHAR(100) NOT NULL,
    resource_id UUID,
    granted BOOLEAN NOT NULL,
    context JSONB DEFAULT '{}'::JSONB, -- Request context
    deny_reason TEXT, -- Why was it denied?
    ip_address INET,
    user_agent TEXT,
    occurred_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX ix_authz_audit_user_occurred ON authorization_audit (user_id, occurred_at DESC);
CREATE INDEX ix_authz_audit_denied ON authorization_audit (occurred_at DESC) WHERE granted = FALSE;
CREATE INDEX ix_authz_audit_resource ON authorization_audit (resource_type, resource_id) WHERE granted = TRUE;
CREATE INDEX ix_authz_audit_action ON authorization_audit (action, occurred_at DESC);

COMMENT ON TABLE authorization_audit IS 'Immutable audit trail of all authorization checks';
COMMENT ON COLUMN authorization_audit.granted IS 'TRUE if permission granted, FALSE if denied';
COMMENT ON COLUMN authorization_audit.context IS 'Full request context for debugging and compliance';

-- ============================================================================
-- TWO_PERSON_APPROVALS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS two_person_approvals (
    approval_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    action VARCHAR(100) NOT NULL,
    resource_type VARCHAR(100) NOT NULL,
    resource_id UUID NOT NULL,
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    initiator_user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE SET NULL,
    initiator_reason TEXT NOT NULL,
    initiator_attestation TEXT,
    initiated_at TIMESTAMPTZ DEFAULT NOW(),
    approver_user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
    approver_reason TEXT,
    approver_attestation TEXT,
    approved_at TIMESTAMPTZ,
    status VARCHAR(20) DEFAULT 'pending'
        CHECK (status IN ('pending', 'approved', 'rejected', 'expired', 'cancelled')),
    expires_at TIMESTAMPTZ DEFAULT NOW() + INTERVAL '24 hours',
    completed_at TIMESTAMPTZ,
    context JSONB DEFAULT '{}'::JSONB,
    CONSTRAINT different_approvers CHECK (approver_user_id IS NULL OR initiator_user_id != approver_user_id)
);

CREATE INDEX ix_two_person_pending ON two_person_approvals (site_id, status, expires_at) 
    WHERE status = 'pending';
CREATE INDEX ix_two_person_resource ON two_person_approvals (resource_type, resource_id);
CREATE INDEX ix_two_person_initiator ON two_person_approvals (initiator_user_id, status);

COMMENT ON TABLE two_person_approvals IS 'Two-person approval workflow for high-risk operations';
COMMENT ON CONSTRAINT different_approvers ON two_person_approvals IS 'Approver must be different from initiator';

-- ============================================================================
-- ABAC PERMISSION CHECK FUNCTION
-- ============================================================================
CREATE OR REPLACE FUNCTION check_abac_permission(
    p_user_id UUID,
    p_action VARCHAR,
    p_resource_type VARCHAR,
    p_site_id UUID,
    p_context JSONB DEFAULT '{}'::JSONB,
    p_log_check BOOLEAN DEFAULT TRUE,
    p_resource_id UUID DEFAULT NULL
)
RETURNS TABLE (
    granted BOOLEAN,
    requires_two_person BOOLEAN,
    deny_reason TEXT
) AS $$
DECLARE
    v_user_role VARCHAR;
    v_permission_exists BOOLEAN := FALSE;
    v_requires_two_person BOOLEAN := FALSE;
    v_conditions_met BOOLEAN := TRUE;
    v_deny_reason TEXT := NULL;
BEGIN
    -- Get user's role for this site
    SELECT r.role_name INTO v_user_role
    FROM user_sites us
    JOIN roles r ON r.role_id = us.role_id
    WHERE us.user_id = p_user_id 
      AND us.site_id = p_site_id
      AND us.revoked_at IS NULL;
    
    -- If user not assigned to site, deny
    IF v_user_role IS NULL THEN
        v_deny_reason := 'User not assigned to site';
        
        -- Log the failed check
        IF p_log_check THEN
            INSERT INTO authorization_audit (
                user_id, site_id, action, resource_type, resource_id,
                granted, deny_reason, context
            ) VALUES (
                p_user_id, p_site_id, p_action, p_resource_type, p_resource_id,
                FALSE, v_deny_reason, p_context
            );
        END IF;
        
        RETURN QUERY SELECT FALSE, FALSE, v_deny_reason;
        RETURN;
    END IF;
    
    -- Check if permission exists for this role/action/resource
    -- NOTE: Condition evaluation is simplified (uses JSONB @> containment).
    -- For complex operators (_lte, _gte, etc.), implement a dedicated PL/pgSQL
    -- function or move evaluation to application code.
    SELECT 
        TRUE,
        ap.requires_two_person,
        CASE 
            WHEN ap.conditions IS NOT NULL AND ap.conditions != '{}'::JSONB 
            THEN p_context @> ap.conditions -- Simplified: checks if context contains conditions
            ELSE TRUE
        END
    INTO v_permission_exists, v_requires_two_person, v_conditions_met
    FROM abac_permissions ap
    WHERE ap.role_name = v_user_role
      AND ap.action = p_action
      AND ap.resource_type = p_resource_type;
    
    -- Determine result
    IF NOT v_permission_exists THEN
        v_deny_reason := format('Role %s does not have permission for action %s on %s', 
                                v_user_role, p_action, p_resource_type);
    ELSIF NOT v_conditions_met THEN
        v_deny_reason := 'Required conditions not met';
    ELSE
        v_deny_reason := NULL; -- Granted
    END IF;
    
    -- Log the authorization check
    IF p_log_check THEN
        INSERT INTO authorization_audit (
            user_id, site_id, action, resource_type, resource_id,
            granted, deny_reason, context
        ) VALUES (
            p_user_id, p_site_id, p_action, p_resource_type, p_resource_id,
            (v_permission_exists AND v_conditions_met), v_deny_reason, p_context
        );
    END IF;
    
    RETURN QUERY SELECT 
        (v_permission_exists AND v_conditions_met)::BOOLEAN,
        v_requires_two_person,
        v_deny_reason;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

COMMENT ON FUNCTION check_abac_permission IS 'Evaluates ABAC permissions and logs to audit trail';

-- ============================================================================
-- RLS POLICIES
-- ============================================================================

-- ABAC_PERMISSIONS: Admin-only
ALTER TABLE abac_permissions ENABLE ROW LEVEL SECURITY;

CREATE POLICY abac_permissions_admin_only ON abac_permissions
FOR ALL
USING (current_setting('app.user_role', TRUE) IN ('admin', 'service_account'));

-- AUTHORIZATION_AUDIT: Read-only, site-scoped
ALTER TABLE authorization_audit ENABLE ROW LEVEL SECURITY;

CREATE POLICY authz_audit_site_scoped ON authorization_audit
FOR SELECT
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
        AND revoked_at IS NULL
    )
    OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
);

-- INSERT allowed for service_account/admin only, no direct updates/deletes
CREATE POLICY authz_audit_service_insert ON authorization_audit
FOR INSERT
WITH CHECK (current_setting('app.user_role', TRUE) IN ('service_account', 'admin'));

CREATE POLICY authz_audit_no_update ON authorization_audit
FOR UPDATE
USING (FALSE)
WITH CHECK (FALSE);

CREATE POLICY authz_audit_no_delete ON authorization_audit
FOR DELETE
USING (FALSE);

-- TWO_PERSON_APPROVALS: Site-scoped
ALTER TABLE two_person_approvals ENABLE ROW LEVEL SECURITY;

CREATE POLICY two_person_site_scoped ON two_person_approvals
FOR ALL
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
        AND revoked_at IS NULL
    )
    OR current_setting('app.user_role', TRUE) IN ('service_account', 'admin')
);

GRANT SELECT, INSERT, UPDATE, DELETE ON abac_permissions TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON authorization_audit TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON two_person_approvals TO harvestry_app;

-- ============================================================================
-- SEED DEFAULT ABAC PERMISSIONS
-- ============================================================================

INSERT INTO abac_permissions (role_name, action, resource_type, requires_two_person, requires_reason) VALUES
    -- Inventory destruction (high-risk)
    ('manager', 'approve_destruction', 'inventory_lot', TRUE, TRUE),
    ('admin', 'approve_destruction', 'inventory_lot', TRUE, TRUE),
    
    -- Irrigation override (safety-critical)
    ('manager', 'override_interlock', 'irrigation_program', FALSE, TRUE),
    ('admin', 'override_interlock', 'irrigation_program', FALSE, TRUE),
    
    -- High-value financial approvals
    ('manager', 'approve_invoice', 'invoice', FALSE, FALSE),
    ('admin', 'approve_invoice', 'invoice', FALSE, FALSE),
    
    -- User management
    ('admin', 'revoke_license', 'user', FALSE, TRUE),
    ('admin', 'assign_role', 'user', FALSE, FALSE),
    
    -- Compliance actions
    ('manager', 'submit_compliance_report', 'compliance_report', FALSE, FALSE),
    ('admin', 'submit_compliance_report', 'compliance_report', FALSE, FALSE)
ON CONFLICT (role_name, action, resource_type) DO NOTHING;

-- ============================================================================
-- AUDIT TRIGGERS
-- ============================================================================

CREATE TRIGGER abac_permissions_updated_at BEFORE UPDATE ON abac_permissions
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- ============================================================================
-- COMPLETION
-- ============================================================================

DO $$
BEGIN
    RAISE NOTICE '✓ FRP-01 ABAC tables created successfully';
    RAISE NOTICE '  - abac_permissions: Fine-grained permission definitions';
    RAISE NOTICE '  - authorization_audit: Immutable audit trail';
    RAISE NOTICE '  - two_person_approvals: Dual-approval workflow for high-risk ops';
    RAISE NOTICE '  - check_abac_permission(): Function for permission evaluation';
END $$;
