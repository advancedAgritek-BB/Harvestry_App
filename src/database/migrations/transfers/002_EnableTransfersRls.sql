-- Migration: Enable RLS for Transfers tables
-- Version: 002
-- Description: Applies site-scoped RLS to transfers/manifests/receipts.

ALTER TABLE outbound_transfers ENABLE ROW LEVEL SECURITY;
ALTER TABLE outbound_transfer_packages ENABLE ROW LEVEL SECURITY;
ALTER TABLE transport_manifests ENABLE ROW LEVEL SECURITY;
ALTER TABLE inbound_transfer_receipts ENABLE ROW LEVEL SECURITY;
ALTER TABLE inbound_transfer_receipt_lines ENABLE ROW LEVEL SECURITY;
ALTER TABLE transfer_events ENABLE ROW LEVEL SECURITY;

CREATE POLICY outbound_transfers_site_isolation ON outbound_transfers
    FOR ALL
    USING (site_id = COALESCE(current_setting('app.current_site_id', true)::uuid, '00000000-0000-0000-0000-000000000000'::uuid));

CREATE POLICY outbound_transfer_packages_site_isolation ON outbound_transfer_packages
    FOR ALL
    USING (site_id = COALESCE(current_setting('app.current_site_id', true)::uuid, '00000000-0000-0000-0000-000000000000'::uuid));

CREATE POLICY transport_manifests_site_isolation ON transport_manifests
    FOR ALL
    USING (site_id = COALESCE(current_setting('app.current_site_id', true)::uuid, '00000000-0000-0000-0000-000000000000'::uuid));

CREATE POLICY inbound_transfer_receipts_site_isolation ON inbound_transfer_receipts
    FOR ALL
    USING (site_id = COALESCE(current_setting('app.current_site_id', true)::uuid, '00000000-0000-0000-0000-000000000000'::uuid));

CREATE POLICY inbound_transfer_receipt_lines_site_isolation ON inbound_transfer_receipt_lines
    FOR ALL
    USING (site_id = COALESCE(current_setting('app.current_site_id', true)::uuid, '00000000-0000-0000-0000-000000000000'::uuid));

CREATE POLICY transfer_events_site_isolation ON transfer_events
    FOR ALL
    USING (site_id = COALESCE(current_setting('app.current_site_id', true)::uuid, '00000000-0000-0000-0000-000000000000'::uuid));

GRANT SELECT, INSERT, UPDATE, DELETE ON outbound_transfers TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON outbound_transfer_packages TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON transport_manifests TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON inbound_transfer_receipts TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON inbound_transfer_receipt_lines TO harvestry_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON transfer_events TO harvestry_app;

