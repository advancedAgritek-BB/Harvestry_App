-- Migration: Create scale calibrations table
-- Version: 005
-- Description: Tracks calibration events for scale devices

CREATE TABLE IF NOT EXISTS scale_calibrations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    scale_device_id UUID NOT NULL REFERENCES scale_devices(id) ON DELETE CASCADE,
    
    -- Calibration dates
    calibration_date DATE NOT NULL,
    calibration_due_date DATE NOT NULL,
    calibration_type VARCHAR(30) NOT NULL,
    
    -- Who performed/certified
    performed_by VARCHAR(200),
    certified_by VARCHAR(200),
    certification_number VARCHAR(100),
    calibration_company VARCHAR(200),
    
    -- Test weights used (JSON array)
    -- Format: [{nominal: 1000, actual: 999.98, measured: 999.97}]
    test_weights_used_json JSONB,
    
    -- Results
    passed BOOLEAN NOT NULL,
    deviation_grams DECIMAL(8, 4),
    deviation_percent DECIMAL(5, 4),
    notes TEXT,
    
    -- Certificate documentation
    certificate_url VARCHAR(500),
    certificate_document_id UUID,
    
    -- Audit
    recorded_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    recorded_by_user_id UUID NOT NULL
);

-- Indexes
CREATE INDEX idx_calibrations_device ON scale_calibrations(scale_device_id);
CREATE INDEX idx_calibrations_date ON scale_calibrations(calibration_date);
CREATE INDEX idx_calibrations_due ON scale_calibrations(calibration_due_date);
CREATE INDEX idx_calibrations_passed ON scale_calibrations(passed);

-- Partial index for finding current valid calibrations
CREATE INDEX idx_calibrations_valid 
    ON scale_calibrations(scale_device_id, calibration_due_date DESC) 
    WHERE passed = TRUE;

-- Comments
COMMENT ON TABLE scale_calibrations IS 'Calibration records for scale devices';
COMMENT ON COLUMN scale_calibrations.calibration_type IS 'Type: internal, external, certified';
COMMENT ON COLUMN scale_calibrations.test_weights_used_json IS 'JSON array of test weights: [{nominal, actual, measured}]';
COMMENT ON COLUMN scale_calibrations.deviation_grams IS 'Maximum deviation from standard in grams';
COMMENT ON COLUMN scale_calibrations.deviation_percent IS 'Maximum deviation as percentage';
