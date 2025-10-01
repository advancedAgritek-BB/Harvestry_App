-- Audit Trail Hash Chain
-- Track A: Tamper-evident audit logging with nightly verification

BEGIN;

-- ============================================================================
-- Audit Trail Table with Hash Chain
-- ============================================================================

CREATE TABLE IF NOT EXISTS audit_trail (
    audit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- Audit metadata
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    user_id UUID NOT NULL,
    site_id UUID,
    session_id UUID,
    ip_address INET,
    user_agent TEXT,
    
    -- Event details
    event_type VARCHAR(100) NOT NULL, -- CREATE, UPDATE, DELETE, LOGIN, LOGOUT, etc.
    entity_type VARCHAR(100) NOT NULL, -- user, task, inventory_lot, etc.
    entity_id UUID,
    action VARCHAR(200) NOT NULL,
    
    -- Change tracking
    old_values JSONB,
    new_values JSONB,
    diff JSONB, -- Computed diff between old and new
    
    -- Context
    context JSONB, -- Additional metadata
    result VARCHAR(50) NOT NULL, -- SUCCESS, FAILURE, PARTIAL
    error_message TEXT,
    
    -- Hash chain for tamper detection
    previous_hash VARCHAR(64), -- SHA-256 of previous audit entry
    current_hash VARCHAR(64) NOT NULL, -- SHA-256 of this entry
    
    -- Verification
    verified_at TIMESTAMPTZ,
    verification_status VARCHAR(20) DEFAULT 'pending', -- pending, verified, tampered
    
    -- Immutability flag (WORM - Write Once Read Many)
    is_immutable BOOLEAN DEFAULT TRUE
);

-- Indexes
CREATE INDEX ix_audit_trail_occurred ON audit_trail (occurred_at DESC);
CREATE INDEX ix_audit_trail_user ON audit_trail (user_id, occurred_at DESC);
CREATE INDEX ix_audit_trail_site ON audit_trail (site_id, occurred_at DESC);
CREATE INDEX ix_audit_trail_entity ON audit_trail (entity_type, entity_id, occurred_at DESC);
CREATE INDEX ix_audit_trail_event ON audit_trail (event_type, occurred_at DESC);
CREATE INDEX ix_audit_trail_verification ON audit_trail (verification_status, occurred_at DESC);
CREATE INDEX ix_audit_trail_hash ON audit_trail (current_hash);

COMMENT ON TABLE audit_trail IS 'Tamper-evident audit log with cryptographic hash chain';
COMMENT ON COLUMN audit_trail.previous_hash IS 'SHA-256 hash of previous entry for chain integrity';
COMMENT ON COLUMN audit_trail.current_hash IS 'SHA-256 hash of this entry (user_id|occurred_at|action|previous_hash)';

-- ============================================================================
-- Hash Generation Function
-- ============================================================================

CREATE OR REPLACE FUNCTION compute_audit_hash(
    p_user_id UUID,
    p_occurred_at TIMESTAMPTZ,
    p_action VARCHAR,
    p_entity_type VARCHAR,
    p_entity_id UUID,
    p_previous_hash VARCHAR
)
RETURNS VARCHAR AS $$
DECLARE
    v_data TEXT;
BEGIN
    -- Concatenate data for hashing
    v_data := CONCAT(
        COALESCE(p_user_id::TEXT, ''),
        '|',
        p_occurred_at::TEXT,
        '|',
        p_action,
        '|',
        p_entity_type,
        '|',
        COALESCE(p_entity_id::TEXT, ''),
        '|',
        COALESCE(p_previous_hash, '')
    );
    
    -- Return SHA-256 hash
    RETURN encode(digest(v_data, 'sha256'), 'hex');
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- ============================================================================
-- Audit Logging Function
-- ============================================================================

CREATE OR REPLACE FUNCTION log_audit_event(
    p_user_id UUID,
    p_site_id UUID,
    p_event_type VARCHAR,
    p_entity_type VARCHAR,
    p_entity_id UUID,
    p_action VARCHAR,
    p_old_values JSONB DEFAULT NULL,
    p_new_values JSONB DEFAULT NULL,
    p_context JSONB DEFAULT NULL,
    p_result VARCHAR DEFAULT 'SUCCESS'
)
RETURNS UUID AS $$
DECLARE
    v_audit_id UUID;
    v_previous_hash VARCHAR;
    v_current_hash VARCHAR;
    v_diff JSONB;
