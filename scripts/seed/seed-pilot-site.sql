-- ============================================================================
-- Harvestry Pilot Site Seed Data
-- Site: Denver Grow Co., Colorado (METRC)
-- 
-- This script creates idempotent seed data for the pilot site including:
-- - 1 Organization with 1 Site
-- - 10 Users with roles and badges
-- - 2 Rooms with zones and equipment
-- - 3 Strains with genetics
-- - 2 Batches with stage history
-- - Sensor streams and inventory lots
--
-- Run: psql -f seed-pilot-site.sql
-- ============================================================================

-- Enable verbose output
\set QUIET off
\echo 'Starting Harvestry Pilot Site Seed Data...'

-- Wrap in transaction for atomicity
BEGIN;

-- Set RLS bypass for seeding
SET LOCAL app.user_role = 'service_account';
SET LOCAL app.current_user_id = '00000000-0000-0000-0000-000000000001';

-- ============================================================================
-- 1. ORGANIZATION
-- ============================================================================
\echo '  Creating organization...'

INSERT INTO organizations (organization_id, name, slug, status, metadata, created_by, updated_by)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    'Denver Grow Co.',
    'denver-grow-co',
    'active',
    '{"license_type": "cultivation", "state": "CO", "compliance_system": "METRC"}'::JSONB,
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001'
)
ON CONFLICT (slug) DO UPDATE SET 
    name = EXCLUDED.name,
    metadata = EXCLUDED.metadata,
    updated_at = NOW();

-- ============================================================================
-- 2. SITE
-- ============================================================================
\echo '  Creating site...'

INSERT INTO sites (
    site_id, org_id, site_name, site_code, address_line1, city, state_province,
    postal_code, country, timezone, license_number, license_type, license_expiration,
    site_type, status, site_policies
)
VALUES (
    '22222222-2222-2222-2222-222222222222',
    '11111111-1111-1111-1111-111111111111',
    'Denver Main Facility',
    'DGC-DEN-01',
    '1234 Cannabis Way',
    'Denver',
    'Colorado',
    '80202',
    'US',
    'America/Denver',
    'CO-CUL-2025-001234',
    'cultivation',
    '2026-12-31',
    'cultivation',
    'active',
    '{"manual_irrigation_approval": true, "two_person_destruction": true}'::JSONB
)
ON CONFLICT (site_code) DO UPDATE SET
    site_name = EXCLUDED.site_name,
    site_policies = EXCLUDED.site_policies,
    updated_at = NOW();

-- ============================================================================
-- 3. USERS (10 total: 4 Operators, 3 Managers, 2 Compliance, 1 Admin)
-- ============================================================================
\echo '  Creating users...'

-- System user (for audit trails)
INSERT INTO users (user_id, email, first_name, last_name, display_name, status, created_by, updated_by)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'system@harvestry.io',
    'System',
    'Account',
    'System',
    'active',
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001'
)
ON CONFLICT (email) DO NOTHING;

-- Admin user
INSERT INTO users (user_id, email, first_name, last_name, display_name, status, created_by, updated_by)
VALUES (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    'admin@denvergrow.co',
    'Sarah',
    'Johnson',
    'Sarah Johnson',
    'active',
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001'
)
ON CONFLICT (email) DO NOTHING;

-- Managers (3)
INSERT INTO users (user_id, email, first_name, last_name, display_name, status, created_by, updated_by)
VALUES 
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'mike.chen@denvergrow.co', 'Mike', 'Chen', 'Mike Chen', 'active', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'lisa.rodriguez@denvergrow.co', 'Lisa', 'Rodriguez', 'Lisa Rodriguez', 'active', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', 'david.kim@denvergrow.co', 'David', 'Kim', 'David Kim', 'active', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (email) DO NOTHING;

