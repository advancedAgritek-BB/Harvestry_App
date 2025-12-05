-- ============================================================================
-- FRP-03: Genetics, Strains & Batches
-- Migration: Enable Row-Level Security Policies for Genetics Schema
-- ----------------------------------------------------------------------------
-- This script creates helper functions that read the current user, role, and
-- site context from session settings (configured by the application) and
-- applies site-scoped RLS policies across all FRP-03 tables.
-- ============================================================================

-- Helper: current user role (returns NULL if not set)
CREATE OR REPLACE FUNCTION genetics.current_user_role()
RETURNS TEXT
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    role_setting TEXT;
BEGIN
    role_setting := NULLIF(current_setting('app.user_role', true), '');
    RETURN role_setting;
EXCEPTION WHEN others THEN
    RETURN NULL;
END;
$$;

-- Helper: current user id (NULL if not set or sentinel value)
CREATE OR REPLACE FUNCTION genetics.current_user_id()
RETURNS UUID
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    user_setting TEXT;
    candidate UUID;
BEGIN
    user_setting := NULLIF(current_setting('app.current_user_id', true), '');
    IF user_setting IS NULL THEN
        RETURN NULL;
    END IF;

    candidate := user_setting::UUID;
    IF candidate = '00000000-0000-0000-0000-000000000000'::UUID THEN
        RETURN NULL;
    END IF;

    RETURN candidate;
EXCEPTION WHEN others THEN
    RETURN NULL;
END;
$$;

-- Helper: current site id (NULL if not set)
CREATE OR REPLACE FUNCTION genetics.current_site_id()
RETURNS UUID
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    site_setting TEXT;
BEGIN
    site_setting := NULLIF(current_setting('app.site_id', true), '');
    IF site_setting IS NULL THEN
        RETURN NULL;
    END IF;
    RETURN site_setting::UUID;
EXCEPTION WHEN others THEN
    RETURN NULL;
END;
$$;

-- Helper: determine if caller can access a site
CREATE OR REPLACE FUNCTION genetics.can_access_site(target_site UUID)
RETURNS BOOLEAN
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    role TEXT := genetics.current_user_role();
    user_id UUID := genetics.current_user_id();
    context_site UUID := genetics.current_site_id();
BEGIN
    IF target_site IS NULL THEN
        RETURN FALSE;
    END IF;

    -- Admins and service accounts bypass site membership checks
    IF role IN ('admin', 'service_account') THEN
        RETURN TRUE;
    END IF;

    -- If context site is set it must match
    IF context_site IS NOT NULL AND context_site <> target_site THEN
        RETURN FALSE;
    END IF;

    -- Require explicit site membership for user principal
    IF user_id IS NULL THEN
        RETURN FALSE;
    END IF;

    RETURN EXISTS (
        SELECT 1
        FROM user_sites us
        WHERE us.site_id = target_site
          AND us.user_id = user_id
          AND us.revoked_at IS NULL
    );
END;
$$;

-- Helper: determine if caller can access a batch (derives site from parent row)
CREATE OR REPLACE FUNCTION genetics.can_access_batch(batch_uuid UUID)
RETURNS BOOLEAN
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    batch_site UUID;
BEGIN
    IF batch_uuid IS NULL THEN
        RETURN FALSE;
    END IF;

    SELECT b.site_id INTO batch_site
    FROM genetics.batches b
    WHERE b.id = batch_uuid;

    IF batch_site IS NULL THEN
        RETURN FALSE;
    END IF;

    RETURN genetics.can_access_site(batch_site);
END;
$$;

-- ---------------------------------------------------------------------------
-- Enable RLS and apply policies (site-scoped helper used for all tables)
-- ---------------------------------------------------------------------------

-- Genetics master data
ALTER TABLE genetics.genetics ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_genetics_select ON genetics.genetics
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_genetics_insert ON genetics.genetics
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_genetics_update ON genetics.genetics
    FOR UPDATE USING (genetics.can_access_site(site_id))
                 WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_genetics_delete ON genetics.genetics
    FOR DELETE USING (genetics.can_access_site(site_id));

ALTER TABLE genetics.phenotypes ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_phenotypes_select ON genetics.phenotypes
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_phenotypes_insert ON genetics.phenotypes
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_phenotypes_update ON genetics.phenotypes
    FOR UPDATE USING (genetics.can_access_site(site_id))
                 WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_phenotypes_delete ON genetics.phenotypes
    FOR DELETE USING (genetics.can_access_site(site_id));

ALTER TABLE genetics.strains ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_strains_select ON genetics.strains
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_strains_insert ON genetics.strains
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_strains_update ON genetics.strains
    FOR UPDATE USING (genetics.can_access_site(site_id))
                 WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_strains_delete ON genetics.strains
    FOR DELETE USING (genetics.can_access_site(site_id));

