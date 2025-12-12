-- Migration: Create Inventory Movements Table
-- Version: 003
-- Description: Creates movement tracking table for full audit trail of all inventory operations

-- ============================================================================
-- MOVEMENT TYPE ENUM
-- ============================================================================
DO $$ BEGIN
    CREATE TYPE movement_type AS ENUM (
        'transfer',       -- Move between locations
        'receive',        -- Receive from vendor/transfer
        'ship',           -- Ship to customer/transfer out
        'return',         -- Customer/vendor return
        'adjustment',     -- Quantity adjustment (up or down)
        'split',          -- Split package into multiple
        'merge',          -- Merge packages into one
        'process_input',  -- Consumed as production input
        'process_output', -- Created as production output
        'destruction',    -- Destroyed/disposed
        'cycle_count',    -- Cycle count adjustment
        'reserve',        -- Reserve for order
        'unreserve'       -- Release reservation
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- ============================================================================
-- MOVEMENT STATUS ENUM
-- ============================================================================
DO $$ BEGIN
    CREATE TYPE movement_status AS ENUM (
        'pending',        -- Awaiting execution
        'in_progress',    -- Currently being executed
        'completed',      -- Successfully completed
        'cancelled',      -- Cancelled before completion
        'failed',         -- Failed during execution
        'pending_approval' -- Awaiting approval
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- ============================================================================
-- ADJUSTMENT REASON ENUM (expanded)
-- ============================================================================
DO $$ BEGIN
    CREATE TYPE adjustment_reason_code AS ENUM (
        'damage',
        'theft',
        'spoilage',
        'expiration',
        'measurement_error',
        'scale_variance',
        'cycle_count',
        'quality_issue',
        'contamination',
        'regulatory_destruction',
        'sample',
        'drying',
        'moisture_loss',
        'processing_loss',
        'audit_adjustment',
        'entry_error',
        'other'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- ============================================================================
-- INVENTORY MOVEMENTS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS inventory_movements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    
    -- Movement classification
    movement_type movement_type NOT NULL,
    status movement_status NOT NULL DEFAULT 'completed',
    
    -- Package reference
    package_id UUID NOT NULL REFERENCES packages(id),
    package_label VARCHAR(30) NOT NULL,
    item_id UUID,
    item_name VARCHAR(200),
    
    -- Locations
    from_location_id UUID,
    from_location_path VARCHAR(500),
    to_location_id UUID,
    to_location_path VARCHAR(500),
    
    -- Quantity tracking
    quantity DECIMAL(12,4) NOT NULL,
    unit_of_measure VARCHAR(20) NOT NULL,
    quantity_before DECIMAL(12,4),
    quantity_after DECIMAL(12,4),
    
    -- Cost tracking (for COGS calculation)
    unit_cost DECIMAL(12,4),
    total_cost DECIMAL(12,4),
    
    -- Adjustment details
    reason_code VARCHAR(50),
    reason_notes TEXT,
    
    -- Split/Merge references
    source_package_ids UUID[],
    target_package_ids UUID[],
    
    -- Processing reference
    processing_job_id UUID,
    processing_job_number VARCHAR(50),
    
    -- Order reference (for ship/reserve)
    sales_order_id UUID,
    sales_order_number VARCHAR(50),
    transfer_id UUID,
    
    -- Compliance
    metrc_manifest_id VARCHAR(50),
    biotrack_transfer_id VARCHAR(50),
    sync_status VARCHAR(20) DEFAULT 'pending',
    sync_error TEXT,
    synced_at TIMESTAMPTZ,
    
    -- Verification
    verified_by_user_id UUID,
    verified_at TIMESTAMPTZ,
    scan_data TEXT,
    barcode_scanned VARCHAR(100),
    
    -- Evidence
    notes TEXT,
    evidence_urls JSONB DEFAULT '[]',
    photo_urls JSONB DEFAULT '[]',
    
    -- Two-person approval
    requires_approval BOOLEAN DEFAULT FALSE,
    first_approver_id UUID,
    first_approved_at TIMESTAMPTZ,
    second_approver_id UUID,
    second_approved_at TIMESTAMPTZ,
    rejection_reason TEXT,
    
    -- Batch reference (for batch movements)
    batch_movement_id UUID,
    batch_sequence INTEGER,
    
    -- Audit
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    completed_at TIMESTAMPTZ,
    completed_by_user_id UUID,
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    
    -- Metadata for extensibility
    metadata_json JSONB DEFAULT '{}'
);

-- ============================================================================
-- INDEXES
-- ============================================================================
CREATE INDEX IF NOT EXISTS idx_movements_site_id ON inventory_movements(site_id);
CREATE INDEX IF NOT EXISTS idx_movements_package_id ON inventory_movements(package_id);
CREATE INDEX IF NOT EXISTS idx_movements_type ON inventory_movements(movement_type);
CREATE INDEX IF NOT EXISTS idx_movements_status ON inventory_movements(status);
CREATE INDEX IF NOT EXISTS idx_movements_created_at ON inventory_movements(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_movements_from_location ON inventory_movements(from_location_id) WHERE from_location_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_movements_to_location ON inventory_movements(to_location_id) WHERE to_location_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_movements_processing_job ON inventory_movements(processing_job_id) WHERE processing_job_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_movements_sync_status ON inventory_movements(sync_status) WHERE sync_status != 'synced';
CREATE INDEX IF NOT EXISTS idx_movements_batch ON inventory_movements(batch_movement_id) WHERE batch_movement_id IS NOT NULL;

-- Composite index for recent movements by site
CREATE INDEX IF NOT EXISTS idx_movements_site_recent ON inventory_movements(site_id, created_at DESC);

-- Composite index for COGS calculation
CREATE INDEX IF NOT EXISTS idx_movements_cogs ON inventory_movements(site_id, movement_type, created_at, status)
    WHERE movement_type IN ('ship', 'process_input', 'destruction') AND status = 'completed';

-- ============================================================================
-- BATCH MOVEMENTS TABLE (for grouping related movements)
-- ============================================================================
CREATE TABLE IF NOT EXISTS batch_movements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    
    -- Batch details
    batch_type VARCHAR(30) NOT NULL, -- 'transfer', 'adjustment', 'receive', etc.
    description TEXT,
    
    -- Counts
    total_movements INTEGER DEFAULT 0,
    completed_movements INTEGER DEFAULT 0,
    failed_movements INTEGER DEFAULT 0,
    
    -- Status
    status movement_status NOT NULL DEFAULT 'pending',
    
    -- Audit
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    completed_at TIMESTAMPTZ,
    
    notes TEXT
);

CREATE INDEX IF NOT EXISTS idx_batch_movements_site ON batch_movements(site_id);
CREATE INDEX IF NOT EXISTS idx_batch_movements_status ON batch_movements(status);
CREATE INDEX IF NOT EXISTS idx_batch_movements_created ON batch_movements(created_at DESC);

-- ============================================================================
-- COMMENTS
-- ============================================================================
COMMENT ON TABLE inventory_movements IS 'Complete audit trail of all inventory movements and transactions';
COMMENT ON TABLE batch_movements IS 'Groups related movements for batch operations';

COMMENT ON COLUMN inventory_movements.movement_type IS 'Type: transfer, receive, ship, return, adjustment, split, merge, process_input, process_output, destruction, cycle_count';
COMMENT ON COLUMN inventory_movements.quantity_before IS 'Package quantity before this movement';
COMMENT ON COLUMN inventory_movements.quantity_after IS 'Package quantity after this movement';
COMMENT ON COLUMN inventory_movements.total_cost IS 'Total cost impact (quantity * unit_cost)';
COMMENT ON COLUMN inventory_movements.source_package_ids IS 'For merge: array of source package IDs';
COMMENT ON COLUMN inventory_movements.target_package_ids IS 'For split: array of created package IDs';
COMMENT ON COLUMN inventory_movements.batch_movement_id IS 'Reference to parent batch operation';




