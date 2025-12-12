-- Migration: Enable RLS for Sales tables
-- Version: 002
-- Description: Applies site-scoped RLS to sales orders, allocations, and shipments

-- Enable RLS
ALTER TABLE sales_orders ENABLE ROW LEVEL SECURITY;
ALTER TABLE sales_order_lines ENABLE ROW LEVEL SECURITY;
ALTER TABLE sales_allocations ENABLE ROW LEVEL SECURITY;
ALTER TABLE shipments ENABLE ROW LEVEL SECURITY;
ALTER TABLE shipment_packages ENABLE ROW LEVEL SECURITY;

-- Policies (site-scoped via app.current_site_id)
CREATE POLICY sales_orders_site_isolation ON sales_orders
    FOR ALL
    USING (site_id = COALESCE(current_setting('app.current_site_id', true)::uuid, '00000000-0000-0000-0000-000000000000'::uuid));

CREATE POLICY sales_order_lines_site_isolation ON sales_order_lines
    FOR ALL
    USING (site_id = COALESCE(current_setting('app.current_site_id', true)::uuid, '00000000-0000-0000-0000-000000000000'::uuid));

CREATE POLICY sales_allocations_site_isolation ON sales_allocations
    FOR ALL
    USING (site_id = COALESCE(current_setting('app.current_site_id', true)::uuid, '00000000-0000-0000-0000-000000000000'::uuid));

CREATE POLICY shipments_site_isolation ON shipments
    FOR ALL
    USING (site_id = COALESCE(current_setting('app.current_site_id', true)::uuid, '00000000-0000-0000-0000-000000000000'::uuid));

CREATE POLICY shipment_packages_site_isolation ON shipment_packages
    FOR ALL
    USING (site_id = COALESCE(current_setting('app.current_site_id', true)::uuid, '00000000-0000-0000-0000-000000000000'::uuid));

-- Grants for app role(s)
GRANT SELECT, INSERT, UPDATE, DELETE ON sales_orders TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON sales_order_lines TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON sales_allocations TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON shipments TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON shipment_packages TO harvestry_app;

