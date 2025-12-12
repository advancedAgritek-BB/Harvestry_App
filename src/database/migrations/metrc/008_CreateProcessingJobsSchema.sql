-- Migration: Create processing jobs schema
-- Version: 008
-- Description: Creates processing job tables for METRC manufacturing tracking

-- Create processing job status enum
DO $$ BEGIN
    CREATE TYPE processing_job_status AS ENUM ('active', 'on_hold', 'finished', 'cancelled');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create processing_job_types table
CREATE TABLE IF NOT EXISTS processing_job_types (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    name VARCHAR(200) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    
    -- METRC mapping
    metrc_job_type_id BIGINT,
    metrc_job_type_name VARCHAR(200),
    
    -- Attributes
    attributes_json JSONB DEFAULT '{}',
    
    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL,
    
    CONSTRAINT uq_processing_job_types_name_site UNIQUE(site_id, name)
);

-- Create processing_jobs table
CREATE TABLE IF NOT EXISTS processing_jobs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    job_type_id UUID NOT NULL REFERENCES processing_job_types(id),
    job_type_name VARCHAR(200) NOT NULL,
    
    -- Dates
    start_date DATE NOT NULL,
    finish_date DATE,
    
    -- Status
    status processing_job_status NOT NULL DEFAULT 'active',
    notes TEXT,
    
    -- METRC sync
    metrc_processing_job_id BIGINT,
    metrc_last_sync_at TIMESTAMPTZ,
    metrc_sync_status TEXT,
    
    -- Metadata
    metadata_json JSONB DEFAULT '{}',
    
    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL
);

-- Create processing_job_inputs table
CREATE TABLE IF NOT EXISTS processing_job_inputs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    processing_job_id UUID NOT NULL REFERENCES processing_jobs(id) ON DELETE CASCADE,
    package_label VARCHAR(30) NOT NULL,
    quantity DECIMAL(12, 4) NOT NULL,
    unit_of_measure VARCHAR(20) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Create processing_job_outputs table
CREATE TABLE IF NOT EXISTS processing_job_outputs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    processing_job_id UUID NOT NULL REFERENCES processing_jobs(id) ON DELETE CASCADE,
    package_label VARCHAR(30) NOT NULL,
    item_id UUID NOT NULL,
    item_name VARCHAR(200) NOT NULL,
    quantity DECIMAL(12, 4) NOT NULL,
    unit_of_measure VARCHAR(20) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Indexes for processing_job_types
CREATE INDEX IF NOT EXISTS idx_processing_job_types_site_id ON processing_job_types(site_id);
CREATE INDEX IF NOT EXISTS idx_processing_job_types_is_active ON processing_job_types(is_active);

-- Indexes for processing_jobs
CREATE INDEX IF NOT EXISTS idx_processing_jobs_site_id ON processing_jobs(site_id);
CREATE INDEX IF NOT EXISTS idx_processing_jobs_job_type_id ON processing_jobs(job_type_id);
CREATE INDEX IF NOT EXISTS idx_processing_jobs_status ON processing_jobs(status);
CREATE INDEX IF NOT EXISTS idx_processing_jobs_start_date ON processing_jobs(start_date);
CREATE INDEX IF NOT EXISTS idx_processing_jobs_metrc_processing_job_id ON processing_jobs(metrc_processing_job_id) WHERE metrc_processing_job_id IS NOT NULL;

-- Indexes for processing_job_inputs
CREATE INDEX IF NOT EXISTS idx_processing_job_inputs_job_id ON processing_job_inputs(processing_job_id);
CREATE INDEX IF NOT EXISTS idx_processing_job_inputs_package_label ON processing_job_inputs(package_label);

-- Indexes for processing_job_outputs
CREATE INDEX IF NOT EXISTS idx_processing_job_outputs_job_id ON processing_job_outputs(processing_job_id);
CREATE INDEX IF NOT EXISTS idx_processing_job_outputs_package_label ON processing_job_outputs(package_label);

COMMENT ON TABLE processing_job_types IS 'Types of processing/manufacturing jobs';
COMMENT ON TABLE processing_jobs IS 'Processing/manufacturing job records for METRC';
COMMENT ON TABLE processing_job_inputs IS 'Input packages consumed by processing jobs';
COMMENT ON TABLE processing_job_outputs IS 'Output packages created by processing jobs';









