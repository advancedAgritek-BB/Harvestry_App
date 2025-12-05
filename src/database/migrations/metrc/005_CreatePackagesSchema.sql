-- Migration: Create packages schema
-- Version: 005
-- Description: Creates package tables for METRC package tracking

-- Create package status enum
DO $$ BEGIN
    CREATE TYPE package_status AS ENUM ('active', 'on_hold', 'in_transit', 'finished', 'inactive');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create lab testing state enum
DO $$ BEGIN
    CREATE TYPE lab_testing_state AS ENUM (
        'not_submitted', 'test_pending', 'test_passed', 'test_failed', 'not_required'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create package type enum
DO $$ BEGIN
    CREATE TYPE package_type AS ENUM ('product', 'immature_plant', 'vegetative_plant', 'seeds');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create adjustment reason enum
DO $$ BEGIN
    CREATE TYPE adjustment_reason AS ENUM (
        'drying', 'scale_variance', 'entry_error', 'moisture_loss', 'processing_loss',
        'theft', 'audit_adjustment', 'waste', 'contamination', 'other'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create packages table
CREATE TABLE IF NOT EXISTS packages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    package_label VARCHAR(30) NOT NULL,
    
    -- Item information
    item_id UUID NOT NULL,
    item_name VARCHAR(200) NOT NULL,
    item_category VARCHAR(100) NOT NULL,
    
    -- Quantity tracking
    quantity DECIMAL(12, 4) NOT NULL,
    initial_quantity DECIMAL(12, 4) NOT NULL,
    unit_of_measure VARCHAR(20) NOT NULL,
    
    -- Location
    location_id UUID,
    location_name VARCHAR(200),
    sublocation_name VARCHAR(100),
    
    -- Source tracking
    source_harvest_id UUID,
    source_harvest_name VARCHAR(200),
    source_package_labels JSONB DEFAULT '[]',
    
    -- Production batch
    production_batch_number VARCHAR(100),
    is_production_batch BOOLEAN DEFAULT FALSE,
    
    -- Special flags
    is_trade_sample BOOLEAN DEFAULT FALSE,
    is_donation BOOLEAN DEFAULT FALSE,
    product_requires_remediation BOOLEAN DEFAULT FALSE,
    
    -- Medical tracking
    patient_license_number VARCHAR(50),
    
    -- Dates
    packaged_date DATE NOT NULL,
    expiration_date DATE,
    use_by_date DATE,
    finished_date DATE,
    
    -- Lab testing
    lab_testing_state lab_testing_state NOT NULL DEFAULT 'not_submitted',
    lab_testing_state_required BOOLEAN DEFAULT FALSE,
    
    -- Potency
    thc_percent DECIMAL(6, 3),
    thc_content DECIMAL(10, 4),
    thc_content_uom VARCHAR(20),
    cbd_percent DECIMAL(6, 3),
    cbd_content DECIMAL(10, 4),
    
    -- Status
    status package_status NOT NULL DEFAULT 'active',
    package_type package_type NOT NULL DEFAULT 'product',
    notes TEXT,
    
    -- METRC sync
    metrc_package_id BIGINT,
    metrc_last_sync_at TIMESTAMPTZ,
    metrc_sync_status TEXT,
    
    -- Metadata
    metadata_json JSONB DEFAULT '{}',
    
    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL,
    
    CONSTRAINT uq_packages_label_site UNIQUE(site_id, package_label)
);

-- Create package_adjustments table
CREATE TABLE IF NOT EXISTS package_adjustments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    package_id UUID NOT NULL REFERENCES packages(id) ON DELETE CASCADE,
    quantity DECIMAL(12, 4) NOT NULL,
    unit_of_measure VARCHAR(20) NOT NULL,
    reason adjustment_reason NOT NULL,
    reason_note TEXT,
    adjustment_date DATE NOT NULL,
    performed_by_user_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    metrc_adjustment_id BIGINT
);

-- Create package_remediations table
CREATE TABLE IF NOT EXISTS package_remediations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    package_id UUID NOT NULL REFERENCES packages(id) ON DELETE CASCADE,
    remediation_method_name VARCHAR(200) NOT NULL,
    remediation_steps TEXT,
    remediation_date DATE NOT NULL,
    performed_by_user_id UUID NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    metrc_remediation_id BIGINT
);

-- Indexes for packages
CREATE INDEX IF NOT EXISTS idx_packages_site_id ON packages(site_id);
CREATE INDEX IF NOT EXISTS idx_packages_package_label ON packages(package_label);
CREATE INDEX IF NOT EXISTS idx_packages_item_id ON packages(item_id);
CREATE INDEX IF NOT EXISTS idx_packages_status ON packages(status);
CREATE INDEX IF NOT EXISTS idx_packages_lab_testing_state ON packages(lab_testing_state);
CREATE INDEX IF NOT EXISTS idx_packages_source_harvest_id ON packages(source_harvest_id) WHERE source_harvest_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_packages_metrc_package_id ON packages(metrc_package_id) WHERE metrc_package_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_packages_packaged_date ON packages(packaged_date);

-- Indexes for package_adjustments
CREATE INDEX IF NOT EXISTS idx_package_adjustments_package_id ON package_adjustments(package_id);
CREATE INDEX IF NOT EXISTS idx_package_adjustments_adjustment_date ON package_adjustments(adjustment_date);

-- Indexes for package_remediations
CREATE INDEX IF NOT EXISTS idx_package_remediations_package_id ON package_remediations(package_id);

COMMENT ON TABLE packages IS 'Cannabis packages for METRC tracking - core trackable units';
COMMENT ON TABLE package_adjustments IS 'Package quantity adjustments';
COMMENT ON TABLE package_remediations IS 'Package remediation records';



