-- Migration: Create scale readings table
-- Version: 006
-- Description: Records individual scale readings with calibration snapshot

CREATE TABLE IF NOT EXISTS scale_readings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    
    -- What was weighed (at least one required)
    harvest_id UUID REFERENCES harvests(id) ON DELETE SET NULL,
    harvest_plant_id UUID REFERENCES harvest_plants(id) ON DELETE SET NULL,
    lot_id UUID,
    
    -- Scale info
    scale_device_id UUID REFERENCES scale_devices(id) ON DELETE SET NULL,
    
    -- Calibration snapshot at time of reading
    calibration_id UUID REFERENCES scale_calibrations(id) ON DELETE SET NULL,
    calibration_date DATE,
    calibration_due_date DATE,
    calibration_was_valid BOOLEAN DEFAULT FALSE,
    
    -- Weight data
    gross_weight DECIMAL(12, 4) NOT NULL,
    tare_weight DECIMAL(12, 4) DEFAULT 0,
    net_weight DECIMAL(12, 4) NOT NULL,
    unit_of_weight VARCHAR(20) NOT NULL DEFAULT 'Grams',
    
    -- Stability
    is_stable BOOLEAN DEFAULT TRUE,
    stability_duration_ms INT,
    
    -- Timestamp and raw data
    reading_timestamp TIMESTAMPTZ NOT NULL,
    raw_scale_data_json JSONB,
    
    -- Audit
    recorded_by_user_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    -- Constraint: at least one reference required
    CONSTRAINT chk_scale_reading_reference 
        CHECK (harvest_id IS NOT NULL OR harvest_plant_id IS NOT NULL OR lot_id IS NOT NULL)
);

-- Indexes
CREATE INDEX idx_scale_readings_harvest ON scale_readings(harvest_id) WHERE harvest_id IS NOT NULL;
CREATE INDEX idx_scale_readings_plant ON scale_readings(harvest_plant_id) WHERE harvest_plant_id IS NOT NULL;
CREATE INDEX idx_scale_readings_lot ON scale_readings(lot_id) WHERE lot_id IS NOT NULL;
CREATE INDEX idx_scale_readings_device ON scale_readings(scale_device_id) WHERE scale_device_id IS NOT NULL;
CREATE INDEX idx_scale_readings_timestamp ON scale_readings(reading_timestamp);
CREATE INDEX idx_scale_readings_calibration ON scale_readings(calibration_id) WHERE calibration_id IS NOT NULL;

-- Index for finding readings with invalid calibration
CREATE INDEX idx_scale_readings_invalid_cal 
    ON scale_readings(calibration_was_valid) 
    WHERE calibration_was_valid = FALSE;

-- Comments
COMMENT ON TABLE scale_readings IS 'Individual scale readings with full audit trail';
COMMENT ON COLUMN scale_readings.calibration_was_valid IS 'Whether scale was within valid calibration at time of reading';
COMMENT ON COLUMN scale_readings.is_stable IS 'Whether scale reported stable weight';
COMMENT ON COLUMN scale_readings.stability_duration_ms IS 'How long weight was stable before capture (milliseconds)';
COMMENT ON COLUMN scale_readings.raw_scale_data_json IS 'Raw response from scale in JSON format';




