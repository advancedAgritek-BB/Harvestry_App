-- ============================================================================
-- ANALYTICS REPORTING VIEWS
-- ============================================================================

-- ============================================================================
-- HARVESTS VIEW
-- ============================================================================
CREATE OR REPLACE VIEW analytics.vw_harvests_flat WITH (security_invoker = true) AS
SELECT
    h.id AS harvest_id,
    h.site_id,
    s.site_name,
    h.harvest_name,
    h.harvest_type,
    h.strain_id,
    h.strain_name,
    h.location_name,
    h.harvest_start_date,
    h.harvest_end_date,
    h.total_wet_weight,
    h.total_dry_weight,
    h.total_waste_weight,
    h.status,
    h.metrc_sync_status,
    h.created_at,
    u.first_name || ' ' || u.last_name AS created_by_name
FROM public.harvests h
LEFT JOIN public.sites s ON h.site_id = s.site_id
LEFT JOIN public.users u ON h.created_by_user_id = u.user_id;

COMMENT ON VIEW analytics.vw_harvests_flat IS 'Flattened view of harvests for reporting, respecting RLS';

-- ============================================================================
-- TASKS VIEW
-- ============================================================================
CREATE OR REPLACE VIEW analytics.vw_tasks_flat WITH (security_invoker = true) AS
SELECT
    t.task_id,
    t.site_id,
    s.site_name,
    t.title,
    t.description,
    t.task_type,
    t.status,
    t.priority,
    t.due_date,
    t.completed_at,
    t.created_at,
    uc.first_name || ' ' || uc.last_name AS created_by_name,
    ua.first_name || ' ' || ua.last_name AS assigned_to_name
FROM public.tasks t
LEFT JOIN public.sites s ON t.site_id = s.site_id
LEFT JOIN public.users uc ON t.created_by_user_id = uc.user_id
LEFT JOIN public.users ua ON t.assigned_to_user_id = ua.user_id;

COMMENT ON VIEW analytics.vw_tasks_flat IS 'Flattened view of tasks for reporting, respecting RLS';

-- ============================================================================
-- PERMISSIONS
-- ============================================================================
-- Grant access to the app role
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'harvestry_app') THEN
        GRANT SELECT ON analytics.vw_harvests_flat TO harvestry_app;
        GRANT SELECT ON analytics.vw_tasks_flat TO harvestry_app;
        GRANT SELECT, INSERT, UPDATE, DELETE ON analytics.reports TO harvestry_app;
        GRANT SELECT, INSERT, UPDATE, DELETE ON analytics.dashboards TO harvestry_app;
        GRANT SELECT, INSERT, UPDATE, DELETE ON analytics.shares TO harvestry_app;
        GRANT USAGE ON SCHEMA analytics TO harvestry_app;
    END IF;
END $$;




