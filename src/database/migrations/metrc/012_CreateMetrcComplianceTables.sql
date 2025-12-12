-- Migration: 012_CreateMetrcComplianceTables.sql
-- Description: Creates tables for METRC compliance sync management
-- FRP: METRC Integration MVP

-- =====================================================
-- METRC LICENSES TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS metrc_licenses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    license_number VARCHAR(50) NOT NULL,
    state_code VARCHAR(2) NOT NULL,
    facility_name VARCHAR(200) NOT NULL,
    vendor_api_key_encrypted VARCHAR(500),
    user_api_key_encrypted VARCHAR(500),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    use_sandbox BOOLEAN NOT NULL DEFAULT FALSE,
    auto_sync_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    sync_interval_minutes INTEGER NOT NULL DEFAULT 15,
    last_sync_at TIMESTAMPTZ,
    last_successful_sync_at TIMESTAMPTZ,
    last_sync_error VARCHAR(2000),
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by_user_id UUID,
    updated_by_user_id UUID,
    CONSTRAINT uq_metrc_licenses_license_number UNIQUE (license_number),
    CONSTRAINT fk_metrc_licenses_site FOREIGN KEY (site_id) REFERENCES sites(id),
    CONSTRAINT ck_metrc_licenses_state_code CHECK (LENGTH(state_code) = 2)
);

CREATE INDEX IF NOT EXISTS idx_metrc_licenses_site_id ON metrc_licenses(site_id);
CREATE INDEX IF NOT EXISTS idx_metrc_licenses_active_auto_sync ON metrc_licenses(is_active, auto_sync_enabled) WHERE is_active = TRUE AND auto_sync_enabled = TRUE;

-- =====================================================
-- METRC SYNC JOBS TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS metrc_sync_jobs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    license_number VARCHAR(50) NOT NULL,
    state_code VARCHAR(2) NOT NULL,
    direction VARCHAR(20) NOT NULL,
    status VARCHAR(30) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    last_heartbeat_at TIMESTAMPTZ,
    total_items INTEGER NOT NULL DEFAULT 0,
    processed_items INTEGER NOT NULL DEFAULT 0,
    successful_items INTEGER NOT NULL DEFAULT 0,
    failed_items INTEGER NOT NULL DEFAULT 0,
    retry_count INTEGER NOT NULL DEFAULT 0,
    max_retries INTEGER NOT NULL DEFAULT 3,
    error_message VARCHAR(2000),
    error_details TEXT,
    initiated_by VARCHAR(50),
    initiated_by_user_id UUID,
    CONSTRAINT fk_metrc_sync_jobs_site FOREIGN KEY (site_id) REFERENCES sites(id)
);

CREATE INDEX IF NOT EXISTS idx_metrc_sync_jobs_license_number ON metrc_sync_jobs(license_number);
CREATE INDEX IF NOT EXISTS idx_metrc_sync_jobs_site_id ON metrc_sync_jobs(site_id);
CREATE INDEX IF NOT EXISTS idx_metrc_sync_jobs_status ON metrc_sync_jobs(status);
CREATE INDEX IF NOT EXISTS idx_metrc_sync_jobs_created_at ON metrc_sync_jobs(created_at DESC);

-- =====================================================
-- METRC QUEUE ITEMS TABLE (OUTBOX)
-- =====================================================
CREATE TABLE IF NOT EXISTS metrc_queue_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sync_job_id UUID NOT NULL,
    site_id UUID NOT NULL,
    license_number VARCHAR(50) NOT NULL,
    entity_type VARCHAR(30) NOT NULL,
    operation_type VARCHAR(30) NOT NULL,
    harvestry_entity_id UUID NOT NULL,
    metrc_id BIGINT,
    metrc_label VARCHAR(50),
    payload_json JSONB NOT NULL,
    status VARCHAR(30) NOT NULL,
    priority INTEGER NOT NULL DEFAULT 100,
    retry_count INTEGER NOT NULL DEFAULT 0,
    max_retries INTEGER NOT NULL DEFAULT 3,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    scheduled_at TIMESTAMPTZ,
    processed_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    error_message VARCHAR(2000),
    error_code VARCHAR(50),
    response_json JSONB,
    idempotency_key VARCHAR(200),
    depends_on_item_id UUID,
    CONSTRAINT fk_metrc_queue_items_sync_job FOREIGN KEY (sync_job_id) REFERENCES metrc_sync_jobs(id),
    CONSTRAINT fk_metrc_queue_items_site FOREIGN KEY (site_id) REFERENCES sites(id),
    CONSTRAINT fk_metrc_queue_items_depends_on FOREIGN KEY (depends_on_item_id) REFERENCES metrc_queue_items(id),
    CONSTRAINT uq_metrc_queue_items_idempotency UNIQUE (idempotency_key)
);

