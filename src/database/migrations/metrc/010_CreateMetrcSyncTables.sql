-- Migration: Create METRC sync infrastructure tables
-- Version: 010
-- Description: Creates tables for METRC sync queue, events, and error tracking

-- Create sync direction enum
DO $$ BEGIN
    CREATE TYPE sync_direction AS ENUM ('outbound', 'inbound');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create sync status enum
DO $$ BEGIN
    CREATE TYPE sync_status AS ENUM ('pending', 'processing', 'completed', 'failed', 'dlq');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create metrc_sync_queue table (outbound sync requests)
CREATE TABLE IF NOT EXISTS metrc_sync_queue (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    
    -- Entity reference
    entity_type VARCHAR(50) NOT NULL,
    entity_id UUID NOT NULL,
    operation VARCHAR(20) NOT NULL, -- create, update, delete
    
    -- Request data
    payload_json JSONB NOT NULL,
    
    -- Queue management
    priority SMALLINT DEFAULT 0,
    status sync_status NOT NULL DEFAULT 'pending',
    attempt_count SMALLINT DEFAULT 0,
    max_attempts SMALLINT DEFAULT 5,
    next_attempt_at TIMESTAMPTZ,
    
    -- Processing info
    locked_at TIMESTAMPTZ,
    locked_by VARCHAR(100),
    processed_at TIMESTAMPTZ,
    
    -- Response data
    response_json JSONB,
    error_message TEXT,
    
    -- Timestamps
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Create metrc_sync_events table (audit log)
CREATE TABLE IF NOT EXISTS metrc_sync_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    sync_queue_id UUID REFERENCES metrc_sync_queue(id),
    
    -- Event details
    direction sync_direction NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    entity_id UUID,
    operation VARCHAR(20) NOT NULL,
    
    -- Request/Response
    request_json JSONB,
    response_json JSONB,
    
    -- Status
    success BOOLEAN NOT NULL,
    error_message TEXT,
    http_status_code SMALLINT,
    
    -- Timing
    started_at TIMESTAMPTZ NOT NULL,
    completed_at TIMESTAMPTZ NOT NULL,
    duration_ms INTEGER,
    
    -- Metadata
    correlation_id VARCHAR(100),
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Create metrc_sync_errors table (failed syncs with detailed tracking)
CREATE TABLE IF NOT EXISTS metrc_sync_errors (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    sync_queue_id UUID REFERENCES metrc_sync_queue(id),
    
    -- Error details
    entity_type VARCHAR(50) NOT NULL,
    entity_id UUID NOT NULL,
    operation VARCHAR(20) NOT NULL,
    
    -- Error information
    error_code VARCHAR(50),
    error_message TEXT NOT NULL,
    error_details JSONB,
    http_status_code SMALLINT,
    
    -- Retry tracking
    retry_count SMALLINT DEFAULT 0,
    last_retry_at TIMESTAMPTZ,
    next_retry_at TIMESTAMPTZ,
    is_resolved BOOLEAN DEFAULT FALSE,
    resolved_at TIMESTAMPTZ,
    resolved_by_user_id UUID,
    resolution_notes TEXT,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Create metrc_inbound_cache table (cached data from METRC pulls)
CREATE TABLE IF NOT EXISTS metrc_inbound_cache (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    
    -- Cache key
    cache_key VARCHAR(200) NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    
    -- Cached data
    data_json JSONB NOT NULL,
    
    -- Cache management
    fetched_at TIMESTAMPTZ NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    is_stale BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT uq_metrc_inbound_cache_key UNIQUE(site_id, cache_key)
);

-- Create metrc_tags table (tag management)
CREATE TABLE IF NOT EXISTS metrc_tags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    
    -- Tag details
    tag_number VARCHAR(30) NOT NULL,
    tag_type VARCHAR(20) NOT NULL, -- 'plant', 'package'
    
    -- Assignment
    assigned_to_entity_id UUID,
    assigned_to_entity_type VARCHAR(50),
    assigned_date DATE,
    
    -- Status
    status VARCHAR(20) NOT NULL DEFAULT 'available', -- 'available', 'assigned', 'retired'
    
    -- Order info
    order_number VARCHAR(50),
    received_date DATE,
    
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_metrc_tags_number_site UNIQUE(site_id, tag_number)
);

-- Indexes for metrc_sync_queue
CREATE INDEX IF NOT EXISTS idx_metrc_sync_queue_site_id ON metrc_sync_queue(site_id);
CREATE INDEX IF NOT EXISTS idx_metrc_sync_queue_status ON metrc_sync_queue(status);
CREATE INDEX IF NOT EXISTS idx_metrc_sync_queue_next_attempt ON metrc_sync_queue(next_attempt_at) WHERE status = 'pending';
CREATE INDEX IF NOT EXISTS idx_metrc_sync_queue_entity ON metrc_sync_queue(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_metrc_sync_queue_priority ON metrc_sync_queue(priority DESC, created_at ASC) WHERE status = 'pending';

-- Indexes for metrc_sync_events
CREATE INDEX IF NOT EXISTS idx_metrc_sync_events_site_id ON metrc_sync_events(site_id);
CREATE INDEX IF NOT EXISTS idx_metrc_sync_events_queue_id ON metrc_sync_events(sync_queue_id) WHERE sync_queue_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_metrc_sync_events_created_at ON metrc_sync_events(created_at);
CREATE INDEX IF NOT EXISTS idx_metrc_sync_events_entity ON metrc_sync_events(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_metrc_sync_events_success ON metrc_sync_events(success);

-- Indexes for metrc_sync_errors
CREATE INDEX IF NOT EXISTS idx_metrc_sync_errors_site_id ON metrc_sync_errors(site_id);
CREATE INDEX IF NOT EXISTS idx_metrc_sync_errors_unresolved ON metrc_sync_errors(is_resolved) WHERE is_resolved = FALSE;
CREATE INDEX IF NOT EXISTS idx_metrc_sync_errors_entity ON metrc_sync_errors(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_metrc_sync_errors_next_retry ON metrc_sync_errors(next_retry_at) WHERE is_resolved = FALSE;

-- Indexes for metrc_inbound_cache
CREATE INDEX IF NOT EXISTS idx_metrc_inbound_cache_site_id ON metrc_inbound_cache(site_id);
CREATE INDEX IF NOT EXISTS idx_metrc_inbound_cache_expires ON metrc_inbound_cache(expires_at);
CREATE INDEX IF NOT EXISTS idx_metrc_inbound_cache_entity_type ON metrc_inbound_cache(entity_type);

-- Indexes for metrc_tags
CREATE INDEX IF NOT EXISTS idx_metrc_tags_site_id ON metrc_tags(site_id);
CREATE INDEX IF NOT EXISTS idx_metrc_tags_status ON metrc_tags(status);
CREATE INDEX IF NOT EXISTS idx_metrc_tags_type_status ON metrc_tags(tag_type, status);
CREATE INDEX IF NOT EXISTS idx_metrc_tags_assigned ON metrc_tags(assigned_to_entity_type, assigned_to_entity_id) WHERE assigned_to_entity_id IS NOT NULL;

COMMENT ON TABLE metrc_sync_queue IS 'Queue for outbound METRC sync requests';
COMMENT ON TABLE metrc_sync_events IS 'Audit log of all METRC sync operations';
COMMENT ON TABLE metrc_sync_errors IS 'Detailed error tracking for failed METRC syncs';
COMMENT ON TABLE metrc_inbound_cache IS 'Cache for data pulled from METRC API';
COMMENT ON TABLE metrc_tags IS 'METRC tag inventory management';



