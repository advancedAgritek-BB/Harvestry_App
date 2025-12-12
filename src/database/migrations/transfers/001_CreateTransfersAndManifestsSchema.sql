-- Migration: Create Transfers + Transport Manifests tables (METRC-oriented)
-- Version: 001
-- Description: Adds outbound transfers, transport manifests, inbound receipts, and return/void tracking.
--
-- Notes:
-- - Uses site_id for RLS scoping; avoids hard FK to sites due to mixed schema conventions elsewhere.
-- - Outbound transfer is the compliance object; shipments remain operational.

-- ============================================================================
-- ENUMS
-- ============================================================================
DO $$ BEGIN
    CREATE TYPE outbound_transfer_status AS ENUM (
        'draft',
        'ready',
        'submitted_to_metrc',
        'in_transit',
        'delivered',
        'accepted',
        'rejected',
        'voided',
        'cancelled'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE manifest_status AS ENUM (
        'draft',
        'ready',
        'submitted_to_metrc',
        'voided'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE inbound_receipt_status AS ENUM (
        'draft',
        'accepted',
        'rejected',
        'partial'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- ============================================================================
-- OUTBOUND TRANSFERS
-- ============================================================================
CREATE TABLE IF NOT EXISTS outbound_transfers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,

    -- Links
    shipment_id UUID, -- shipment is optional for imported transfers
    sales_order_id UUID,

    -- Destination
    destination_license_number VARCHAR(100) NOT NULL,
    destination_facility_name VARCHAR(200),

    -- Timing
    planned_departure_at TIMESTAMPTZ,
    planned_arrival_at TIMESTAMPTZ,

    -- Status
    status outbound_transfer_status NOT NULL DEFAULT 'draft',
    status_reason TEXT,

    -- METRC identifiers (for transfer templates)
    metrc_transfer_template_id BIGINT,
    metrc_transfer_number VARCHAR(50),
    metrc_last_submitted_at TIMESTAMPTZ,
    metrc_sync_status TEXT,
    metrc_sync_error TEXT,

    -- Audit
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_outbound_transfers_site_status ON outbound_transfers(site_id, status);
CREATE INDEX IF NOT EXISTS idx_outbound_transfers_metrc_template ON outbound_transfers(metrc_transfer_template_id) WHERE metrc_transfer_template_id IS NOT NULL;

-- Packages included in the transfer (package granularity)
CREATE TABLE IF NOT EXISTS outbound_transfer_packages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    outbound_transfer_id UUID NOT NULL REFERENCES outbound_transfers(id) ON DELETE CASCADE,

    package_id UUID NOT NULL,
    package_label VARCHAR(30),
    quantity DECIMAL(12,4) NOT NULL,
    unit_of_measure VARCHAR(20) NOT NULL,

    CONSTRAINT ck_outbound_transfer_packages_qty_positive CHECK (quantity > 0)
);

CREATE INDEX IF NOT EXISTS idx_outbound_transfer_packages_transfer ON outbound_transfer_packages(outbound_transfer_id);
CREATE INDEX IF NOT EXISTS idx_outbound_transfer_packages_package ON outbound_transfer_packages(package_id);

-- ============================================================================
-- TRANSPORT MANIFEST (driver/vehicle/shipping details)
-- ============================================================================
CREATE TABLE IF NOT EXISTS transport_manifests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    outbound_transfer_id UUID NOT NULL REFERENCES outbound_transfers(id) ON DELETE CASCADE,

    status manifest_status NOT NULL DEFAULT 'draft',

    transporter_name VARCHAR(200),
    transporter_license_number VARCHAR(100),

    driver_name VARCHAR(200),
    driver_license_number VARCHAR(100),
    driver_phone VARCHAR(30),

    vehicle_make VARCHAR(100),
    vehicle_model VARCHAR(100),
    vehicle_plate VARCHAR(30),

    departure_at TIMESTAMPTZ,
    arrival_at TIMESTAMPTZ,

    -- METRC identifiers (if used)
    metrc_manifest_number VARCHAR(50),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_transport_manifests_site_status ON transport_manifests(site_id, status);
CREATE INDEX IF NOT EXISTS idx_transport_manifests_transfer ON transport_manifests(outbound_transfer_id);

-- ============================================================================
-- INBOUND RECEIPTS (accept/reject/partial)
-- ============================================================================
CREATE TABLE IF NOT EXISTS inbound_transfer_receipts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,

    outbound_transfer_id UUID REFERENCES outbound_transfers(id) ON DELETE SET NULL,
    metrc_transfer_id BIGINT,
    metrc_transfer_number VARCHAR(50),

    status inbound_receipt_status NOT NULL DEFAULT 'draft',
    received_at TIMESTAMPTZ,
    received_by_user_id UUID,
    notes TEXT,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_inbound_receipts_site_status ON inbound_transfer_receipts(site_id, status);
CREATE INDEX IF NOT EXISTS idx_inbound_receipts_metrc ON inbound_transfer_receipts(metrc_transfer_id) WHERE metrc_transfer_id IS NOT NULL;

CREATE TABLE IF NOT EXISTS inbound_transfer_receipt_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    inbound_receipt_id UUID NOT NULL REFERENCES inbound_transfer_receipts(id) ON DELETE CASCADE,

    package_label VARCHAR(30) NOT NULL,
    received_quantity DECIMAL(12,4) NOT NULL,
    unit_of_measure VARCHAR(20) NOT NULL,

    accepted BOOLEAN NOT NULL DEFAULT TRUE,
    rejection_reason TEXT,

    CONSTRAINT ck_inbound_transfer_receipt_lines_qty_nonnegative CHECK (received_quantity >= 0)
);

CREATE INDEX IF NOT EXISTS idx_inbound_receipt_lines_receipt ON inbound_transfer_receipt_lines(inbound_receipt_id);

-- ============================================================================
-- TRANSFER EVENTS (audit trail for void/cancel/return)
-- ============================================================================
CREATE TABLE IF NOT EXISTS transfer_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL,
    outbound_transfer_id UUID NOT NULL REFERENCES outbound_transfers(id) ON DELETE CASCADE,

    event_type VARCHAR(50) NOT NULL, -- created, submitted, voided, cancelled, return_created, etc.
    event_reason TEXT,
    metadata JSONB DEFAULT '{}'::jsonb,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_transfer_events_transfer ON transfer_events(outbound_transfer_id, created_at DESC);