BEGIN
    -- Get previous hash (last audit entry)
    SELECT current_hash INTO v_previous_hash
    FROM audit_trail
    ORDER BY occurred_at DESC
    LIMIT 1;
    
    -- Compute diff if both old and new values provided
    IF p_old_values IS NOT NULL AND p_new_values IS NOT NULL THEN
        SELECT jsonb_build_object(
            'added', p_new_values - p_old_values,
            'removed', p_old_values - p_new_values,
            'changed', COALESCE(changed_agg, '{}'::jsonb)
        ) INTO v_diff
        FROM (
            SELECT jsonb_object_agg(
                key,
                jsonb_build_object('old', p_old_values->key, 'new', p_new_values->key)
            ) as changed_agg
            FROM (
                SELECT key FROM jsonb_object_keys(p_old_values) key
                INTERSECT
                SELECT key FROM jsonb_object_keys(p_new_values) key
            ) keys
            WHERE p_old_values->key != p_new_values->key
        ) diff_query;
    END IF;
    
    -- Compute current hash
    v_current_hash := compute_audit_hash(
        p_user_id,
        NOW(),
        p_action,
        p_entity_type,
        p_entity_id,
        v_previous_hash
    );
    
    -- Insert audit entry
    INSERT INTO audit_trail (
        user_id,
        site_id,
        event_type,
        entity_type,
        entity_id,
        action,
        old_values,
        new_values,
        diff,
        context,
        result,
        previous_hash,
        current_hash
    ) VALUES (
        p_user_id,
        p_site_id,
        p_event_type,
        p_entity_type,
        p_entity_id,
        p_action,
        p_old_values,
        p_new_values,
        v_diff,
        p_context,
        p_result,
        v_previous_hash,
        v_current_hash
    )
    RETURNING audit_id INTO v_audit_id;
    
    RETURN v_audit_id;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

COMMENT ON FUNCTION log_audit_event IS 'Create tamper-evident audit log entry with hash chain';

-- ============================================================================
-- Hash Chain Verification Function
-- ============================================================================

