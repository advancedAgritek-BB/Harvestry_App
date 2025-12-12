-- Migration: Create Sales Orders + Fulfillment core tables
-- Version: 001
-- Description: Adds sales orders, lines, allocations, and shipments (package-level fulfillment)
--
-- Notes:
-- - Uses site_id for RLS scoping; avoids hard FK to sites due to mixed schema conventions elsewhere.
-- - Packages remain the inventory source-of-truth; allocations reference package_id.
-- - Quantities are DECIMAL(12,4) to align with packages/movements.
--
-- ============================================================================
-- ENUMS
-- ============================================================================
DO $$ BEGIN
    CREATE TYPE sales_order_status AS ENUM (
        'draft',
        'submitted',
        'allocated',
        'partially_shipped',
        'shipped',
        'cancelled'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE shipment_status AS ENUM (
        'draft',
        'picking',
        'packed',
        'shipped',
        'cancelled'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- ============================================================================
-- SALES ORDERS
-- ============================================================================
CREATE TABLE IF NOT EXISTS sales_orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    order_number VARCHAR(40) NOT NULL,

    -- Customer / destination
    customer_id UUID,
    customer_name VARCHAR(200) NOT NULL,
    destination_license_number VARCHAR(100),
    destination_facility_name VARCHAR(200),

    -- Dates
    requested_ship_date DATE,
    submitted_at TIMESTAMPTZ,
    cancelled_at TIMESTAMPTZ,

    status sales_order_status NOT NULL DEFAULT 'draft',
    notes TEXT,

    -- Audit
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL,

    CONSTRAINT uq_sales_orders_site_order_number UNIQUE (site_id, order_number)
);

CREATE INDEX IF NOT EXISTS idx_sales_orders_site_status ON sales_orders(site_id, status);
CREATE INDEX IF NOT EXISTS idx_sales_orders_site_created ON sales_orders(site_id, created_at DESC);

-- ============================================================================
-- SALES ORDER LINES (item-level demand)
-- ============================================================================
CREATE TABLE IF NOT EXISTS sales_order_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sales_order_id UUID NOT NULL REFERENCES sales_orders(id) ON DELETE CASCADE,
    site_id UUID NOT NULL,

    line_number INT NOT NULL,
    item_id UUID NOT NULL,
    item_name VARCHAR(200) NOT NULL,
    unit_of_measure VARCHAR(20) NOT NULL,

    requested_quantity DECIMAL(12,4) NOT NULL,
    allocated_quantity DECIMAL(12,4) NOT NULL DEFAULT 0,
    shipped_quantity DECIMAL(12,4) NOT NULL DEFAULT 0,

    -- Pricing (optional for now)
    unit_price DECIMAL(12,4),
    currency_code VARCHAR(3) DEFAULT 'USD',

    CONSTRAINT uq_sales_order_lines_order_line UNIQUE (sales_order_id, line_number)
);

CREATE INDEX IF NOT EXISTS idx_sales_order_lines_site_item ON sales_order_lines(site_id, item_id);
CREATE INDEX IF NOT EXISTS idx_sales_order_lines_order ON sales_order_lines(sales_order_id);

-- ============================================================================
-- ALLOCATIONS (package-level reservation source of truth)
-- ============================================================================
CREATE TABLE IF NOT EXISTS sales_allocations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    sales_order_id UUID NOT NULL REFERENCES sales_orders(id) ON DELETE CASCADE,
    sales_order_line_id UUID NOT NULL REFERENCES sales_order_lines(id) ON DELETE CASCADE,

    package_id UUID NOT NULL,
    package_label VARCHAR(30),
    allocated_quantity DECIMAL(12,4) NOT NULL,
    unit_of_measure VARCHAR(20) NOT NULL,

    -- Lifecycle
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    cancelled_at TIMESTAMPTZ,
    cancelled_by_user_id UUID,
    cancel_reason TEXT,

    CONSTRAINT ck_sales_allocations_qty_positive CHECK (allocated_quantity > 0)
);

CREATE INDEX IF NOT EXISTS idx_sales_allocations_site_order ON sales_allocations(site_id, sales_order_id);
CREATE INDEX IF NOT EXISTS idx_sales_allocations_package ON sales_allocations(package_id);

-- ============================================================================
-- SHIPMENTS (operational execution: pack + ship)
-- ============================================================================
CREATE TABLE IF NOT EXISTS shipments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    shipment_number VARCHAR(40) NOT NULL,
    sales_order_id UUID NOT NULL REFERENCES sales_orders(id) ON DELETE RESTRICT,

    status shipment_status NOT NULL DEFAULT 'draft',
    picking_started_at TIMESTAMPTZ,
    packed_at TIMESTAMPTZ,
    shipped_at TIMESTAMPTZ,
    cancelled_at TIMESTAMPTZ,

    carrier_name VARCHAR(200),
    tracking_number VARCHAR(100),
    notes TEXT,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL,

    CONSTRAINT uq_shipments_site_shipment_number UNIQUE (site_id, shipment_number)
);

CREATE INDEX IF NOT EXISTS idx_shipments_site_status ON shipments(site_id, status);
CREATE INDEX IF NOT EXISTS idx_shipments_order ON shipments(sales_order_id);

-- Shipment lines at package granularity (what is packed/shipped)
CREATE TABLE IF NOT EXISTS shipment_packages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    shipment_id UUID NOT NULL REFERENCES shipments(id) ON DELETE CASCADE,
    sales_allocation_id UUID REFERENCES sales_allocations(id) ON DELETE SET NULL,

    package_id UUID NOT NULL,
    package_label VARCHAR(30),
    quantity DECIMAL(12,4) NOT NULL,
    unit_of_measure VARCHAR(20) NOT NULL,

    packed_at TIMESTAMPTZ,
    packed_by_user_id UUID,

    CONSTRAINT ck_shipment_packages_qty_positive CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS idx_shipment_packages_shipment ON shipment_packages(shipment_id);
CREATE INDEX IF NOT EXISTS idx_shipment_packages_package ON shipment_packages(package_id);

