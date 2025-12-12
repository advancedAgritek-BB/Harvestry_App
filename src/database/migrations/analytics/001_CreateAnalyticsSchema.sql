-- ============================================================================
-- ANALYTICS SCHEMA & TABLES
-- ============================================================================

CREATE SCHEMA IF NOT EXISTS analytics;

-- ============================================================================
-- REPORTS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS analytics.reports (
    report_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    config JSONB NOT NULL, -- Report definition (columns, filters, sorts)
    visualization_config JSONB DEFAULT '{}'::JSONB, -- Default chart settings
    is_public BOOLEAN DEFAULT FALSE, -- Visible to everyone in the system (or org if we add org_id)
    owner_id UUID NOT NULL REFERENCES public.users(user_id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID REFERENCES public.users(user_id),
    updated_by UUID REFERENCES public.users(user_id)
);

CREATE INDEX ix_reports_owner ON analytics.reports(owner_id);
CREATE INDEX ix_reports_public ON analytics.reports(is_public) WHERE is_public = TRUE;

COMMENT ON TABLE analytics.reports IS 'User-defined report configurations';
COMMENT ON COLUMN analytics.reports.config IS 'JSON definition of the query: selected fields, filters, aggregations';

-- ============================================================================
-- DASHBOARDS TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS analytics.dashboards (
    dashboard_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    layout_config JSONB DEFAULT '[]'::JSONB, -- Array of widgets with positions
    is_public BOOLEAN DEFAULT FALSE,
    owner_id UUID NOT NULL REFERENCES public.users(user_id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID REFERENCES public.users(user_id),
    updated_by UUID REFERENCES public.users(user_id)
);

CREATE INDEX ix_dashboards_owner ON analytics.dashboards(owner_id);

COMMENT ON TABLE analytics.dashboards IS 'User-defined dashboard layouts containing widgets';

-- ============================================================================
-- SHARES TABLE
-- ============================================================================
CREATE TABLE IF NOT EXISTS analytics.shares (
    share_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    resource_type VARCHAR(50) CHECK (resource_type IN ('report', 'dashboard')),
    resource_id UUID NOT NULL,
    shared_with_id UUID NOT NULL, -- user_id or role_id
    shared_with_type VARCHAR(20) CHECK (shared_with_type IN ('user', 'role')),
    permission_level VARCHAR(20) DEFAULT 'view' CHECK (permission_level IN ('view', 'edit', 'admin')),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    created_by UUID REFERENCES public.users(user_id)
);

CREATE INDEX ix_shares_resource ON analytics.shares(resource_type, resource_id);
CREATE INDEX ix_shares_target ON analytics.shares(shared_with_type, shared_with_id);

COMMENT ON TABLE analytics.shares IS 'Granular sharing permissions for analytics resources';

-- ============================================================================
-- RLS POLICIES
-- ============================================================================

-- Reports RLS
ALTER TABLE analytics.reports ENABLE ROW LEVEL SECURITY;

CREATE POLICY reports_owner_access ON analytics.reports
FOR ALL
USING (
    owner_id = current_setting('app.current_user_id', TRUE)::UUID
    OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
);

CREATE POLICY reports_public_read ON analytics.reports
FOR SELECT
USING (is_public = TRUE);

CREATE POLICY reports_shared_read ON analytics.reports
FOR SELECT
USING (
    report_id IN (
        SELECT resource_id 
        FROM analytics.shares 
        WHERE resource_type = 'report'
        AND (
            (shared_with_type = 'user' AND shared_with_id = current_setting('app.current_user_id', TRUE)::UUID)
            -- OR (shared_with_type = 'role' AND shared_with_id IN (SELECT role_id FROM user_roles...)) -- Simplified for now
        )
    )
);

-- Dashboards RLS
ALTER TABLE analytics.dashboards ENABLE ROW LEVEL SECURITY;

CREATE POLICY dashboards_owner_access ON analytics.dashboards
FOR ALL
USING (
    owner_id = current_setting('app.current_user_id', TRUE)::UUID
    OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
);

CREATE POLICY dashboards_public_read ON analytics.dashboards
FOR SELECT
USING (is_public = TRUE);

-- Shares RLS
ALTER TABLE analytics.shares ENABLE ROW LEVEL SECURITY;

CREATE POLICY shares_view_access ON analytics.shares
FOR SELECT
USING (
    created_by = current_setting('app.current_user_id', TRUE)::UUID
    OR (shared_with_type = 'user' AND shared_with_id = current_setting('app.current_user_id', TRUE)::UUID)
    OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
);

-- Audit Triggers (reusing existing function if available in public, otherwise redefine)
-- Assuming update_updated_at_column exists in public schema from previous migrations
CREATE TRIGGER reports_updated_at BEFORE UPDATE ON analytics.reports
    FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();

CREATE TRIGGER dashboards_updated_at BEFORE UPDATE ON analytics.dashboards
    FOR EACH ROW EXECUTE FUNCTION public.update_updated_at_column();





