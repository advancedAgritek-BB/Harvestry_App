-- Migration: Create lab tests schema
-- Version: 007
-- Description: Creates lab test tables for METRC lab test tracking

-- Create lab test status enum
DO $$ BEGIN
    CREATE TYPE lab_test_status AS ENUM ('pending', 'passed', 'failed', 'requires_remediation', 'voided');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create lab test type enum
DO $$ BEGIN
    CREATE TYPE lab_test_type AS ENUM (
        'potency', 'thc', 'cbd', 'cannabinoids',
        'terpenes',
        'microbial', 'mycotoxins', 'pesticides', 'heavy_metals', 'residual_solvents', 'foreign_material', 'water_activity', 'moisture_content',
        'homogeneity', 'other'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create lab_test_batches table
CREATE TABLE IF NOT EXISTS lab_test_batches (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    package_label VARCHAR(30) NOT NULL,
    
    -- Lab facility
    lab_facility_license_number VARCHAR(50) NOT NULL,
    lab_facility_name VARCHAR(200) NOT NULL,
    
    -- Dates
    collected_date DATE,
    received_date DATE,
    test_completed_date DATE,
    
    -- Status
    status lab_test_status NOT NULL DEFAULT 'pending',
    notes TEXT,
    
    -- METRC sync
    metrc_lab_test_id BIGINT,
    metrc_last_sync_at TIMESTAMPTZ,
    metrc_sync_status TEXT,
    
    -- Document
    document_url TEXT,
    document_file_name VARCHAR(255),
    
    -- Metadata
    metadata_json JSONB DEFAULT '{}',
    
    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL
);

-- Create lab_test_results table
CREATE TABLE IF NOT EXISTS lab_test_results (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    lab_test_batch_id UUID NOT NULL REFERENCES lab_test_batches(id) ON DELETE CASCADE,
    test_type lab_test_type NOT NULL,
    test_type_name VARCHAR(100) NOT NULL,
    analyte_name VARCHAR(100),
    result_value DECIMAL(12, 6),
    result_unit VARCHAR(20),
    limit_value DECIMAL(12, 6),
    passed BOOLEAN NOT NULL,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Indexes for lab_test_batches
CREATE INDEX IF NOT EXISTS idx_lab_test_batches_site_id ON lab_test_batches(site_id);
CREATE INDEX IF NOT EXISTS idx_lab_test_batches_package_label ON lab_test_batches(package_label);
CREATE INDEX IF NOT EXISTS idx_lab_test_batches_status ON lab_test_batches(status);
CREATE INDEX IF NOT EXISTS idx_lab_test_batches_lab_facility ON lab_test_batches(lab_facility_license_number);
CREATE INDEX IF NOT EXISTS idx_lab_test_batches_metrc_lab_test_id ON lab_test_batches(metrc_lab_test_id) WHERE metrc_lab_test_id IS NOT NULL;

-- Indexes for lab_test_results
CREATE INDEX IF NOT EXISTS idx_lab_test_results_batch_id ON lab_test_results(lab_test_batch_id);
CREATE INDEX IF NOT EXISTS idx_lab_test_results_test_type ON lab_test_results(test_type);
CREATE INDEX IF NOT EXISTS idx_lab_test_results_passed ON lab_test_results(passed);

COMMENT ON TABLE lab_test_batches IS 'Lab test batch records for METRC';
COMMENT ON TABLE lab_test_results IS 'Individual test results within a batch';