CREATE OR REPLACE FUNCTION verify_audit_hash_chain(
    p_start_date TIMESTAMPTZ DEFAULT NOW() - INTERVAL '1 day',
    p_end_date TIMESTAMPTZ DEFAULT NOW()
)
RETURNS TABLE (
    audit_id UUID,
    occurred_at TIMESTAMPTZ,
    expected_hash VARCHAR,
    actual_hash VARCHAR,
    is_valid BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    WITH audit_sequence AS (
        SELECT 
            a.audit_id,
            a.occurred_at,
            a.user_id,
            a.action,
            a.entity_type,
            a.entity_id,
            a.previous_hash,
            a.current_hash,
            LAG(a.current_hash) OVER (ORDER BY a.occurred_at) as expected_previous_hash
        FROM audit_trail a
        WHERE a.occurred_at BETWEEN p_start_date AND p_end_date
        ORDER BY a.occurred_at
    )
    SELECT
        s.audit_id,
        s.occurred_at,
        compute_audit_hash(
            s.user_id,
            s.occurred_at,
            s.action,
            s.entity_type,
            s.entity_id,
            s.previous_hash
        ) as expected_hash,
        s.current_hash as actual_hash,
        compute_audit_hash(
            s.user_id,
            s.occurred_at,
            s.action,
            s.entity_type,
            s.entity_id,
            s.previous_hash
        ) = s.current_hash AND
        s.previous_hash = s.expected_previous_hash as is_valid
    FROM audit_sequence s;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION verify_audit_hash_chain IS 'Verify integrity of audit hash chain for tampering detection';

-- ============================================================================
-- Nightly Verification Job Function
-- ============================================================================

CREATE OR REPLACE FUNCTION run_nightly_audit_verification()
RETURNS TABLE (
    verification_date DATE,
    total_records BIGINT,
    valid_records BIGINT,
    invalid_records BIGINT,
    tampering_detected BOOLEAN
) AS $$
DECLARE
    v_start_date TIMESTAMPTZ;
    v_end_date TIMESTAMPTZ;
    v_total BIGINT;
    v_valid BIGINT;
    v_invalid BIGINT;
BEGIN
    -- Verify last 24 hours
    v_end_date := NOW();
    v_start_date := v_end_date - INTERVAL '24 hours';
    
    -- Count valid and invalid records
    SELECT 
        COUNT(*),
        SUM(CASE WHEN is_valid THEN 1 ELSE 0 END),
        SUM(CASE WHEN NOT is_valid THEN 1 ELSE 0 END)
    INTO v_total, v_valid, v_invalid
    FROM verify_audit_hash_chain(v_start_date, v_end_date);
    
    -- Update verification status
    UPDATE audit_trail
    SET 
        verified_at = NOW(),
        verification_status = CASE 
            WHEN audit_id IN (
                SELECT audit_id 
                FROM verify_audit_hash_chain(v_start_date, v_end_date) 
                WHERE is_valid = TRUE
            ) THEN 'verified'
            ELSE 'tampered'
        END
    WHERE occurred_at BETWEEN v_start_date AND v_end_date
      AND verification_status = 'pending';
    
    -- Return summary
    RETURN QUERY
    SELECT 
        CURRENT_DATE as verification_date,
        v_total,
        v_valid,
        v_invalid,
        (v_invalid > 0) as tampering_detected;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION run_nightly_audit_verification IS 'Scheduled job to verify audit log integrity';

-- ============================================================================
-- Prevent Audit Trail Modifications (RLS)
-- ============================================================================

ALTER TABLE audit_trail ENABLE ROW LEVEL SECURITY;

-- Read-only access for all users
CREATE POLICY audit_trail_readonly ON audit_trail
FOR SELECT
USING (TRUE);


-- Prevent updates (immutable rows)
CREATE POLICY audit_trail_no_update ON audit_trail
FOR UPDATE
USING (FALSE);

-- Prevent deletes (immutable rows)
CREATE POLICY audit_trail_no_delete ON audit_trail
FOR DELETE
USING (FALSE);

-- Inserts only through log_audit_event function (security definer)
CREATE POLICY audit_trail_insert_via_function ON audit_trail
FOR INSERT
WITH CHECK (FALSE); -- Force use of log_audit_event function

-- ============================================================================
-- Audit Verification Log
-- ============================================================================

CREATE TABLE IF NOT EXISTS audit_verification_log (
    verification_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    verification_date DATE NOT NULL DEFAULT CURRENT_DATE,
    start_time TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    end_time TIMESTAMPTZ,
    total_records BIGINT,
    valid_records BIGINT,
    invalid_records BIGINT,
    tampering_detected BOOLEAN DEFAULT FALSE,
    tampered_record_ids UUID[],
    notified_at TIMESTAMPTZ,
    UNIQUE (verification_date)
);

CREATE INDEX ix_audit_verification_date ON audit_verification_log (verification_date DESC);

COMMENT ON TABLE audit_verification_log IS 'Log of nightly audit integrity verifications';

COMMIT;

-- ============================================================================
-- Usage Examples
-- ============================================================================

-- Log an audit event
/*
SELECT log_audit_event(
    p_user_id := 'user-uuid',
    p_site_id := 'site-uuid',
    p_event_type := 'UPDATE',
    p_entity_type := 'inventory_lot',
    p_entity_id := 'lot-uuid',
    p_action := 'Updated quantity',
    p_old_values := '{"quantity": 100}'::JSONB,
    p_new_values := '{"quantity": 95}'::JSONB,
    p_context := '{"reason": "Product dispensed"}'::JSONB
);
*/

-- Verify hash chain
/*
SELECT * FROM verify_audit_hash_chain(
    NOW() - INTERVAL '1 day',
    NOW()
);
*/

-- Run nightly verification
/*
SELECT * FROM run_nightly_audit_verification();
*/
