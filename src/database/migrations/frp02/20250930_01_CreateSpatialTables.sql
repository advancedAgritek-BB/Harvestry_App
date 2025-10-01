-- =====================================================================
-- FRP-02: Spatial Model Tables
-- File: 20250930_01_CreateSpatialTables.sql
-- Description: Site → Room → Zone → SubZone/Bench → Row hierarchy
--              with flexible inventory_locations for both cultivation
--              and warehouse management
-- =====================================================================

-- =====================================================================
-- 1) Room Types (Core + Custom)
-- =====================================================================

-- Core room types enum
CREATE TYPE room_type AS ENUM (
    'Veg',
    'Flower',
    'Mother',
    'Clone',
    'Dry',
    'Cure',
    'Extraction',
    'Manufacturing',
    'Vault',
    'Custom'  -- Allows user-defined types
);

-- Room status enum
CREATE TYPE room_status AS ENUM (
    'Active',
    'Inactive',
    'Maintenance',
    'Quarantine'
);

-- =====================================================================
-- 2) Rooms Table
-- =====================================================================

CREATE TABLE rooms (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    room_type room_type NOT NULL,
    custom_room_type VARCHAR(100),  -- Used when room_type = 'Custom'
    status room_status NOT NULL DEFAULT 'Active',
    description TEXT,
    floor_level INTEGER,  -- Building floor (optional)
    area_sqft DECIMAL(10,2),  -- Floor area
    height_ft DECIMAL(6,2),  -- Ceiling height
    
    -- Environment metadata (optional)
    target_temp_f DECIMAL(5,2),
    target_humidity_pct DECIMAL(5,2),
    target_co2_ppm INTEGER,
    
    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    
    CONSTRAINT rooms_code_site_unique UNIQUE (site_id, code),
    CONSTRAINT rooms_custom_type_check CHECK (
        (room_type = 'Custom' AND custom_room_type IS NOT NULL) OR
        (room_type <> 'Custom' AND custom_room_type IS NULL)
    )
);

CREATE INDEX idx_rooms_site_id ON rooms(site_id);
CREATE INDEX idx_rooms_status ON rooms(status);
CREATE INDEX idx_rooms_room_type ON rooms(room_type);

COMMENT ON TABLE rooms IS 'Rooms within a site - top level of spatial hierarchy';
COMMENT ON COLUMN rooms.custom_room_type IS 'User-defined room type when room_type = Custom';

-- =====================================================================
-- 3) Inventory Locations (Universal Hierarchy)
-- =====================================================================

-- Location type enum (supports both cultivation and warehouse paths)
CREATE TYPE location_type AS ENUM (
    'Room',      -- Top level (references rooms table)
    'Zone',      -- Cultivation: grow zone | Warehouse: storage area
    'SubZone',   -- Cultivation: bench/sub-area
    'Row',       -- Cultivation: plant rows
    'Position',  -- Cultivation: specific plant position (matrix location)
    'Rack',      -- Warehouse: storage rack
    'Shelf',     -- Warehouse: rack shelf
    'Bin'        -- Warehouse: bin/tote on shelf
);

-- Location status enum
CREATE TYPE location_status AS ENUM (
    'Active',
    'Inactive',
    'Full',
    'Reserved',
    'Quarantine'
);

CREATE TABLE inventory_locations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    room_id UUID REFERENCES rooms(id) ON DELETE CASCADE,  -- Top level reference
    
    -- Hierarchy
    parent_id UUID REFERENCES inventory_locations(id) ON DELETE CASCADE,
    location_type location_type NOT NULL,
    
    -- Identification
    code VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    barcode VARCHAR(100),  -- For scanning
    
    -- Hierarchy path (materialized for fast queries)
    path VARCHAR(500),  -- e.g., 'Room>Zone>SubZone>Row>Position'
    depth INTEGER NOT NULL DEFAULT 0,
    
    -- Status
    status location_status NOT NULL DEFAULT 'Active',
    
    -- Dimensions (optional)
    length_ft DECIMAL(6,2),
    width_ft DECIMAL(6,2),
    height_ft DECIMAL(6,2),
    
    -- Cultivation-specific (for Zone, SubZone, Row, Position)
    plant_capacity INTEGER,  -- Max plants at this location
    current_plant_count INTEGER DEFAULT 0,
    
    -- Matrix coordinates (for Position type)
    row_number INTEGER,
    column_number INTEGER,
    
    -- Warehouse-specific (for Rack, Shelf, Bin)
    weight_capacity_lbs DECIMAL(10,2),
    current_weight_lbs DECIMAL(10,2) DEFAULT 0,
    
    -- Metadata
    notes TEXT,
    metadata_json JSONB,  -- Flexible extension point
    
    -- Audit fields
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by_user_id UUID NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by_user_id UUID NOT NULL,
    
    CONSTRAINT locations_code_site_unique UNIQUE (site_id, code),
    CONSTRAINT locations_parent_not_self CHECK (parent_id IS NULL OR parent_id <> id),
    CONSTRAINT locations_room_or_parent CHECK (
        (location_type = 'Room' AND room_id IS NOT NULL AND parent_id IS NULL) OR
        (location_type <> 'Room' AND parent_id IS NOT NULL)
    )
);

