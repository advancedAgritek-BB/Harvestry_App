-- Migration: Create Growlink Integration Tables
-- Version: 001
-- Description: Creates tables for Growlink OAuth credentials and stream mappings

-- Growlink OAuth Credentials
CREATE TABLE IF NOT EXISTS growlink_credentials (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL,
    growlink_account_id VARCHAR(255) NOT NULL,
    access_token TEXT NOT NULL,
    refresh_token TEXT NOT NULL,
    token_expires_at TIMESTAMPTZ NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'NotConnected',
    last_sync_at TIMESTAMPTZ,
    last_sync_error TEXT,
    consecutive_failures INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_growlink_credentials_site UNIQUE (site_id),
    CONSTRAINT fk_growlink_credentials_site FOREIGN KEY (site_id) 
        REFERENCES sites(id) ON DELETE CASCADE
);

-- Indexes for efficient querying
CREATE INDEX IF NOT EXISTS ix_growlink_credentials_status 
    ON growlink_credentials(status);
CREATE INDEX IF NOT EXISTS ix_growlink_credentials_site_id 
    ON growlink_credentials(site_id);

-- Growlink to Harvestry Stream Mappings
CREATE TABLE IF NOT EXISTS growlink_stream_mappings (
    id UUID PRIMARY KEY,
    site_id UUID NOT NULL,
    growlink_device_id VARCHAR(255) NOT NULL,
    growlink_sensor_id VARCHAR(255) NOT NULL,
    growlink_sensor_name VARCHAR(255) NOT NULL,
    growlink_sensor_type VARCHAR(100) NOT NULL,
    harvestry_stream_id UUID NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    auto_created BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_growlink_mapping_sensor 
        UNIQUE (site_id, growlink_device_id, growlink_sensor_id),
    CONSTRAINT fk_growlink_mapping_site FOREIGN KEY (site_id) 
        REFERENCES sites(id) ON DELETE CASCADE
);

-- Indexes for efficient querying
CREATE INDEX IF NOT EXISTS ix_growlink_mappings_site_active 
    ON growlink_stream_mappings(site_id, is_active);
CREATE INDEX IF NOT EXISTS ix_growlink_mappings_stream 
    ON growlink_stream_mappings(harvestry_stream_id);

-- Enable RLS for multi-tenant security
ALTER TABLE growlink_credentials ENABLE ROW LEVEL SECURITY;
ALTER TABLE growlink_stream_mappings ENABLE ROW LEVEL SECURITY;

-- RLS Policies (site-scoped access)
CREATE POLICY growlink_credentials_site_policy ON growlink_credentials
    FOR ALL
    USING (site_id = current_setting('harvestry.site_id', true)::uuid);

CREATE POLICY growlink_mappings_site_policy ON growlink_stream_mappings
    FOR ALL
    USING (site_id = current_setting('harvestry.site_id', true)::uuid);

-- Comments for documentation
COMMENT ON TABLE growlink_credentials IS 'OAuth credentials for Growlink API integration per site';
COMMENT ON TABLE growlink_stream_mappings IS 'Maps Growlink sensors to Harvestry SensorStreams';
COMMENT ON COLUMN growlink_credentials.status IS 'Connection status: NotConnected, Pending, Connected, TokenExpired, Error, Disconnected';
COMMENT ON COLUMN growlink_stream_mappings.auto_created IS 'True if mapping was auto-created during sync';