-- Batch lifecycle configuration
ALTER TABLE genetics.batch_stage_definitions ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_stage_definitions_select ON genetics.batch_stage_definitions
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_stage_definitions_insert ON genetics.batch_stage_definitions
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_stage_definitions_update ON genetics.batch_stage_definitions
    FOR UPDATE USING (genetics.can_access_site(site_id))
                 WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_stage_definitions_delete ON genetics.batch_stage_definitions
    FOR DELETE USING (genetics.can_access_site(site_id));

ALTER TABLE genetics.batch_stage_transitions ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_stage_transitions_select ON genetics.batch_stage_transitions
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_stage_transitions_insert ON genetics.batch_stage_transitions
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_stage_transitions_update ON genetics.batch_stage_transitions
    FOR UPDATE USING (genetics.can_access_site(site_id))
                 WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_stage_transitions_delete ON genetics.batch_stage_transitions
    FOR DELETE USING (genetics.can_access_site(site_id));

ALTER TABLE genetics.batch_stage_history ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_stage_history_select ON genetics.batch_stage_history
    FOR SELECT USING (genetics.can_access_batch(batch_id));
CREATE POLICY genetics_stage_history_insert ON genetics.batch_stage_history
    FOR INSERT WITH CHECK (genetics.can_access_batch(batch_id));
CREATE POLICY genetics_stage_history_delete ON genetics.batch_stage_history
    FOR DELETE USING (genetics.can_access_batch(batch_id));

-- Batches + audit trail
ALTER TABLE genetics.batches ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_batches_select ON genetics.batches
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_batches_insert ON genetics.batches
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_batches_update ON genetics.batches
    FOR UPDATE USING (genetics.can_access_site(site_id))
                 WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_batches_delete ON genetics.batches
    FOR DELETE USING (genetics.can_access_site(site_id));

ALTER TABLE genetics.batch_events ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_batch_events_select ON genetics.batch_events
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_batch_events_insert ON genetics.batch_events
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_batch_events_delete ON genetics.batch_events
    FOR DELETE USING (genetics.can_access_site(site_id));

ALTER TABLE genetics.batch_relationships ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_batch_relationships_select ON genetics.batch_relationships
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_batch_relationships_insert ON genetics.batch_relationships
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_batch_relationships_delete ON genetics.batch_relationships
    FOR DELETE USING (genetics.can_access_site(site_id));

ALTER TABLE genetics.batch_code_rules ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_batch_code_rules_select ON genetics.batch_code_rules
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_batch_code_rules_insert ON genetics.batch_code_rules
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_batch_code_rules_update ON genetics.batch_code_rules
    FOR UPDATE USING (genetics.can_access_site(site_id))
                 WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_batch_code_rules_delete ON genetics.batch_code_rules
    FOR DELETE USING (genetics.can_access_site(site_id));

-- Mother plants & health
ALTER TABLE genetics.mother_plants ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_mother_plants_select ON genetics.mother_plants
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_mother_plants_insert ON genetics.mother_plants
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_mother_plants_update ON genetics.mother_plants
    FOR UPDATE USING (genetics.can_access_site(site_id))
                 WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_mother_plants_delete ON genetics.mother_plants
    FOR DELETE USING (genetics.can_access_site(site_id));

ALTER TABLE genetics.mother_health_logs ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_mother_health_logs_select ON genetics.mother_health_logs
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_mother_health_logs_insert ON genetics.mother_health_logs
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_mother_health_logs_delete ON genetics.mother_health_logs
    FOR DELETE USING (genetics.can_access_site(site_id));

ALTER TABLE genetics.mother_propagation_events ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_mother_propagation_events_select ON genetics.mother_propagation_events
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_mother_propagation_events_insert ON genetics.mother_propagation_events
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_mother_propagation_events_delete ON genetics.mother_propagation_events
    FOR DELETE USING (genetics.can_access_site(site_id));

-- Propagation settings & overrides
ALTER TABLE genetics.propagation_settings ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_propagation_settings_select ON genetics.propagation_settings
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_propagation_settings_insert ON genetics.propagation_settings
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_propagation_settings_update ON genetics.propagation_settings
    FOR UPDATE USING (genetics.can_access_site(site_id))
                 WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_propagation_settings_delete ON genetics.propagation_settings
    FOR DELETE USING (genetics.can_access_site(site_id));

ALTER TABLE genetics.propagation_override_requests ENABLE ROW LEVEL SECURITY;
CREATE POLICY genetics_propagation_overrides_select ON genetics.propagation_override_requests
    FOR SELECT USING (genetics.can_access_site(site_id));
CREATE POLICY genetics_propagation_overrides_insert ON genetics.propagation_override_requests
    FOR INSERT WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_propagation_overrides_update ON genetics.propagation_override_requests
    FOR UPDATE USING (genetics.can_access_site(site_id))
                 WITH CHECK (genetics.can_access_site(site_id));
CREATE POLICY genetics_propagation_overrides_delete ON genetics.propagation_override_requests
    FOR DELETE USING (genetics.can_access_site(site_id));

-- ============================================================================
-- End of RLS configuration
-- ============================================================================
