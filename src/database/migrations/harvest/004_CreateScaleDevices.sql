-- Migration: Create scale devices table
-- Version: 004
-- Description: Tracks scale devices used for weighing operations

CREATE TABLE IF NOT EXISTS scale_devices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    
    -- Identification
    device_name VARCHAR(100) NOT NULL,
    device_serial_number VARCHAR(100),
    
    -- Manufacturer/Model
    manufacturer VARCHAR(100),
    model VARCHAR(100),
    
    -- Specifications
    capacity_grams DECIMAL(12, 4),
    readability_grams DECIMAL(8, 4),
    
    -- Connection
    connection_type VARCHAR(20) NOT NULL DEFAULT 'usb',
    connection_config_json JSONB,
    
    -- Location
    location_id UUID,
    location_name VARCHAR(200),
    
    -- Status
    is_active BOOLEAN DEFAULT TRUE,
    
    -- Calibration settings
    requires_calibration BOOLEAN DEFAULT TRUE,
    calibration_interval_days INT DEFAULT 365,
    
    -- Audit
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Indexes
CREATE INDEX idx_scale_devices_site ON scale_devices(site_id);
CREATE INDEX idx_scale_devices_active ON scale_devices(is_active) WHERE is_active = TRUE;
CREATE INDEX idx_scale_devices_location ON scale_devices(location_id) WHERE location_id IS NOT NULL;
CREATE INDEX idx_scale_devices_serial ON scale_devices(device_serial_number) WHERE device_serial_number IS NOT NULL;

-- Comments
COMMENT ON TABLE scale_devices IS 'Scale devices used for weighing in harvest and inventory operations';
COMMENT ON COLUMN scale_devices.connection_type IS 'Connection type: usb, serial, network, bluetooth';
COMMENT ON COLUMN scale_devices.connection_config_json IS 'JSON configuration: port, IP address, baud rate, etc.';
COMMENT ON COLUMN scale_devices.capacity_grams IS 'Maximum weight capacity in grams';
COMMENT ON COLUMN scale_devices.readability_grams IS 'Smallest measurable increment (e.g., 0.1g)';





