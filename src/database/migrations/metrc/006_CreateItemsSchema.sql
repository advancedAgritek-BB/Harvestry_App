-- Migration: Create items schema
-- Version: 006
-- Description: Creates item/product definition tables for METRC

-- Create item category enum (subset of METRC categories)
DO $$ BEGIN
    CREATE TYPE item_category AS ENUM (
        'buds', 'shake', 'trim', 'flower', 'flower_lot',
        'concentrate', 'concentrate_for_infusion', 'wax', 'shatter', 'resin', 'rosin', 'oil', 'distillate', 'kief', 'hash',
        'infused_edible', 'infused_non_edible', 'infused_pre_roll', 'infused_beverage', 'infused_topical', 'infused_tincture', 'infused_capsule', 'infused_suppository', 'infused_transdermal_patch',
        'pre_roll', 'pre_roll_infused', 'pre_roll_flower',
        'vaporizer_cartridge', 'vaporizer_pen',
        'immature_plant', 'clone', 'seeds', 'tissue', 'mature_plant',
        'sample', 'waste_product', 'other'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create item status enum
DO $$ BEGIN
    CREATE TYPE item_status AS ENUM ('active', 'inactive');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create unit of measure enum
DO $$ BEGIN
    CREATE TYPE unit_of_measure AS ENUM (
        'grams', 'milligrams', 'kilograms', 'ounces', 'pounds',
        'each',
        'milliliters', 'liters', 'fluid_ounces',
        'other'
    );
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Create items table
CREATE TABLE IF NOT EXISTS items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    name VARCHAR(200) NOT NULL,
    category item_category NOT NULL,
    unit_of_measure unit_of_measure NOT NULL,
    status item_status NOT NULL DEFAULT 'active',
    
    -- Strain association
    strain_id UUID,
    strain_name VARCHAR(200),
    
    -- Unit weight for count-based items
    unit_weight DECIMAL(12, 4),
    unit_weight_uom VARCHAR(20),
    
    -- Default potency values
    default_thc_percent DECIMAL(6, 3),
    default_thc_content DECIMAL(10, 4),
    default_thc_content_uom VARCHAR(20),
    default_cbd_percent DECIMAL(6, 3),
    default_cbd_content DECIMAL(10, 4),
    
    -- Lab testing
    requires_lab_testing BOOLEAN DEFAULT FALSE,
    default_lab_testing_state VARCHAR(50),
    
    -- Additional properties
    description TEXT,
    sku VARCHAR(50),
    barcode VARCHAR(100),
    
    -- METRC sync
    metrc_item_id BIGINT,
    metrc_last_sync_at TIMESTAMPTZ,
    metrc_sync_status TEXT,
    
    -- Metadata
    metadata_json JSONB DEFAULT '{}',
    
    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_by_user_id UUID NOT NULL,
    
    CONSTRAINT uq_items_name_site UNIQUE(site_id, name)
);

-- Indexes for items
CREATE INDEX IF NOT EXISTS idx_items_site_id ON items(site_id);
CREATE INDEX IF NOT EXISTS idx_items_category ON items(category);
CREATE INDEX IF NOT EXISTS idx_items_status ON items(status);
CREATE INDEX IF NOT EXISTS idx_items_strain_id ON items(strain_id) WHERE strain_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_items_metrc_item_id ON items(metrc_item_id) WHERE metrc_item_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_items_sku ON items(sku) WHERE sku IS NOT NULL;

COMMENT ON TABLE items IS 'Product/item definitions for METRC compliance';



