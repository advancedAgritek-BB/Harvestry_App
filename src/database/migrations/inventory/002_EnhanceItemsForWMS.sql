-- Migration: Enhance Items Table for WMS
-- Version: 002
-- Description: Adds inventory classification, tracking flags, reorder management, and pricing fields

-- ============================================================================
-- INVENTORY CLASSIFICATION
-- ============================================================================
ALTER TABLE items ADD COLUMN IF NOT EXISTS inventory_category VARCHAR(30) DEFAULT 'finished_good';

COMMENT ON COLUMN items.inventory_category IS 'Default classification for packages: raw_material, work_in_progress, finished_good, consumable, byproduct';

-- ============================================================================
-- TRACKING FLAGS
-- ============================================================================
ALTER TABLE items ADD COLUMN IF NOT EXISTS is_lot_tracked BOOLEAN DEFAULT TRUE;
ALTER TABLE items ADD COLUMN IF NOT EXISTS is_serial_tracked BOOLEAN DEFAULT FALSE;

COMMENT ON COLUMN items.is_lot_tracked IS 'Whether this item requires lot/batch tracking';
COMMENT ON COLUMN items.is_serial_tracked IS 'Whether this item requires serial number tracking';

-- ============================================================================
-- REORDER MANAGEMENT
-- ============================================================================
ALTER TABLE items ADD COLUMN IF NOT EXISTS reorder_point DECIMAL(12,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS reorder_quantity DECIMAL(12,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS safety_stock DECIMAL(12,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS lead_time_days INTEGER;
ALTER TABLE items ADD COLUMN IF NOT EXISTS min_order_quantity DECIMAL(12,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS max_order_quantity DECIMAL(12,4);

COMMENT ON COLUMN items.reorder_point IS 'Quantity threshold that triggers reorder alert';
COMMENT ON COLUMN items.reorder_quantity IS 'Suggested quantity to reorder';
COMMENT ON COLUMN items.safety_stock IS 'Minimum stock level to maintain';
COMMENT ON COLUMN items.lead_time_days IS 'Expected days from order to delivery';
COMMENT ON COLUMN items.min_order_quantity IS 'Minimum quantity for purchase orders';
COMMENT ON COLUMN items.max_order_quantity IS 'Maximum quantity for purchase orders';

-- ============================================================================
-- PRICING
-- ============================================================================
ALTER TABLE items ADD COLUMN IF NOT EXISTS list_price DECIMAL(12,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS wholesale_price DECIMAL(12,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS cost_price DECIMAL(12,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS margin_percent DECIMAL(5,2);

COMMENT ON COLUMN items.list_price IS 'Standard retail/list price';
COMMENT ON COLUMN items.wholesale_price IS 'Wholesale/bulk price';
COMMENT ON COLUMN items.cost_price IS 'Standard cost for inventory valuation';
COMMENT ON COLUMN items.margin_percent IS 'Target margin percentage';

-- ============================================================================
-- PRODUCT FLAGS
-- ============================================================================
ALTER TABLE items ADD COLUMN IF NOT EXISTS is_sellable BOOLEAN DEFAULT TRUE;
ALTER TABLE items ADD COLUMN IF NOT EXISTS is_purchasable BOOLEAN DEFAULT FALSE;
ALTER TABLE items ADD COLUMN IF NOT EXISTS is_producible BOOLEAN DEFAULT FALSE;
ALTER TABLE items ADD COLUMN IF NOT EXISTS is_active_for_sale BOOLEAN DEFAULT TRUE;

COMMENT ON COLUMN items.is_sellable IS 'Can be sold to customers';
COMMENT ON COLUMN items.is_purchasable IS 'Can be purchased from vendors';
COMMENT ON COLUMN items.is_producible IS 'Can be produced/manufactured';
COMMENT ON COLUMN items.is_active_for_sale IS 'Currently available for sale';

-- ============================================================================
-- DEFAULT LOCATIONS
-- ============================================================================
ALTER TABLE items ADD COLUMN IF NOT EXISTS default_receiving_location_id UUID;
ALTER TABLE items ADD COLUMN IF NOT EXISTS default_storage_location_id UUID;
ALTER TABLE items ADD COLUMN IF NOT EXISTS default_production_location_id UUID;

COMMENT ON COLUMN items.default_receiving_location_id IS 'Default location for receiving this item';
COMMENT ON COLUMN items.default_storage_location_id IS 'Default storage location';
COMMENT ON COLUMN items.default_production_location_id IS 'Default location for production output';

-- ============================================================================
-- SHELF LIFE
-- ============================================================================
ALTER TABLE items ADD COLUMN IF NOT EXISTS shelf_life_days INTEGER;
ALTER TABLE items ADD COLUMN IF NOT EXISTS requires_expiration_date BOOLEAN DEFAULT FALSE;

COMMENT ON COLUMN items.shelf_life_days IS 'Default shelf life in days for new packages';
COMMENT ON COLUMN items.requires_expiration_date IS 'Whether packages must have expiration date';

-- ============================================================================
-- WEIGHT TRACKING
-- ============================================================================
ALTER TABLE items ADD COLUMN IF NOT EXISTS standard_weight DECIMAL(12,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS standard_weight_uom VARCHAR(20);
ALTER TABLE items ADD COLUMN IF NOT EXISTS weight_tolerance_percent DECIMAL(5,2) DEFAULT 5.0;

COMMENT ON COLUMN items.standard_weight IS 'Standard weight for count-based items';
COMMENT ON COLUMN items.standard_weight_uom IS 'Unit of measure for standard weight';
COMMENT ON COLUMN items.weight_tolerance_percent IS 'Acceptable variance percent for weight';

-- ============================================================================
-- INDEXES
-- ============================================================================
CREATE INDEX IF NOT EXISTS idx_items_inventory_category ON items(inventory_category);
CREATE INDEX IF NOT EXISTS idx_items_reorder ON items(site_id, reorder_point) WHERE reorder_point IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_items_sellable ON items(site_id, is_sellable) WHERE is_sellable = TRUE;
CREATE INDEX IF NOT EXISTS idx_items_purchasable ON items(site_id, is_purchasable) WHERE is_purchasable = TRUE;
CREATE INDEX IF NOT EXISTS idx_items_producible ON items(site_id, is_producible) WHERE is_producible = TRUE;

-- Composite index for product catalog queries
CREATE INDEX IF NOT EXISTS idx_items_catalog ON items(site_id, status, is_sellable, category);

COMMENT ON TABLE items IS 'Product/item definitions with WMS enhancements for reorder management, pricing, and tracking';