CREATE INDEX IF NOT EXISTS idx_metrc_queue_items_sync_job_id ON metrc_queue_items(sync_job_id);
CREATE INDEX IF NOT EXISTS idx_metrc_queue_items_license_number ON metrc_queue_items(license_number);
CREATE INDEX IF NOT EXISTS idx_metrc_queue_items_status ON metrc_queue_items(status);
CREATE INDEX IF NOT EXISTS idx_metrc_queue_items_processing ON metrc_queue_items(license_number, status, priority, scheduled_at)
    WHERE status IN ('Pending', 'Failed');
CREATE INDEX IF NOT EXISTS idx_metrc_queue_items_harvestry_entity ON metrc_queue_items(harvestry_entity_id);

-- =====================================================
-- METRC SYNC CHECKPOINTS TABLE
-- =====================================================
CREATE TABLE IF NOT EXISTS metrc_sync_checkpoints (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    license_id UUID NOT NULL,
    entity_type VARCHAR(30) NOT NULL,
    direction VARCHAR(20) NOT NULL,
    last_sync_timestamp TIMESTAMPTZ,
    last_synced_metrc_id BIGINT,
    last_sync_item_count INTEGER NOT NULL DEFAULT 0,
    last_successful_sync_at TIMESTAMPTZ,
    last_failed_sync_at TIMESTAMPTZ,
    last_error VARCHAR(2000),
    consecutive_failures INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fk_metrc_sync_checkpoints_license FOREIGN KEY (license_id) REFERENCES metrc_licenses(id),
    CONSTRAINT uq_metrc_sync_checkpoints_license_entity_direction UNIQUE (license_id, entity_type, direction)
);

CREATE INDEX IF NOT EXISTS idx_metrc_sync_checkpoints_license_id ON metrc_sync_checkpoints(license_id);

-- =====================================================
-- RLS POLICIES
-- =====================================================
ALTER TABLE metrc_licenses ENABLE ROW LEVEL SECURITY;
ALTER TABLE metrc_sync_jobs ENABLE ROW LEVEL SECURITY;
ALTER TABLE metrc_queue_items ENABLE ROW LEVEL SECURITY;
ALTER TABLE metrc_sync_checkpoints ENABLE ROW LEVEL SECURITY;

-- Licenses: Site-scoped access
CREATE POLICY metrc_licenses_site_isolation ON metrc_licenses
    FOR ALL
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID
           OR current_setting('app.current_user_id', TRUE) = 'service_account');

-- Sync Jobs: Site-scoped access
CREATE POLICY metrc_sync_jobs_site_isolation ON metrc_sync_jobs
    FOR ALL
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID
           OR current_setting('app.current_user_id', TRUE) = 'service_account');

-- Queue Items: Site-scoped access
CREATE POLICY metrc_queue_items_site_isolation ON metrc_queue_items
    FOR ALL
    USING (site_id = current_setting('app.current_site_id', TRUE)::UUID
           OR current_setting('app.current_user_id', TRUE) = 'service_account');

-- Checkpoints: Through license (site-scoped)
CREATE POLICY metrc_sync_checkpoints_site_isolation ON metrc_sync_checkpoints
    FOR ALL
    USING (license_id IN (
        SELECT id FROM metrc_licenses 
        WHERE site_id = current_setting('app.current_site_id', TRUE)::UUID
    ) OR current_setting('app.current_user_id', TRUE) = 'service_account');

-- =====================================================
-- TRIGGERS
-- =====================================================

-- Auto-update updated_at timestamp for licenses
CREATE OR REPLACE FUNCTION update_metrc_licenses_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_metrc_licenses_updated_at
    BEFORE UPDATE ON metrc_licenses
    FOR EACH ROW
    EXECUTE FUNCTION update_metrc_licenses_updated_at();

-- Auto-update updated_at timestamp for checkpoints
CREATE OR REPLACE FUNCTION update_metrc_sync_checkpoints_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_metrc_sync_checkpoints_updated_at
    BEFORE UPDATE ON metrc_sync_checkpoints
    FOR EACH ROW
    EXECUTE FUNCTION update_metrc_sync_checkpoints_updated_at();

-- =====================================================
-- COMMENTS
-- =====================================================
COMMENT ON TABLE metrc_licenses IS 'METRC license configurations per site';
COMMENT ON TABLE metrc_sync_jobs IS 'METRC synchronization job tracking';
COMMENT ON TABLE metrc_queue_items IS 'METRC outbox queue for sync operations';
COMMENT ON TABLE metrc_sync_checkpoints IS 'Per-entity-type sync progress checkpoints';

COMMENT ON COLUMN metrc_licenses.vendor_api_key_encrypted IS 'Encrypted METRC vendor API key';
COMMENT ON COLUMN metrc_licenses.user_api_key_encrypted IS 'Encrypted METRC user API key';
COMMENT ON COLUMN metrc_queue_items.idempotency_key IS 'Unique key to prevent duplicate operations';
COMMENT ON COLUMN metrc_queue_items.depends_on_item_id IS 'Reference to item that must complete first';
