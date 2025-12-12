-- RLS Test Suite: Setup Test Data
-- This script creates the test data needed for RLS isolation tests

BEGIN;

-- Create test organizations
INSERT INTO public.organizations (id, name, created_at)
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'Test Org Alpha', NOW()),
    ('22222222-2222-2222-2222-222222222222', 'Test Org Beta', NOW())
ON CONFLICT (id) DO NOTHING;

-- Create test sites
INSERT INTO public.sites (id, name, organization_id, created_at)
VALUES 
    -- Org Alpha sites
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Alpha Site 1', '11111111-1111-1111-1111-111111111111', NOW()),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab', 'Alpha Site 2', '11111111-1111-1111-1111-111111111111', NOW()),
    -- Org Beta sites
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Beta Site 1', '22222222-2222-2222-2222-222222222222', NOW()),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbc', 'Beta Site 2', '22222222-2222-2222-2222-222222222222', NOW())
ON CONFLICT (id) DO NOTHING;

-- Create test users
INSERT INTO public.users (id, email, first_name, last_name, status, created_at)
VALUES 
    -- Alpha organization users
    ('a1111111-1111-1111-1111-111111111111', 'admin.alpha@test.com', 'Admin', 'Alpha', 'active', NOW()),
    ('a2222222-2222-2222-2222-222222222222', 'operator.alpha1@test.com', 'Operator', 'Alpha1', 'active', NOW()),
    ('a3333333-3333-3333-3333-333333333333', 'operator.alpha2@test.com', 'Operator', 'Alpha2', 'active', NOW()),
    -- Beta organization users
    ('b1111111-1111-1111-1111-111111111111', 'admin.beta@test.com', 'Admin', 'Beta', 'active', NOW()),
    ('b2222222-2222-2222-2222-222222222222', 'operator.beta@test.com', 'Operator', 'Beta', 'active', NOW()),
    -- Service account
    ('00000000-0000-0000-0000-000000000000', 'service@harvestry.io', 'Service', 'Account', 'active', NOW())
ON CONFLICT (id) DO NOTHING;

-- Create roles if they don't exist
INSERT INTO public.roles (id, name, description, created_at)
VALUES 
    ('r1111111-1111-1111-1111-111111111111', 'admin', 'Administrator', NOW()),
    ('r2222222-2222-2222-2222-222222222222', 'operator', 'Operator', NOW()),
    ('r3333333-3333-3333-3333-333333333333', 'service_account', 'Service Account', NOW())
ON CONFLICT (id) DO NOTHING;

-- Assign users to sites with roles
INSERT INTO public.user_sites (id, user_id, site_id, role_id, is_primary_site, created_at)
VALUES 
    -- Alpha Admin has access to both Alpha sites
    (gen_random_uuid(), 'a1111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'r1111111-1111-1111-1111-111111111111', true, NOW()),
    (gen_random_uuid(), 'a1111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab', 'r1111111-1111-1111-1111-111111111111', false, NOW()),
    -- Alpha Operator 1 has access to Alpha Site 1 only
    (gen_random_uuid(), 'a2222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'r2222222-2222-2222-2222-222222222222', true, NOW()),
    -- Alpha Operator 2 has access to Alpha Site 2 only
    (gen_random_uuid(), 'a3333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab', 'r2222222-2222-2222-2222-222222222222', true, NOW()),
    -- Beta Admin has access to both Beta sites
    (gen_random_uuid(), 'b1111111-1111-1111-1111-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'r1111111-1111-1111-1111-111111111111', true, NOW()),
    (gen_random_uuid(), 'b1111111-1111-1111-1111-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbc', 'r1111111-1111-1111-1111-111111111111', false, NOW()),
    -- Beta Operator has access to Beta Site 1
    (gen_random_uuid(), 'b2222222-2222-2222-2222-222222222222', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'r2222222-2222-2222-2222-222222222222', true, NOW())
ON CONFLICT DO NOTHING;

-- Create test rooms for each site
INSERT INTO public.rooms (id, site_id, name, room_type, status, created_at)
VALUES 
    -- Alpha Site 1 rooms
    ('ra111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Alpha1 Veg Room', 'vegetation', 'active', NOW()),
    ('ra222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Alpha1 Flower Room', 'flowering', 'active', NOW()),
    -- Alpha Site 2 rooms
    ('ra333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab', 'Alpha2 Veg Room', 'vegetation', 'active', NOW()),
    -- Beta Site 1 rooms
    ('rb111111-1111-1111-1111-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Beta1 Veg Room', 'vegetation', 'active', NOW()),
    ('rb222222-2222-2222-2222-222222222222', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Beta1 Flower Room', 'flowering', 'active', NOW()),
    -- Beta Site 2 rooms
    ('rb333333-3333-3333-3333-333333333333', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbc', 'Beta2 Veg Room', 'vegetation', 'active', NOW())
ON CONFLICT (id) DO NOTHING;

-- Create test strains for genetics testing
INSERT INTO genetics.strains (id, site_id, name, genetic_lineage, created_at)
VALUES 
    -- Alpha Site 1 strains
    ('sa111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Alpha OG', 'OG Kush x Northern Lights', NOW()),
    ('sa222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Alpha Haze', 'Super Silver Haze x Blue Dream', NOW()),
    -- Alpha Site 2 strains
    ('sa333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab', 'Alpha Diesel', 'Sour Diesel x Durban', NOW()),
    -- Beta Site 1 strains
    ('sb111111-1111-1111-1111-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Beta Kush', 'Master Kush x OG', NOW()),
    ('sb222222-2222-2222-2222-222222222222', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Beta Purple', 'Purple Punch x Gelato', NOW())
ON CONFLICT (id) DO NOTHING;

-- Create test tasks
INSERT INTO tasks.tasks (id, site_id, title, status, priority, created_by_user_id, created_at)
VALUES 
    -- Alpha Site 1 tasks
    ('ta111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Alpha1 Task 1', 'pending', 'medium', 'a2222222-2222-2222-2222-222222222222', NOW()),
    ('ta222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Alpha1 Task 2', 'in_progress', 'high', 'a2222222-2222-2222-2222-222222222222', NOW()),
    -- Alpha Site 2 tasks
    ('ta333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab', 'Alpha2 Task 1', 'pending', 'low', 'a3333333-3333-3333-3333-333333333333', NOW()),
    -- Beta Site 1 tasks
    ('tb111111-1111-1111-1111-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Beta1 Task 1', 'pending', 'medium', 'b2222222-2222-2222-2222-222222222222', NOW()),
    ('tb222222-2222-2222-2222-222222222222', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Beta1 Task 2', 'completed', 'high', 'b2222222-2222-2222-2222-222222222222', NOW())
ON CONFLICT (id) DO NOTHING;

COMMIT;

-- Verification query
SELECT 
    'Organizations' as entity, COUNT(*) as count FROM public.organizations WHERE id IN ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222')
UNION ALL
SELECT 'Sites', COUNT(*) FROM public.sites WHERE organization_id IN ('11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222')
UNION ALL
SELECT 'Users', COUNT(*) FROM public.users WHERE email LIKE '%@test.com'
UNION ALL
SELECT 'User Site Assignments', COUNT(*) FROM public.user_sites WHERE user_id IN (SELECT id FROM public.users WHERE email LIKE '%@test.com');








