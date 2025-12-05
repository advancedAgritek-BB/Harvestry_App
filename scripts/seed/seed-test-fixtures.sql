-- ============================================================================
-- Harvestry Test Fixtures (Minimal Seed Data for CI)
-- 
-- Lightweight seed data for integration testing. Contains only essential
-- records needed for test scenarios without the full pilot site data.
--
-- Run: psql -f seed-test-fixtures.sql
-- ============================================================================

\set QUIET off
\echo 'Starting Harvestry Test Fixtures Seed...'

BEGIN;

-- Set RLS bypass for seeding
SET LOCAL app.user_role = 'service_account';
SET LOCAL app.current_user_id = '00000000-0000-0000-0000-000000000001';

-- ============================================================================
-- 1. MINIMAL ORGANIZATION & SITE
-- ============================================================================

INSERT INTO organizations (organization_id, name, slug, status, created_by, updated_by)
VALUES (
    'ffffffff-ffff-ffff-ffff-ffffffffffff',
    'Test Organization',
    'test-org',
    'active',
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001'
)
ON CONFLICT (slug) DO NOTHING;

INSERT INTO sites (
    site_id, org_id, site_name, site_code, city, state_province,
    timezone, site_type, status
)
VALUES (
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    'ffffffff-ffff-ffff-ffff-ffffffffffff',
    'Test Site',
    'TEST-01',
    'Test City',
    'Test State',
    'UTC',
    'cultivation',
    'active'
)
ON CONFLICT (site_code) DO NOTHING;

-- ============================================================================
-- 2. TEST USERS (System + Admin + Operator)
-- ============================================================================

-- System user
INSERT INTO users (user_id, email, first_name, last_name, display_name, status, created_by, updated_by)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'system@test.harvestry.io',
    'System',
    'Test',
    'System',
    'active',
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001'
)
ON CONFLICT (email) DO NOTHING;

-- Test admin
INSERT INTO users (user_id, email, first_name, last_name, display_name, status, created_by, updated_by)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    'admin@test.harvestry.io',
    'Test',
    'Admin',
    'Test Admin',
    'active',
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001'
)
ON CONFLICT (email) DO NOTHING;

-- Test operator
INSERT INTO users (user_id, email, first_name, last_name, display_name, status, created_by, updated_by)
VALUES (
    '22222222-2222-2222-2222-222222222222',
    'operator@test.harvestry.io',
    'Test',
    'Operator',
    'Test Operator',
    'active',
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001'
)
ON CONFLICT (email) DO NOTHING;

-- ============================================================================
-- 3. USER-SITE ASSIGNMENTS
-- ============================================================================

DO $$
DECLARE
    v_admin_role UUID;
    v_operator_role UUID;
    v_test_site UUID := 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee';
BEGIN
    SELECT role_id INTO v_admin_role FROM roles WHERE role_name = 'admin';
    SELECT role_id INTO v_operator_role FROM roles WHERE role_name = 'operator';
    
    -- Admin assignment
    INSERT INTO user_sites (user_id, site_id, role_id, is_primary_site, assigned_by, created_by, updated_by)
    VALUES ('11111111-1111-1111-1111-111111111111', v_test_site, v_admin_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
    ON CONFLICT (user_id, site_id) DO NOTHING;
    
    -- Operator assignment
    INSERT INTO user_sites (user_id, site_id, role_id, is_primary_site, assigned_by, created_by, updated_by)
    VALUES ('22222222-2222-2222-2222-222222222222', v_test_site, v_operator_role, true, '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
    ON CONFLICT (user_id, site_id) DO NOTHING;
END $$;

-- ============================================================================
-- 4. TEST BADGE
-- ============================================================================

INSERT INTO badges (badge_id, user_id, site_id, badge_code, badge_type, status, expires_at)
VALUES (
    '33333333-3333-3333-3333-333333333333',
    '22222222-2222-2222-2222-222222222222',
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    'TEST-OP-001',
    'physical',
    'active',
    NOW() + INTERVAL '1 year'
)
ON CONFLICT (badge_code) DO NOTHING;

-- ============================================================================
-- 5. MINIMAL ROOM & ZONE
-- ============================================================================

INSERT INTO rooms (id, site_id, code, name, room_type, status, created_by_user_id, updated_by_user_id)
VALUES (
    '44444444-4444-4444-4444-444444444444',
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    'TEST-ROOM-01',
    'Test Room',
    'Veg',
    'Active',
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001'
)
ON CONFLICT (site_id, code) DO NOTHING;

INSERT INTO inventory_locations (id, site_id, room_id, parent_id, location_type, code, name, status, plant_capacity, created_by_user_id, updated_by_user_id)
VALUES (
    '55555555-5555-5555-5555-555555555555',
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    '44444444-4444-4444-4444-444444444444',
    NULL,
    'Room',
    'TEST-ROOM-01',
    'Test Room',
    'Active',
    NULL,
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001'
)
ON CONFLICT (site_id, code) DO NOTHING;

INSERT INTO inventory_locations (id, site_id, room_id, parent_id, location_type, code, name, status, plant_capacity, created_by_user_id, updated_by_user_id)
VALUES (
    '66666666-6666-6666-6666-666666666666',
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    '44444444-4444-4444-4444-444444444444',
    '55555555-5555-5555-5555-555555555555',
    'Zone',
    'TEST-ZONE-01',
    'Test Zone',
    'Active',
    100,
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001'
)
ON CONFLICT (site_id, code) DO NOTHING;

-- ============================================================================
-- 6. MINIMAL SENSOR STREAM
-- ============================================================================

INSERT INTO sensor_streams (id, site_id, equipment_id, stream_type, unit, display_name, zone_id, is_active)
VALUES (
    '77777777-7777-7777-7777-777777777777',
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    '00000000-0000-0000-0000-000000000000', -- Placeholder equipment ID
    'Temperature',
    'degF',
    'Test Temperature Stream',
    '66666666-6666-6666-6666-666666666666',
    true
)
ON CONFLICT DO NOTHING;

COMMIT;

\echo ''
\echo 'Test Fixtures Seed Complete!'
\echo '  - 1 Test Organization + Site'
\echo '  - 3 Users (system, admin, operator)'
\echo '  - 1 Badge'
\echo '  - 1 Room with 1 Zone'
\echo '  - 1 Sensor Stream'