-- Compliance Officers (2)
INSERT INTO users (user_id, email, first_name, last_name, display_name, status, created_by, updated_by)
VALUES 
    ('cccccccc-cccc-cccc-cccc-cccccccccc01', 'emma.wilson@denvergrow.co', 'Emma', 'Wilson', 'Emma Wilson', 'active', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('cccccccc-cccc-cccc-cccc-cccccccccc02', 'james.taylor@denvergrow.co', 'James', 'Taylor', 'James Taylor', 'active', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (email) DO NOTHING;

-- Operators (4)
INSERT INTO users (user_id, email, first_name, last_name, display_name, status, created_by, updated_by)
VALUES 
    ('dddddddd-dddd-dddd-dddd-dddddddddd01', 'alex.martinez@denvergrow.co', 'Alex', 'Martinez', 'Alex Martinez', 'active', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd02', 'jordan.lee@denvergrow.co', 'Jordan', 'Lee', 'Jordan Lee', 'active', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd03', 'casey.brown@denvergrow.co', 'Casey', 'Brown', 'Casey Brown', 'active', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('dddddddd-dddd-dddd-dddd-dddddddddd04', 'sam.garcia@denvergrow.co', 'Sam', 'Garcia', 'Sam Garcia', 'active', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (email) DO NOTHING;

-- ============================================================================
-- 4. USER-SITE ASSIGNMENTS WITH ROLES
-- ============================================================================
\echo '  Creating user-site assignments...'

-- Get role IDs
DO $$
DECLARE
    v_admin_role UUID;
    v_manager_role UUID;
    v_supervisor_role UUID;
    v_operator_role UUID;
    v_site_id UUID := '22222222-2222-2222-2222-222222222222';
BEGIN
    SELECT role_id INTO v_admin_role FROM roles WHERE role_name = 'admin';
    SELECT role_id INTO v_manager_role FROM roles WHERE role_name = 'manager';
    SELECT role_id INTO v_supervisor_role FROM roles WHERE role_name = 'supervisor';
    SELECT role_id INTO v_operator_role FROM roles WHERE role_name = 'operator';
    
    -- Admin
    INSERT INTO user_sites (user_id, site_id, role_id, is_primary_site, assigned_by, created_by, updated_by)
    VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', v_site_id, v_admin_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
    ON CONFLICT (user_id, site_id) DO NOTHING;
    
    -- Managers
    INSERT INTO user_sites (user_id, site_id, role_id, is_primary_site, assigned_by, created_by, updated_by)
    VALUES 
        ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', v_site_id, v_manager_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
        ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', v_site_id, v_manager_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
        ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', v_site_id, v_manager_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
    ON CONFLICT (user_id, site_id) DO NOTHING;
    
    -- Compliance (as supervisors)
    INSERT INTO user_sites (user_id, site_id, role_id, is_primary_site, assigned_by, created_by, updated_by)
    VALUES 
        ('cccccccc-cccc-cccc-cccc-cccccccccc01', v_site_id, v_supervisor_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
        ('cccccccc-cccc-cccc-cccc-cccccccccc02', v_site_id, v_supervisor_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
    ON CONFLICT (user_id, site_id) DO NOTHING;
    
    -- Operators
    INSERT INTO user_sites (user_id, site_id, role_id, is_primary_site, assigned_by, created_by, updated_by)
    VALUES 
        ('dddddddd-dddd-dddd-dddd-dddddddddd01', v_site_id, v_operator_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
        ('dddddddd-dddd-dddd-dddd-dddddddddd02', v_site_id, v_operator_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
        ('dddddddd-dddd-dddd-dddd-dddddddddd03', v_site_id, v_operator_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
        ('dddddddd-dddd-dddd-dddd-dddddddddd04', v_site_id, v_operator_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
    ON CONFLICT (user_id, site_id) DO NOTHING;
END $$;

-- ============================================================================
-- 5. BADGES FOR OPERATORS
-- ============================================================================
\echo '  Creating badges...'

INSERT INTO badges (badge_id, user_id, site_id, badge_code, badge_type, status, expires_at)
VALUES 
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01', 'dddddddd-dddd-dddd-dddd-dddddddddd01', '22222222-2222-2222-2222-222222222222', 'DGC-OP-001', 'physical', 'active', NOW() + INTERVAL '1 year'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02', 'dddddddd-dddd-dddd-dddd-dddddddddd02', '22222222-2222-2222-2222-222222222222', 'DGC-OP-002', 'physical', 'active', NOW() + INTERVAL '1 year'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03', 'dddddddd-dddd-dddd-dddd-dddddddddd03', '22222222-2222-2222-2222-222222222222', 'DGC-OP-003', 'physical', 'active', NOW() + INTERVAL '1 year'),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee04', 'dddddddd-dddd-dddd-dddd-dddddddddd04', '22222222-2222-2222-2222-222222222222', 'DGC-OP-004', 'physical', 'active', NOW() + INTERVAL '1 year')
ON CONFLICT (badge_code) DO UPDATE SET
    status = EXCLUDED.status,
    expires_at = EXCLUDED.expires_at,
    updated_at = NOW();

-- ============================================================================
-- 6. ROOMS (2: Veg with 4 zones, Flower with 6 zones)
-- ============================================================================
\echo '  Creating rooms...'

INSERT INTO rooms (id, site_id, code, name, room_type, status, floor_level, area_sqft, height_ft, target_temp_f, target_humidity_pct, target_co2_ppm, created_by_user_id, updated_by_user_id)
VALUES 
    ('33333333-3333-3333-3333-333333333301', '22222222-2222-2222-2222-222222222222', 'VEG-01', 'Vegetation Room 1', 'Veg', 'Active', 1, 2000, 12, 78, 65, 1000, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('33333333-3333-3333-3333-333333333302', '22222222-2222-2222-2222-222222222222', 'FLW-01', 'Flower Room 1', 'Flower', 'Active', 1, 4000, 14, 75, 50, 1200, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (site_id, code) DO UPDATE SET
    name = EXCLUDED.name,
    target_temp_f = EXCLUDED.target_temp_f,
    target_humidity_pct = EXCLUDED.target_humidity_pct,
    updated_at = NOW();

-- ============================================================================
-- 7. ZONES (4 in Veg, 6 in Flower)
-- ============================================================================
\echo '  Creating zones...'

-- Veg Room Zones
INSERT INTO inventory_locations (id, site_id, room_id, parent_id, location_type, code, name, status, plant_capacity, created_by_user_id, updated_by_user_id)
VALUES 
    ('44444444-4444-4444-4444-444444444401', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333301', NULL, 'Room', 'VEG-01', 'Vegetation Room 1', 'Active', NULL, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (site_id, code) DO NOTHING;

INSERT INTO inventory_locations (id, site_id, room_id, parent_id, location_type, code, name, status, plant_capacity, created_by_user_id, updated_by_user_id)
VALUES 
    ('44444444-4444-4444-4444-444444444411', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333301', '44444444-4444-4444-4444-444444444401', 'Zone', 'VEG-Z1', 'Veg Zone A', 'Active', 200, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('44444444-4444-4444-4444-444444444412', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333301', '44444444-4444-4444-4444-444444444401', 'Zone', 'VEG-Z2', 'Veg Zone B', 'Active', 200, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('44444444-4444-4444-4444-444444444413', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333301', '44444444-4444-4444-4444-444444444401', 'Zone', 'VEG-Z3', 'Veg Zone C', 'Active', 200, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('44444444-4444-4444-4444-444444444414', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333301', '44444444-4444-4444-4444-444444444401', 'Zone', 'VEG-Z4', 'Veg Zone D', 'Active', 200, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (site_id, code) DO NOTHING;

-- Flower Room Zones
INSERT INTO inventory_locations (id, site_id, room_id, parent_id, location_type, code, name, status, plant_capacity, created_by_user_id, updated_by_user_id)
VALUES 
    ('44444444-4444-4444-4444-444444444402', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333302', NULL, 'Room', 'FLW-01', 'Flower Room 1', 'Active', NULL, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (site_id, code) DO NOTHING;

INSERT INTO inventory_locations (id, site_id, room_id, parent_id, location_type, code, name, status, plant_capacity, created_by_user_id, updated_by_user_id)
VALUES 
    ('44444444-4444-4444-4444-444444444421', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333302', '44444444-4444-4444-4444-444444444402', 'Zone', 'FLW-Z1', 'Flower Zone 1', 'Active', 150, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('44444444-4444-4444-4444-444444444422', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333302', '44444444-4444-4444-4444-444444444402', 'Zone', 'FLW-Z2', 'Flower Zone 2', 'Active', 150, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('44444444-4444-4444-4444-444444444423', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333302', '44444444-4444-4444-4444-444444444402', 'Zone', 'FLW-Z3', 'Flower Zone 3', 'Active', 150, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('44444444-4444-4444-4444-444444444424', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333302', '44444444-4444-4444-4444-444444444402', 'Zone', 'FLW-Z4', 'Flower Zone 4', 'Active', 150, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('44444444-4444-4444-4444-444444444425', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333302', '44444444-4444-4444-4444-444444444402', 'Zone', 'FLW-Z5', 'Flower Zone 5', 'Active', 150, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('44444444-4444-4444-4444-444444444426', '22222222-2222-2222-2222-222222222222', '33333333-3333-3333-3333-333333333302', '44444444-4444-4444-4444-444444444402', 'Zone', 'FLW-Z6', 'Flower Zone 6', 'Active', 150, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (site_id, code) DO NOTHING;

-- ============================================================================
-- 8. EQUIPMENT (10 sensors, 5 valves, 2 pumps)
-- ============================================================================
\echo '  Creating equipment...'

INSERT INTO equipment (id, site_id, code, type_code, core_type, status, location_id, manufacturer, model, mqtt_topic, online, created_by_user_id, updated_by_user_id)
VALUES 
    -- Temperature Sensors (2)
    ('55555555-5555-5555-5555-555555555501', '22222222-2222-2222-2222-222222222222', 'TEMP-VEG-01', 'temperature_sensor', 'sensor', 'Active', '44444444-4444-4444-4444-444444444411', 'Harvestry', 'HT-100', 'site/22222222-2222-2222-2222-222222222222/equipment/TEMP-VEG-01/telemetry', true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('55555555-5555-5555-5555-555555555502', '22222222-2222-2222-2222-222222222222', 'TEMP-FLW-01', 'temperature_sensor', 'sensor', 'Active', '44444444-4444-4444-4444-444444444421', 'Harvestry', 'HT-100', 'site/22222222-2222-2222-2222-222222222222/equipment/TEMP-FLW-01/telemetry', true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    -- Humidity Sensors (2)
    ('55555555-5555-5555-5555-555555555503', '22222222-2222-2222-2222-222222222222', 'HUM-VEG-01', 'humidity_sensor', 'sensor', 'Active', '44444444-4444-4444-4444-444444444411', 'Harvestry', 'HH-100', 'site/22222222-2222-2222-2222-222222222222/equipment/HUM-VEG-01/telemetry', true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('55555555-5555-5555-5555-555555555504', '22222222-2222-2222-2222-222222222222', 'HUM-FLW-01', 'humidity_sensor', 'sensor', 'Active', '44444444-4444-4444-4444-444444444421', 'Harvestry', 'HH-100', 'site/22222222-2222-2222-2222-222222222222/equipment/HUM-FLW-01/telemetry', true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    -- CO2 Sensors (2)
    ('55555555-5555-5555-5555-555555555505', '22222222-2222-2222-2222-222222222222', 'CO2-VEG-01', 'co2_sensor', 'sensor', 'Active', '44444444-4444-4444-4444-444444444411', 'Harvestry', 'HC-200', 'site/22222222-2222-2222-2222-222222222222/equipment/CO2-VEG-01/telemetry', true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('55555555-5555-5555-5555-555555555506', '22222222-2222-2222-2222-222222222222', 'CO2-FLW-01', 'co2_sensor', 'sensor', 'Active', '44444444-4444-4444-4444-444444444421', 'Harvestry', 'HC-200', 'site/22222222-2222-2222-2222-222222222222/equipment/CO2-FLW-01/telemetry', true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    -- EC Sensors (2)
    ('55555555-5555-5555-5555-555555555507', '22222222-2222-2222-2222-222222222222', 'EC-VEG-01', 'ec_sensor', 'sensor', 'Active', '44444444-4444-4444-4444-444444444412', 'Harvestry', 'HSES12', 'site/22222222-2222-2222-2222-222222222222/equipment/EC-VEG-01/telemetry', true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('55555555-5555-5555-5555-555555555508', '22222222-2222-2222-2222-222222222222', 'EC-FLW-01', 'ec_sensor', 'sensor', 'Active', '44444444-4444-4444-4444-444444444422', 'Harvestry', 'HSES12', 'site/22222222-2222-2222-2222-222222222222/equipment/EC-FLW-01/telemetry', true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    -- pH Sensors (2)
    ('55555555-5555-5555-5555-555555555509', '22222222-2222-2222-2222-222222222222', 'PH-VEG-01', 'ph_sensor', 'sensor', 'Active', '44444444-4444-4444-4444-444444444412', 'Harvestry', 'HPH-100', 'site/22222222-2222-2222-2222-222222222222/equipment/PH-VEG-01/telemetry', true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('55555555-5555-5555-5555-555555555510', '22222222-2222-2222-2222-222222222222', 'PH-FLW-01', 'ph_sensor', 'sensor', 'Active', '44444444-4444-4444-4444-444444444422', 'Harvestry', 'HPH-100', 'site/22222222-2222-2222-2222-222222222222/equipment/PH-FLW-01/telemetry', true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    -- Valves (5)
    ('55555555-5555-5555-5555-555555555511', '22222222-2222-2222-2222-222222222222', 'VALVE-Z1', 'irrigation_valve', 'valve', 'Active', '44444444-4444-4444-4444-444444444411', 'Harvestry', 'HV-200', NULL, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('55555555-5555-5555-5555-555555555512', '22222222-2222-2222-2222-222222222222', 'VALVE-Z2', 'irrigation_valve', 'valve', 'Active', '44444444-4444-4444-4444-444444444412', 'Harvestry', 'HV-200', NULL, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('55555555-5555-5555-5555-555555555513', '22222222-2222-2222-2222-222222222222', 'VALVE-Z3', 'irrigation_valve', 'valve', 'Active', '44444444-4444-4444-4444-444444444421', 'Harvestry', 'HV-200', NULL, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('55555555-5555-5555-5555-555555555514', '22222222-2222-2222-2222-222222222222', 'VALVE-Z4', 'irrigation_valve', 'valve', 'Active', '44444444-4444-4444-4444-444444444422', 'Harvestry', 'HV-200', NULL, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('55555555-5555-5555-5555-555555555515', '22222222-2222-2222-2222-222222222222', 'VALVE-Z5', 'irrigation_valve', 'valve', 'Active', '44444444-4444-4444-4444-444444444423', 'Harvestry', 'HV-200', NULL, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    -- Pumps (2)
    ('55555555-5555-5555-5555-555555555516', '22222222-2222-2222-2222-222222222222', 'PUMP-MAIN-01', 'irrigation_pump', 'pump', 'Active', NULL, 'Harvestry', 'HP-500', NULL, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('55555555-5555-5555-5555-555555555517', '22222222-2222-2222-2222-222222222222', 'PUMP-MAIN-02', 'irrigation_pump', 'pump', 'Active', NULL, 'Harvestry', 'HP-500', NULL, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (site_id, code) DO UPDATE SET
    status = EXCLUDED.status,
    online = EXCLUDED.online,
    updated_at = NOW();

-- ============================================================================
-- 9. SENSOR STREAMS (6 types: temp, humidity, VPD, CO2, EC, pH)
-- ============================================================================
\echo '  Creating sensor streams...'

INSERT INTO sensor_streams (id, site_id, equipment_id, stream_type, unit, display_name, zone_id, is_active)
VALUES 
    -- Temperature streams
    ('66666666-6666-6666-6666-666666666601', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555501', 'Temperature', 'degF', 'Veg Zone A Temperature', '44444444-4444-4444-4444-444444444411', true),
    ('66666666-6666-6666-6666-666666666602', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555502', 'Temperature', 'degF', 'Flower Zone 1 Temperature', '44444444-4444-4444-4444-444444444421', true),
    -- Humidity streams
    ('66666666-6666-6666-6666-666666666603', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555503', 'Humidity', 'pct', 'Veg Zone A Humidity', '44444444-4444-4444-4444-444444444411', true),
    ('66666666-6666-6666-6666-666666666604', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555504', 'Humidity', 'pct', 'Flower Zone 1 Humidity', '44444444-4444-4444-4444-444444444421', true),
    -- CO2 streams
    ('66666666-6666-6666-6666-666666666605', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555505', 'CO2', 'ppm', 'Veg Zone A CO2', '44444444-4444-4444-4444-444444444411', true),
    ('66666666-6666-6666-6666-666666666606', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555506', 'CO2', 'ppm', 'Flower Zone 1 CO2', '44444444-4444-4444-4444-444444444421', true),
    -- EC streams
    ('66666666-6666-6666-6666-666666666607', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555507', 'EC', 'uS', 'Veg Zone B EC', '44444444-4444-4444-4444-444444444412', true),
    ('66666666-6666-6666-6666-666666666608', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555508', 'EC', 'uS', 'Flower Zone 2 EC', '44444444-4444-4444-4444-444444444422', true),
    -- pH streams
    ('66666666-6666-6666-6666-666666666609', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555509', 'pH', 'pH', 'Veg Zone B pH', '44444444-4444-4444-4444-444444444412', true),
    ('66666666-6666-6666-6666-666666666610', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555510', 'pH', 'pH', 'Flower Zone 2 pH', '44444444-4444-4444-4444-444444444422', true)
ON CONFLICT DO NOTHING;

-- ============================================================================
-- 10. GENETICS & STRAINS
-- ============================================================================
\echo '  Creating genetics and strains...'

-- Genetics
INSERT INTO genetics.genetics (id, site_id, name, description, genetic_type, thc_min_percentage, thc_max_percentage, cbd_min_percentage, cbd_max_percentage, flowering_time_days, yield_potential, created_by_user_id, updated_by_user_id)
VALUES 
    ('77777777-7777-7777-7777-777777777701', '22222222-2222-2222-2222-222222222222', 'Blue Dream Genetics', 'Sativa-dominant hybrid genetics from DJ Short Blueberry and Haze', 'Hybrid', 17.0, 24.0, 0.1, 2.0, 65, 'High', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('77777777-7777-7777-7777-777777777702', '22222222-2222-2222-2222-222222222222', 'OG Kush Genetics', 'Classic indica-dominant genetics from SFV OG lineage', 'Indica', 20.0, 27.0, 0.05, 0.3, 55, 'Medium', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('77777777-7777-7777-7777-777777777703', '22222222-2222-2222-2222-222222222222', 'Gorilla Glue Genetics', 'Potent hybrid genetics known for resin production', 'Hybrid', 25.0, 30.0, 0.1, 1.0, 60, 'High', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (site_id, name) DO NOTHING;

-- Strains
INSERT INTO genetics.strains (id, site_id, genetics_id, name, breeder, description, expected_harvest_window_days, created_by_user_id, updated_by_user_id)
VALUES 
    ('88888888-8888-8888-8888-888888888801', '22222222-2222-2222-2222-222222222222', '77777777-7777-7777-7777-777777777701', 'Blue Dream', 'DJ Short', 'Premium sativa-dominant hybrid with sweet berry aroma', 65, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('88888888-8888-8888-8888-888888888802', '22222222-2222-2222-2222-222222222222', '77777777-7777-7777-7777-777777777702', 'OG Kush', 'Unknown', 'Classic indica-dominant with earthy, pine aroma', 55, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('88888888-8888-8888-8888-888888888803', '22222222-2222-2222-2222-222222222222', '77777777-7777-7777-7777-777777777703', 'Gorilla Glue #4', 'GG Strains', 'Heavy-hitting hybrid with diesel and chocolate notes', 60, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (site_id, name) DO NOTHING;

-- ============================================================================
-- 11. BATCH STAGE DEFINITIONS
-- ============================================================================
\echo '  Creating batch stage definitions...'

INSERT INTO genetics.batch_stage_definitions (id, site_id, stage_key, display_name, description, sequence_order, is_terminal, created_by_user_id, updated_by_user_id)
VALUES 
    ('99999999-9999-9999-9999-999999999901', '22222222-2222-2222-2222-222222222222', 'SEEDLING', 'Seedling', 'Initial seedling stage', 1, false, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('99999999-9999-9999-9999-999999999902', '22222222-2222-2222-2222-222222222222', 'VEG', 'Vegetation', 'Vegetative growth stage', 2, false, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('99999999-9999-9999-9999-999999999903', '22222222-2222-2222-2222-222222222222', 'FLOWER', 'Flower', 'Flowering stage', 3, false, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('99999999-9999-9999-9999-999999999904', '22222222-2222-2222-2222-222222222222', 'HARVEST', 'Harvest', 'Ready for harvest', 4, false, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('99999999-9999-9999-9999-999999999905', '22222222-2222-2222-2222-222222222222', 'COMPLETE', 'Complete', 'Batch complete', 5, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (site_id, stage_key) DO NOTHING;

-- ============================================================================
-- 12. BATCHES (2: BD-V-001 in Veg, OG-F-002 in Flower)
-- ============================================================================
\echo '  Creating batches...'

INSERT INTO genetics.batches (id, site_id, strain_id, batch_code, batch_name, batch_type, source_type, plant_count, target_plant_count, current_stage_id, room_id, zone_id, status, created_by_user_id, updated_by_user_id)
VALUES 
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa001', '22222222-2222-2222-2222-222222222222', '88888888-8888-8888-8888-888888888801', 'BD-V-001', 'Blue Dream Veg Batch 1', 'Production', 'Clone', 150, 200, '99999999-9999-9999-9999-999999999902', '33333333-3333-3333-3333-333333333301', '44444444-4444-4444-4444-444444444411', 'Active', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001'),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002', '22222222-2222-2222-2222-222222222222', '88888888-8888-8888-8888-888888888802', 'OG-F-002', 'OG Kush Flower Batch 2', 'Production', 'Clone', 120, 150, '99999999-9999-9999-9999-999999999903', '33333333-3333-3333-3333-333333333302', '44444444-4444-4444-4444-444444444421', 'Active', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT (site_id, batch_code) DO NOTHING;

-- Batch Events
INSERT INTO genetics.batch_events (id, site_id, batch_id, event_type, event_data, performed_by_user_id, performed_at)
VALUES 
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', '22222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa001', 'BatchCreated', '{"initial_count": 150}'::JSONB, 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', NOW() - INTERVAL '14 days'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', '22222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa001', 'StageTransition', '{"from": "SEEDLING", "to": "VEG"}'::JSONB, 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', NOW() - INTERVAL '7 days'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb003', '22222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002', 'BatchCreated', '{"initial_count": 120}'::JSONB, 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', NOW() - INTERVAL '45 days'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb004', '22222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002', 'StageTransition', '{"from": "VEG", "to": "FLOWER"}'::JSONB, 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', NOW() - INTERVAL '21 days')
ON CONFLICT DO NOTHING;

-- ============================================================================
-- COMPLETION
-- ============================================================================

COMMIT;

\echo ''
\echo '============================================================================'
\echo 'Harvestry Pilot Site Seed Data Complete!'
\echo '============================================================================'
\echo 'Created:'
\echo '  - 1 Organization: Denver Grow Co.'
\echo '  - 1 Site: Denver Main Facility (DGC-DEN-01)'
\echo '  - 10 Users: 1 Admin, 3 Managers, 2 Compliance, 4 Operators'
\echo '  - 4 Badges for operators'
\echo '  - 2 Rooms: Veg (4 zones), Flower (6 zones)'
\echo '  - 17 Equipment: 10 sensors, 5 valves, 2 pumps'
\echo '  - 10 Sensor streams'
\echo '  - 3 Genetics with 3 strains'
\echo '  - 2 Active batches with stage history'
\echo '============================================================================'

