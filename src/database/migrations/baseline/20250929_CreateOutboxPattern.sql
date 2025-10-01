-- Migration: Create outbox pattern for reliable side-effects
-- Track A: Idempotent messaging for Slack, QBO, METRC integrations
-- Author: Integrations Squad
-- Date: 2025-09-29

-- ============================================================================
-- UP Migration: Outbox Table
-- ============================================================================

BEGIN;

-- Create outbox table for transactional outbox pattern
CREATE TABLE IF NOT EXISTS outbox_messages (
    message_id UUID NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    site_id UUID NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL, -- task, batch, inventory_lot, invoice, etc.
    aggregate_id UUID NOT NULL,
    event_type VARCHAR(100) NOT NULL, -- task.created, batch.phase_changed, etc.
    destination VARCHAR(50) NOT NULL, -- slack, qbo, metrc, biotrack
    payload JSONB NOT NULL,
    idempotency_key VARCHAR(255) NOT NULL, -- Unique key for deduplication
    priority SMALLINT DEFAULT 5, -- 1=highest, 10=lowest
    max_retry_count INT DEFAULT 5,
    retry_count INT DEFAULT 0,
    last_retry_at TIMESTAMPTZ,
    error_message TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'pending', -- pending, processing, completed, failed, dead_letter
    processed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Unique constraint on idempotency key per destination
CREATE UNIQUE INDEX IF NOT EXISTS uq_outbox_messages_idempotency 
    ON outbox_messages (destination, idempotency_key);

-- Index for worker polling (pending messages ordered by priority + created_at)
CREATE INDEX IF NOT EXISTS ix_outbox_messages_pending 
    ON outbox_messages (destination, status, priority ASC, created_at ASC)
    WHERE status = 'pending';

-- Index for retry logic
CREATE INDEX IF NOT EXISTS ix_outbox_messages_retry 
    ON outbox_messages (destination, status, last_retry_at ASC)
    WHERE status = 'failed' AND retry_count < max_retry_count;

-- Index for site-scoped queries
CREATE INDEX IF NOT EXISTS ix_outbox_messages_site 
    ON outbox_messages (site_id, created_at DESC);

-- Index for aggregate tracking
CREATE INDEX IF NOT EXISTS ix_outbox_messages_aggregate 
    ON outbox_messages (aggregate_type, aggregate_id, created_at DESC);

-- GIN index for payload queries
CREATE INDEX IF NOT EXISTS ix_outbox_messages_payload 
    ON outbox_messages USING GIN (payload);

-- Add RLS (site isolation will be tightened once user_sites table exists in FRP-01)
ALTER TABLE outbox_messages ENABLE ROW LEVEL SECURITY;

CREATE POLICY outbox_messages_service_account ON outbox_messages
FOR ALL
USING (current_setting('app.user_role', TRUE) = 'service_account');

-- Trigger to update updated_at
CREATE OR REPLACE FUNCTION update_outbox_messages_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_outbox_messages_updated_at
    BEFORE UPDATE ON outbox_messages
    FOR EACH ROW
    EXECUTE FUNCTION update_outbox_messages_updated_at();

COMMENT ON TABLE outbox_messages IS 'Transactional outbox for reliable side-effects with idempotency';
COMMENT ON COLUMN outbox_messages.idempotency_key IS 'Unique key preventing duplicate processing';
COMMENT ON COLUMN outbox_messages.priority IS '1=highest priority, 10=lowest priority';

COMMIT;

-- ============================================================================
-- UP Migration: Dead Letter Queue
-- ============================================================================

BEGIN;

-- Create dead_letter_queue for messages that exceeded retry count
CREATE TABLE IF NOT EXISTS dead_letter_queue (
    dlq_id UUID NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    original_message_id UUID NOT NULL,
    site_id UUID NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL,
    aggregate_id UUID NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    destination VARCHAR(50) NOT NULL,
    payload JSONB NOT NULL,
    idempotency_key VARCHAR(255) NOT NULL,
    failure_reason TEXT NOT NULL,
    retry_history JSONB, -- Array of retry attempts with timestamps and errors
    created_at TIMESTAMPTZ DEFAULT NOW(),
    reprocessed_at TIMESTAMPTZ,
    reprocessed_by UUID
);

-- Index for DLQ management
CREATE INDEX IF NOT EXISTS ix_dlq_site_created 
    ON dead_letter_queue (site_id, created_at DESC);

CREATE INDEX IF NOT EXISTS ix_dlq_destination 
    ON dead_letter_queue (destination, created_at DESC);

CREATE INDEX IF NOT EXISTS ix_dlq_unprocessed 
    ON dead_letter_queue (destination, created_at ASC)
    WHERE reprocessed_at IS NULL;

-- Add RLS
ALTER TABLE dead_letter_queue ENABLE ROW LEVEL SECURITY;

CREATE POLICY dlq_service_account ON dead_letter_queue
FOR ALL
USING (current_setting('app.user_role', TRUE) = 'service_account');

COMMENT ON TABLE dead_letter_queue IS 'Failed messages requiring manual intervention';

COMMIT;

-- ============================================================================
-- UP Migration: Outbox Processing Metrics
-- ============================================================================

BEGIN;

-- Create table for tracking outbox worker performance
CREATE TABLE IF NOT EXISTS outbox_processing_metrics (
    metric_id UUID NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    destination VARCHAR(50) NOT NULL,
    window_start TIMESTAMPTZ NOT NULL,
    window_end TIMESTAMPTZ NOT NULL,
    messages_processed INT NOT NULL DEFAULT 0,
    messages_succeeded INT NOT NULL DEFAULT 0,
    messages_failed INT NOT NULL DEFAULT 0,
    avg_processing_time_ms INT,
    p95_processing_time_ms INT,
    p99_processing_time_ms INT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Index for metrics queries
CREATE INDEX IF NOT EXISTS ix_outbox_metrics_destination_window 
    ON outbox_processing_metrics (destination, window_start DESC);

COMMENT ON TABLE outbox_processing_metrics IS 'Outbox worker performance metrics for SLO monitoring';

COMMIT;

-- ============================================================================
-- UP Migration: Helper Functions
-- ============================================================================

BEGIN;

-- Function to enqueue outbox message with idempotency check
CREATE OR REPLACE FUNCTION enqueue_outbox_message(
    p_site_id UUID,
    p_aggregate_type VARCHAR,
    p_aggregate_id UUID,
    p_event_type VARCHAR,
    p_destination VARCHAR,
    p_payload JSONB,
    p_idempotency_key VARCHAR,
    p_priority SMALLINT DEFAULT 5
)
RETURNS UUID AS $$
DECLARE
    v_message_id UUID;
BEGIN
    -- Try to insert, ignore if idempotency key already exists
    INSERT INTO outbox_messages (
        site_id,
        aggregate_type,
        aggregate_id,
        event_type,
        destination,
        payload,
        idempotency_key,
        priority,
        status
    ) VALUES (
        p_site_id,
        p_aggregate_type,
        p_aggregate_id,
        p_event_type,
        p_destination,
        p_payload,
        p_idempotency_key,
        p_priority,
        'pending'
    )
    ON CONFLICT (destination, idempotency_key) DO NOTHING
    RETURNING message_id INTO v_message_id;
    
    RETURN v_message_id;
END;
$$ LANGUAGE plpgsql;

-- Function to claim next pending message (for worker polling)
CREATE OR REPLACE FUNCTION claim_next_outbox_message(
    p_destination VARCHAR,
    p_worker_id VARCHAR
)
RETURNS TABLE (
    message_id UUID,
    aggregate_type VARCHAR,
    aggregate_id UUID,
    event_type VARCHAR,
    payload JSONB,
    retry_count INT
) AS $$
BEGIN
    RETURN QUERY
    UPDATE outbox_messages
    SET 
        status = 'processing',
        updated_at = NOW()
    WHERE message_id = (
        SELECT m.message_id
        FROM outbox_messages m
        WHERE m.destination = p_destination
          AND m.status = 'pending'
        ORDER BY m.priority ASC, m.created_at ASC
        LIMIT 1
        FOR UPDATE SKIP LOCKED
    )
    RETURNING 
        outbox_messages.message_id,
        outbox_messages.aggregate_type,
        outbox_messages.aggregate_id,
        outbox_messages.event_type,
        outbox_messages.payload,
        outbox_messages.retry_count;
END;
$$ LANGUAGE plpgsql;

-- Function to mark message as completed
CREATE OR REPLACE FUNCTION complete_outbox_message(
    p_message_id UUID
)
RETURNS VOID AS $$
BEGIN
    UPDATE outbox_messages
    SET 
        status = 'completed',
        processed_at = NOW(),
        updated_at = NOW()
    WHERE message_id = p_message_id;
END;
$$ LANGUAGE plpgsql;

-- Function to mark message as failed with retry logic
CREATE OR REPLACE FUNCTION fail_outbox_message(
    p_message_id UUID,
    p_error_message TEXT
)
RETURNS VOID AS $$
DECLARE
    v_retry_count INT;
    v_max_retry_count INT;
BEGIN
    -- Get current retry count
    SELECT retry_count, max_retry_count
    INTO v_retry_count, v_max_retry_count
    FROM outbox_messages
    WHERE message_id = p_message_id;
    
    -- Increment retry count
    v_retry_count := v_retry_count + 1;
    
    -- Check if max retries exceeded
    IF v_retry_count >= v_max_retry_count THEN
        -- Move to dead letter queue
        INSERT INTO dead_letter_queue (
            original_message_id,
            site_id,
            aggregate_type,
            aggregate_id,
            event_type,
            destination,
            payload,
            idempotency_key,
            failure_reason
        )
        SELECT 
            message_id,
            site_id,
            aggregate_type,
            aggregate_id,
            event_type,
            destination,
            payload,
            idempotency_key,
            p_error_message
        FROM outbox_messages
        WHERE message_id = p_message_id;
        
        -- Mark as dead_letter
        UPDATE outbox_messages
        SET 
            status = 'dead_letter',
            error_message = p_error_message,
            retry_count = v_retry_count,
            last_retry_at = NOW(),
            updated_at = NOW()
        WHERE message_id = p_message_id;
    ELSE
        -- Mark for retry
        UPDATE outbox_messages
        SET 
            status = 'failed',
            error_message = p_error_message,
            retry_count = v_retry_count,
            last_retry_at = NOW(),
            updated_at = NOW()
        WHERE message_id = p_message_id;
    END IF;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION enqueue_outbox_message IS 'Enqueue message with idempotency protection';
COMMENT ON FUNCTION claim_next_outbox_message IS 'Worker polling with SKIP LOCKED for concurrency';
COMMENT ON FUNCTION complete_outbox_message IS 'Mark message as successfully processed';
COMMENT ON FUNCTION fail_outbox_message IS 'Handle failure with retry logic and DLQ escalation';

COMMIT;

-- ============================================================================
-- DOWN Migration
-- ============================================================================

BEGIN;

-- Drop functions
DROP FUNCTION IF EXISTS fail_outbox_message(UUID, TEXT);
DROP FUNCTION IF EXISTS complete_outbox_message(UUID);
DROP FUNCTION IF EXISTS claim_next_outbox_message(VARCHAR, VARCHAR);
DROP FUNCTION IF EXISTS enqueue_outbox_message(UUID, VARCHAR, UUID, VARCHAR, VARCHAR, JSONB, VARCHAR, SMALLINT);

-- Drop metrics table
DROP TABLE IF EXISTS outbox_processing_metrics CASCADE;

-- Drop DLQ
DROP POLICY IF EXISTS dlq_service_account ON dead_letter_queue;
DROP TABLE IF EXISTS dead_letter_queue CASCADE;

-- Drop outbox
DROP TRIGGER IF EXISTS trg_outbox_messages_updated_at ON outbox_messages;
DROP FUNCTION IF EXISTS update_outbox_messages_updated_at();
DROP POLICY IF EXISTS outbox_messages_service_account ON outbox_messages;
DROP TABLE IF EXISTS outbox_messages CASCADE;

COMMIT;
