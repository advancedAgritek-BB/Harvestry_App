-- ABAC High-Risk Operations Framework
-- Track A: Attribute-Based Access Control for safety-critical actions
-- Author: Security Squad
-- Date: 2025-09-29

-- ============================================================================
-- ABAC Permissions Table
-- ============================================================================

BEGIN;

CREATE TABLE IF NOT EXISTS abac_permissions (
    permission_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    role VARCHAR(50) NOT NULL,
    action VARCHAR(100) NOT NULL,
    resource_type VARCHAR(100) NOT NULL,
    description TEXT,
    conditions JSONB, -- Additional context requirements (e.g., {"amount_lte": 10000})
    requires_two_person BOOLEAN DEFAULT FALSE,
    requires_reason BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID,
    UNIQUE (role, action, resource_type)
);

CREATE INDEX ix_abac_permissions_role ON abac_permissions (role, action);
CREATE INDEX ix_abac_permissions_action ON abac_permissions (action, resource_type);

COMMENT ON TABLE abac_permissions IS 'ABAC permission matrix for high-risk operations';
COMMENT ON COLUMN abac_permissions.conditions IS 'JSONB context requirements (e.g., value thresholds, approval chains)';

-- ============================================================================
-- Authorization Audit Table
-- ============================================================================

CREATE TABLE IF NOT EXISTS authorization_audit (
    audit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    site_id UUID,
    action VARCHAR(100) NOT NULL,
    resource_type VARCHAR(100) NOT NULL,
    resource_id UUID,
    granted BOOLEAN NOT NULL,
    denial_reason TEXT,
    context JSONB,
    ip_address INET,
    user_agent TEXT,
    occurred_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX ix_authz_audit_user_occurred ON authorization_audit (user_id, occurred_at DESC);
CREATE INDEX ix_authz_audit_site_occurred ON authorization_audit (site_id, occurred_at DESC);
CREATE INDEX ix_authz_audit_denied ON authorization_audit (occurred_at DESC) WHERE granted = FALSE;
CREATE INDEX ix_authz_audit_action ON authorization_audit (action, occurred_at DESC);

-- Partition by month for performance
-- SELECT create_hypertable('authorization_audit', 'occurred_at', if_not_exists => TRUE);

COMMENT ON TABLE authorization_audit IS 'Tamper-evident log of all ABAC authorization decisions';

-- ============================================================================
-- Two-Person Approval Tracking
-- ============================================================================

CREATE TABLE IF NOT EXISTS two_person_approvals (
    approval_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    resource_type VARCHAR(100) NOT NULL,
    resource_id UUID NOT NULL,
    action VARCHAR(100) NOT NULL,
    site_id UUID NOT NULL,
    first_approver_id UUID NOT NULL,
    first_approved_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    second_approver_id UUID,
    second_approved_at TIMESTAMPTZ,
    status VARCHAR(20) DEFAULT 'pending', -- pending, approved, expired, cancelled
    reason TEXT,
    context JSONB,
    expires_at TIMESTAMPTZ DEFAULT (NOW() + INTERVAL '24 hours'),
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX ix_two_person_pending ON two_person_approvals (resource_type, resource_id, status)
    WHERE status = 'pending';

CREATE INDEX ix_two_person_expires ON two_person_approvals (expires_at)
    WHERE status = 'pending';

COMMENT ON TABLE two_person_approvals IS 'Two-person approval tracking for high-risk operations';

COMMIT;

-- ============================================================================
-- ABAC Permission Check Function
-- ============================================================================

BEGIN;

CREATE OR REPLACE FUNCTION check_abac_permission(
    p_user_id UUID,
    p_action VARCHAR,
    p_resource_type VARCHAR,
    p_site_id UUID,
    p_resource_id UUID DEFAULT NULL,
    p_context JSONB DEFAULT '{}'::JSONB
)
RETURNS BOOLEAN AS $$
DECLARE
    v_user_role VARCHAR;
    v_has_permission BOOLEAN := FALSE;
    v_requires_two_person BOOLEAN := FALSE;
    v_denial_reason TEXT;
    v_permission_conditions JSONB;
BEGIN
    -- Get user's role for this site
    SELECT us.role INTO v_user_role
    FROM user_sites us
    WHERE us.user_id = p_user_id AND us.site_id = p_site_id;
    
    IF v_user_role IS NULL THEN
        v_denial_reason := 'User not assigned to site';
        v_has_permission := FALSE;
    ELSE
        -- Check if permission exists for this role
        SELECT 
            TRUE,
            requires_two_person,
            conditions
        INTO 
            v_has_permission,
            v_requires_two_person,
            v_permission_conditions
        FROM abac_permissions
        WHERE role = v_user_role
          AND action = p_action
          AND resource_type = p_resource_type
          AND (conditions IS NULL OR conditions @> p_context);
        
        IF NOT FOUND THEN
            v_denial_reason := format('No permission for role %s to perform %s on %s', 
                                     v_user_role, p_action, p_resource_type);
            v_has_permission := FALSE;
        ELSIF v_requires_two_person AND p_resource_id IS NOT NULL THEN
            -- Check if two-person approval is complete
            SELECT TRUE INTO v_has_permission
            FROM two_person_approvals
            WHERE resource_type = p_resource_type
              AND resource_id = p_resource_id
              AND action = p_action
              AND status = 'approved'
              AND second_approved_at IS NOT NULL;
            
            IF NOT FOUND THEN
                v_denial_reason := 'Two-person approval required but not completed';
                v_has_permission := FALSE;
            END IF;
        END IF;
    END IF;
    
    -- Log the authorization check
    INSERT INTO authorization_audit (
        user_id,
        site_id,
        action,
        resource_type,
        resource_id,
        granted,
        denial_reason,
        context
    ) VALUES (
        p_user_id,
        p_site_id,
        p_action,
        p_resource_type,
        p_resource_id,
        v_has_permission,
        v_denial_reason,
        p_context
    );
    
    RETURN v_has_permission;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

COMMENT ON FUNCTION check_abac_permission IS 'Check ABAC permission with audit logging and two-person approval support';

-- ============================================================================
-- Two-Person Approval Functions
-- ============================================================================

-- Initiate two-person approval
CREATE OR REPLACE FUNCTION initiate_two_person_approval(
    p_resource_type VARCHAR,
    p_resource_id UUID,
    p_action VARCHAR,
    p_site_id UUID,
    p_first_approver_id UUID,
    p_reason TEXT,
    p_context JSONB DEFAULT '{}'::JSONB
)
RETURNS UUID AS $$
DECLARE
    v_approval_id UUID;
BEGIN
    INSERT INTO two_person_approvals (
        resource_type,
        resource_id,
        action,
        site_id,
        first_approver_id,
        reason,
        context
    ) VALUES (
        p_resource_type,
        p_resource_id,
        p_action,
        p_site_id,
        p_first_approver_id,
        p_reason,
        p_context
    )
    RETURNING approval_id INTO v_approval_id;
    
    RETURN v_approval_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Complete two-person approval
CREATE OR REPLACE FUNCTION complete_two_person_approval(
    p_approval_id UUID,
    p_second_approver_id UUID
)
RETURNS BOOLEAN AS $$
DECLARE
    v_first_approver_id UUID;
    v_status VARCHAR;
    v_expires_at TIMESTAMPTZ;
BEGIN
    -- Get approval details
    SELECT first_approver_id, status, expires_at
    INTO v_first_approver_id, v_status, v_expires_at
    FROM two_person_approvals
    WHERE approval_id = p_approval_id;
    
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Approval not found';
    END IF;
    
    -- Validate approval can be completed
    IF v_status != 'pending' THEN
        RAISE EXCEPTION 'Approval already completed or expired';
    END IF;
    
    IF NOW() > v_expires_at THEN
        UPDATE two_person_approvals
        SET status = 'expired'
        WHERE approval_id = p_approval_id;
        RAISE EXCEPTION 'Approval has expired';
    END IF;
    
    IF v_first_approver_id = p_second_approver_id THEN
        RAISE EXCEPTION 'Second approver must be different from first approver';
    END IF;
    
    -- Complete approval
    UPDATE two_person_approvals
    SET 
        second_approver_id = p_second_approver_id,
        second_approved_at = NOW(),
        status = 'approved'
    WHERE approval_id = p_approval_id;
    
    RETURN TRUE;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

COMMENT ON FUNCTION initiate_two_person_approval IS 'Start two-person approval process';
COMMENT ON FUNCTION complete_two_person_approval IS 'Complete second approval (validates different approver)';

COMMIT;

-- ============================================================================
-- Seed Default ABAC Permissions
-- ============================================================================

BEGIN;

-- Inventory destruction (requires two-person approval)
INSERT INTO abac_permissions (role, action, resource_type, description, requires_two_person, requires_reason)
VALUES 
    ('manager', 'approve_destruction', 'inventory_lot', 'Approve inventory destruction', TRUE, TRUE),
    ('compliance_officer', 'approve_destruction', 'inventory_lot', 'Approve inventory destruction', TRUE, TRUE);

-- Closed-loop control enablement (requires promotion checklist)
INSERT INTO abac_permissions (role, action, resource_type, description, conditions, requires_reason)
VALUES 
    ('admin', 'enable_closed_loop', 'control_loop', 'Enable closed-loop EC/pH control', 
     '{"promotion_checklist_completed": true}'::JSONB, TRUE),
    ('vp_product', 'enable_closed_loop', 'control_loop', 'Enable closed-loop EC/pH control', 
     '{"promotion_checklist_completed": true}'::JSONB, TRUE);

-- Interlock override (requires reason and manager role)
INSERT INTO abac_permissions (role, action, resource_type, description, requires_reason)
VALUES 
    ('manager', 'override_interlock', 'irrigation_program', 'Override safety interlock', TRUE),
    ('head_grower', 'override_interlock', 'irrigation_program', 'Override safety interlock', TRUE);

-- Invoice approval (amount-based thresholds)
INSERT INTO abac_permissions (role, action, resource_type, description, conditions)
VALUES 
    ('finance_manager', 'approve_invoice', 'invoice', 'Approve invoices up to $10,000', 
     '{"amount_lte": 10000}'::JSONB),
    ('vp_finance', 'approve_invoice', 'invoice', 'Approve invoices (no limit)', NULL);

-- High-value invoice (requires two-person)
INSERT INTO abac_permissions (role, action, resource_type, description, requires_two_person, conditions)
VALUES 
    ('vp_finance', 'approve_high_value_invoice', 'invoice', 'Approve invoices > $10,000', 
     TRUE, '{"amount_gt": 10000}'::JSONB);

-- User license revocation (requires reason)
INSERT INTO abac_permissions (role, action, resource_type, description, requires_reason)
VALUES 
    ('admin', 'revoke_license', 'user', 'Revoke user access', TRUE);

-- Bulk operations (admin only)
INSERT INTO abac_permissions (role, action, resource_type, description, requires_reason)
VALUES 
    ('admin', 'bulk_delete', 'any', 'Bulk delete operations', TRUE);

-- Feature flag management
INSERT INTO abac_permissions (role, action, resource_type, description, requires_reason)
VALUES 
    ('admin', 'toggle_feature_flag', 'feature_flag', 'Enable/disable feature flags', TRUE),
    ('vp_product', 'toggle_feature_flag', 'feature_flag', 'Enable/disable feature flags', TRUE);

COMMENT ON TABLE abac_permissions IS 'Seeded with default high-risk operation permissions';

COMMIT;

-- ============================================================================
-- Monitoring Queries
-- ============================================================================

-- Query: ABAC denials in last hour
/*
SELECT 
    action,
    resource_type,
    COUNT(*) as denial_count,
    COUNT(DISTINCT user_id) as affected_users
FROM authorization_audit
WHERE granted = FALSE
  AND occurred_at > NOW() - INTERVAL '1 hour'
GROUP BY action, resource_type
ORDER BY denial_count DESC;
*/

-- Query: Two-person approvals pending expiration
/*
SELECT 
    resource_type,
    resource_id,
    action,
    first_approver_id,
    expires_at,
    EXTRACT(EPOCH FROM (expires_at - NOW())) / 3600 as hours_until_expiration
FROM two_person_approvals
WHERE status = 'pending'
  AND expires_at > NOW()
ORDER BY expires_at ASC;
*/