CREATE INDEX idx_locations_site_id ON inventory_locations(site_id);
CREATE INDEX idx_locations_room_id ON inventory_locations(room_id);
CREATE INDEX idx_locations_parent_id ON inventory_locations(parent_id);
CREATE INDEX idx_locations_type ON inventory_locations(location_type);
CREATE INDEX idx_locations_status ON inventory_locations(status);
CREATE INDEX idx_locations_barcode ON inventory_locations(barcode) WHERE barcode IS NOT NULL;
CREATE INDEX idx_locations_path ON inventory_locations USING GIN (to_tsvector('english', path));

COMMENT ON TABLE inventory_locations IS 'Universal location hierarchy supporting both cultivation and warehouse paths';
COMMENT ON COLUMN inventory_locations.path IS 'Materialized path for fast hierarchy queries (e.g., Room>Zone>Row>Position)';
COMMENT ON COLUMN inventory_locations.depth IS 'Depth in hierarchy tree (0 = Room level)';

-- =====================================================================
-- 4) Trigger to Auto-Update Location Path
-- =====================================================================

CREATE OR REPLACE FUNCTION update_location_path()
RETURNS TRIGGER AS $$
DECLARE
    parent_path VARCHAR;
    parent_depth INTEGER;
BEGIN
    IF NEW.location_type = 'Room' THEN
        -- Top level: just the room name
        NEW.path := NEW.name;
        NEW.depth := 0;
    ELSE
        -- Get parent's path and depth
        SELECT path, depth INTO parent_path, parent_depth
        FROM inventory_locations
        WHERE id = NEW.parent_id;
        
        IF parent_path IS NULL THEN
            RAISE EXCEPTION 'Parent location not found for location %', NEW.id;
        END IF;
        
        NEW.path := parent_path || '>' || NEW.name;
        NEW.depth := parent_depth + 1;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_locations_update_path
    BEFORE INSERT OR UPDATE ON inventory_locations
    FOR EACH ROW
    EXECUTE FUNCTION update_location_path();

-- =====================================================================
-- 5) Row-Level Security (RLS) Policies
-- =====================================================================

-- Enable RLS
ALTER TABLE rooms ENABLE ROW LEVEL SECURITY;
ALTER TABLE inventory_locations ENABLE ROW LEVEL SECURITY;

-- Rooms: Users can only see rooms for their assigned sites
CREATE POLICY rooms_site_isolation ON rooms
    USING (
        site_id IN (
            SELECT site_id 
            FROM user_sites 
            WHERE user_id = current_setting('app.current_user_id', true)::UUID
        )
    );

-- Inventory Locations: Users can only see locations for their assigned sites
CREATE POLICY locations_site_isolation ON inventory_locations
    USING (
        site_id IN (
            SELECT site_id 
            FROM user_sites 
            WHERE user_id = current_setting('app.current_user_id', true)::UUID
        )
    );

-- Additional policy: Room-level access restriction (users can be restricted to specific rooms)
-- This will be implemented in FRP-04 when we have room-level permissions

-- =====================================================================
-- 6) Grants
-- =====================================================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'harvestry_app') THEN
        CREATE ROLE harvestry_app;
    END IF;
END;
$$;

-- Grant access to application role (from FRP-01)
GRANT SELECT, INSERT, UPDATE, DELETE ON rooms TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON inventory_locations TO harvestry_app;

-- Grant sequence access
GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO harvestry_app;

-- =====================================================================
-- End of Spatial Tables Migration
-- =====================================================================
