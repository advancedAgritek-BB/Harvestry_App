-- ============================================================================
-- FRP-04: Tasks, Messaging & Slack Integration
-- Migration: Create Task Workflow Tables
-- ============================================================================
-- NOTE: Apply after FRP-01 (Identity/SOP/Training) since foreign data relies on
--       existing users/sites identifiers for FK references.
-- ============================================================================

BEGIN;

-- --------------------------------------------------------------------------
-- tasks
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS tasks (
    task_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(site_id) ON DELETE CASCADE,
    task_type SMALLINT NOT NULL,
    custom_task_type VARCHAR(100),
    title VARCHAR(200) NOT NULL,
    description TEXT,
    created_by_user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE RESTRICT,
    assigned_by_user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE RESTRICT,
    assigned_to_user_id UUID REFERENCES users(user_id) ON DELETE SET NULL,
    assigned_to_role VARCHAR(100),
    assigned_at TIMESTAMPTZ,
    status SMALLINT NOT NULL,
    priority SMALLINT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    due_date TIMESTAMPTZ,
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    cancelled_at TIMESTAMPTZ,
    cancellation_reason TEXT,
    blocking_reason TEXT,
    related_entity_type VARCHAR(100),
    related_entity_id UUID
);

CREATE INDEX IF NOT EXISTS ix_tasks_site_status ON tasks(site_id, status);
CREATE INDEX IF NOT EXISTS ix_tasks_site_assigned_user ON tasks(site_id, assigned_to_user_id)
    WHERE assigned_to_user_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_tasks_due_date ON tasks(due_date)
    WHERE due_date IS NOT NULL;

-- --------------------------------------------------------------------------
-- task_state_history
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS task_state_history (
    task_state_history_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_id UUID NOT NULL REFERENCES tasks(task_id) ON DELETE CASCADE,
    from_status SMALLINT NOT NULL,
    to_status SMALLINT NOT NULL,
    changed_by UUID NOT NULL REFERENCES users(user_id) ON DELETE RESTRICT,
    changed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    reason TEXT
);

CREATE INDEX IF NOT EXISTS ix_task_state_history_task_changed_at
    ON task_state_history(task_id, changed_at);

-- --------------------------------------------------------------------------
-- task_dependencies
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS task_dependencies (
    task_dependency_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_id UUID NOT NULL REFERENCES tasks(task_id) ON DELETE CASCADE,
    depends_on_task_id UUID NOT NULL REFERENCES tasks(task_id) ON DELETE CASCADE,
    dependency_type SMALLINT NOT NULL,
    is_blocking BOOLEAN NOT NULL DEFAULT TRUE,
    minimum_lag INTERVAL,
    CONSTRAINT task_dependency_not_self CHECK (task_id <> depends_on_task_id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_task_dependencies_unique_link
    ON task_dependencies(task_id, depends_on_task_id);

-- --------------------------------------------------------------------------
-- task_watchers
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS task_watchers (
    task_watcher_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_id UUID NOT NULL REFERENCES tasks(task_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_task_watchers_task_user
    ON task_watchers(task_id, user_id);

-- --------------------------------------------------------------------------
-- task_time_entries
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS task_time_entries (
    task_time_entry_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_id UUID NOT NULL REFERENCES tasks(task_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    started_at TIMESTAMPTZ NOT NULL,
    ended_at TIMESTAMPTZ,
    notes TEXT,
    CONSTRAINT time_entry_duration CHECK (ended_at IS NULL OR ended_at >= started_at)
);

CREATE INDEX IF NOT EXISTS ix_task_time_entries_task_user_start
    ON task_time_entries(task_id, user_id, started_at DESC);

-- --------------------------------------------------------------------------
-- task_required_sops
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS task_required_sops (
    task_required_sop_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_id UUID NOT NULL REFERENCES tasks(task_id) ON DELETE CASCADE,
    sop_id UUID NOT NULL REFERENCES sops(sop_id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_task_required_sops_unique
    ON task_required_sops(task_id, sop_id);

-- --------------------------------------------------------------------------
-- task_required_training
-- --------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS task_required_training (
    task_required_training_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    task_id UUID NOT NULL REFERENCES tasks(task_id) ON DELETE CASCADE,
    training_module_id UUID NOT NULL REFERENCES training_modules(module_id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_task_required_training_unique
    ON task_required_training(task_id, training_module_id);

-- --------------------------------------------------------------------------
-- Updated-at trigger
-- --------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION set_tasks_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tasks_set_updated_at ON tasks;
CREATE TRIGGER tasks_set_updated_at
    BEFORE UPDATE ON tasks
    FOR EACH ROW
    EXECUTE FUNCTION set_tasks_updated_at();

-- --------------------------------------------------------------------------
-- Row Level Security scaffolding (policies to be finalized post design review)
-- --------------------------------------------------------------------------
ALTER TABLE tasks ENABLE ROW LEVEL SECURITY;
ALTER TABLE task_state_history ENABLE ROW LEVEL SECURITY;
ALTER TABLE task_dependencies ENABLE ROW LEVEL SECURITY;
ALTER TABLE task_watchers ENABLE ROW LEVEL SECURITY;
ALTER TABLE task_time_entries ENABLE ROW LEVEL SECURITY;
ALTER TABLE task_required_sops ENABLE ROW LEVEL SECURITY;
ALTER TABLE task_required_training ENABLE ROW LEVEL SECURITY;

CREATE POLICY tasks_site_access ON tasks
    FOR ALL
    USING (
        COALESCE(site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), site_id::text)
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    )
    WITH CHECK (
        COALESCE(site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), site_id::text)
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    );

CREATE POLICY task_state_history_site_access ON task_state_history
    FOR ALL
    USING (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_state_history.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_state_history.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    );

CREATE POLICY task_dependencies_site_access ON task_dependencies
    FOR ALL
    USING (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_dependencies.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_dependencies.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    );

CREATE POLICY task_watchers_site_access ON task_watchers
    FOR ALL
    USING (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_watchers.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_watchers.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    );

CREATE POLICY task_time_entries_site_access ON task_time_entries
    FOR ALL
    USING (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_time_entries.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_time_entries.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    );

CREATE POLICY task_required_sops_site_access ON task_required_sops
    FOR ALL
    USING (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_required_sops.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_required_sops.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    );

CREATE POLICY task_required_training_site_access ON task_required_training
    FOR ALL
    USING (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_required_training.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    )
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM tasks t
            WHERE t.task_id = task_required_training.task_id
              AND (
                COALESCE(t.site_id::text, '') = COALESCE(current_setting('app.site_id', TRUE), t.site_id::text)
                OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
              )
        )
    );

COMMIT;
