-- Migration: Enhance Packages Table for WMS
-- Version: 001
-- Description: Adds costing, reservation, classification, hold management, vendor info, and quality fields

-- ============================================================================
-- COSTING FIELDS (Critical for financial metrics)
-- ============================================================================
ALTER TABLE packages ADD COLUMN IF NOT EXISTS unit_cost DECIMAL(12,4);
ALTER TABLE packages ADD COLUMN IF NOT EXISTS material_cost DECIMAL(12,4) DEFAULT 0;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS labor_cost DECIMAL(12,4) DEFAULT 0;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS overhead_cost DECIMAL(12,4) DEFAULT 0;

COMMENT ON COLUMN packages.unit_cost IS 'Cost per unit of measure';
COMMENT ON COLUMN packages.material_cost IS 'Raw material cost component';
COMMENT ON COLUMN packages.labor_cost IS 'Labor cost component';
COMMENT ON COLUMN packages.overhead_cost IS 'Overhead cost component';

-- ============================================================================
-- RESERVATION TRACKING
-- ============================================================================
ALTER TABLE packages ADD COLUMN IF NOT EXISTS reserved_quantity DECIMAL(12,4) DEFAULT 0;

COMMENT ON COLUMN packages.reserved_quantity IS 'Quantity reserved for orders but not yet consumed';

-- ============================================================================
-- INVENTORY CLASSIFICATION
-- ============================================================================
-- Create enum if not exists
DO $$ BEGIN
    CREATE TYPE inventory_category AS ENUM (
        'raw_material',
        'work_in_progress',
        'finished_good',
        'consumable',
        'byproduct'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

ALTER TABLE packages ADD COLUMN IF NOT EXISTS inventory_category VARCHAR(30) DEFAULT 'finished_good';

COMMENT ON COLUMN packages.inventory_category IS 'Classification: raw_material, work_in_progress, finished_good, consumable, byproduct';

-- ============================================================================
-- EXTENDED HOLD MANAGEMENT
-- ============================================================================
-- Create hold reason enum
DO $$ BEGIN
    CREATE TYPE hold_reason_code AS ENUM (
        'coa_failed',
        'coa_pending',
        'contamination',
        'quality_issue',
        'regulatory',
        'customer_return',
        'investigation',
        'audit_review',
        'damaged',
        'expired',
        'other'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

ALTER TABLE packages ADD COLUMN IF NOT EXISTS hold_reason_code VARCHAR(30);
ALTER TABLE packages ADD COLUMN IF NOT EXISTS hold_placed_at TIMESTAMPTZ;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS hold_placed_by_user_id UUID;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS hold_released_at TIMESTAMPTZ;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS hold_released_by_user_id UUID;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS requires_two_person_release BOOLEAN DEFAULT FALSE;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS hold_first_approver_id UUID;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS hold_first_approved_at TIMESTAMPTZ;

COMMENT ON COLUMN packages.hold_reason_code IS 'Reason code for holds: coa_failed, contamination, etc.';
COMMENT ON COLUMN packages.hold_placed_at IS 'Timestamp when hold was placed';
COMMENT ON COLUMN packages.hold_placed_by_user_id IS 'User who placed the hold';
COMMENT ON COLUMN packages.hold_released_at IS 'Timestamp when hold was released';
COMMENT ON COLUMN packages.hold_released_by_user_id IS 'User who released the hold';
COMMENT ON COLUMN packages.requires_two_person_release IS 'Whether release requires two-person approval';
COMMENT ON COLUMN packages.hold_first_approver_id IS 'First approver for two-person release';
COMMENT ON COLUMN packages.hold_first_approved_at IS 'Timestamp of first approval';

-- ============================================================================
-- VENDOR/RECEIVING INFO
-- ============================================================================
ALTER TABLE packages ADD COLUMN IF NOT EXISTS vendor_id UUID;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS vendor_name VARCHAR(200);
ALTER TABLE packages ADD COLUMN IF NOT EXISTS vendor_lot_number VARCHAR(100);
ALTER TABLE packages ADD COLUMN IF NOT EXISTS purchase_order_id UUID;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS purchase_order_number VARCHAR(50);
ALTER TABLE packages ADD COLUMN IF NOT EXISTS received_date DATE;

COMMENT ON COLUMN packages.vendor_id IS 'Reference to vendor for purchased inventory';
COMMENT ON COLUMN packages.vendor_name IS 'Denormalized vendor name for display';
COMMENT ON COLUMN packages.vendor_lot_number IS 'Vendor batch/lot number for traceability';
COMMENT ON COLUMN packages.purchase_order_id IS 'Reference to purchase order';
COMMENT ON COLUMN packages.purchase_order_number IS 'Denormalized PO number for display';
COMMENT ON COLUMN packages.received_date IS 'Date inventory was received';

-- ============================================================================
-- QUALITY GRADE
-- ============================================================================
-- Create grade enum
DO $$ BEGIN
    CREATE TYPE quality_grade AS ENUM (
        'premium',
        'a',
        'b',
        'c',
        'standard',
        'economy',
        'reject'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

ALTER TABLE packages ADD COLUMN IF NOT EXISTS grade VARCHAR(10);
ALTER TABLE packages ADD COLUMN IF NOT EXISTS quality_score DECIMAL(5,2);
ALTER TABLE packages ADD COLUMN IF NOT EXISTS quality_notes TEXT;

COMMENT ON COLUMN packages.grade IS 'Quality grade: premium, a, b, c, standard, economy, reject';
COMMENT ON COLUMN packages.quality_score IS 'Numeric quality score (0-100)';
COMMENT ON COLUMN packages.quality_notes IS 'Notes about quality assessment';

-- ============================================================================
-- LINEAGE ENHANCEMENT
-- ============================================================================
ALTER TABLE packages ADD COLUMN IF NOT EXISTS generation_depth INTEGER DEFAULT 0;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS root_ancestor_id UUID;
ALTER TABLE packages ADD COLUMN IF NOT EXISTS ancestry_path TEXT;

COMMENT ON COLUMN packages.generation_depth IS 'Number of transformations from origin (0 = original)';
COMMENT ON COLUMN packages.root_ancestor_id IS 'ID of the original ancestor package';
COMMENT ON COLUMN packages.ancestry_path IS 'Materialized path of ancestor IDs for fast queries';

-- ============================================================================
-- INDEXES
-- ============================================================================
CREATE INDEX IF NOT EXISTS idx_packages_inventory_category ON packages(inventory_category);
CREATE INDEX IF NOT EXISTS idx_packages_vendor_id ON packages(vendor_id) WHERE vendor_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_packages_hold_reason ON packages(hold_reason_code) WHERE hold_reason_code IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_packages_unit_cost ON packages(unit_cost) WHERE unit_cost IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_packages_grade ON packages(grade) WHERE grade IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_packages_received_date ON packages(received_date) WHERE received_date IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_packages_root_ancestor ON packages(root_ancestor_id) WHERE root_ancestor_id IS NOT NULL;

-- Composite index for financial queries
CREATE INDEX IF NOT EXISTS idx_packages_financial ON packages(site_id, inventory_category, status) 
    WHERE status = 'active';

-- Composite index for expiring inventory
CREATE INDEX IF NOT EXISTS idx_packages_expiring ON packages(site_id, expiration_date, status) 
    WHERE status = 'active' AND expiration_date IS NOT NULL;

COMMENT ON TABLE packages IS 'Cannabis packages (lots) for METRC tracking with WMS enhancements for costing, holds, and lineage';



