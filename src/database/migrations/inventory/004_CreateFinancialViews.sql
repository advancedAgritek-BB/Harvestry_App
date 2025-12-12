-- Migration: Create Financial Views
-- Version: 004
-- Description: Creates views and functions for inventory valuation, COGS, and alerts

-- ============================================================================
-- INVENTORY VALUE BY CATEGORY
-- ============================================================================
CREATE OR REPLACE VIEW inventory_value_by_category AS
SELECT 
    p.site_id,
    COALESCE(p.inventory_category, 'finished_good') as inventory_category,
    COUNT(*) as package_count,
    SUM(p.quantity) as total_quantity,
    SUM(p.quantity * COALESCE(p.unit_cost, 0)) as total_value,
    AVG(p.unit_cost) as avg_unit_cost,
    SUM(p.reserved_quantity) as total_reserved,
    SUM(p.quantity - COALESCE(p.reserved_quantity, 0)) as total_available
FROM packages p
WHERE p.status = 'active'
GROUP BY p.site_id, COALESCE(p.inventory_category, 'finished_good');

COMMENT ON VIEW inventory_value_by_category IS 'Inventory value summary grouped by category (raw_material, wip, finished_good, etc.)';

-- ============================================================================
-- INVENTORY VALUE SUMMARY
-- ============================================================================
CREATE OR REPLACE VIEW inventory_value_summary AS
SELECT 
    p.site_id,
    COUNT(*) as total_packages,
    SUM(p.quantity) as total_quantity,
    SUM(p.quantity * COALESCE(p.unit_cost, 0)) as total_value,
    AVG(p.unit_cost) as avg_unit_cost,
    SUM(CASE WHEN p.inventory_category = 'raw_material' THEN p.quantity * COALESCE(p.unit_cost, 0) ELSE 0 END) as raw_material_value,
    SUM(CASE WHEN p.inventory_category = 'work_in_progress' THEN p.quantity * COALESCE(p.unit_cost, 0) ELSE 0 END) as wip_value,
    SUM(CASE WHEN p.inventory_category = 'finished_good' THEN p.quantity * COALESCE(p.unit_cost, 0) ELSE 0 END) as finished_goods_value,
    SUM(CASE WHEN p.inventory_category = 'consumable' THEN p.quantity * COALESCE(p.unit_cost, 0) ELSE 0 END) as consumable_value,
    SUM(CASE WHEN p.inventory_category = 'byproduct' THEN p.quantity * COALESCE(p.unit_cost, 0) ELSE 0 END) as byproduct_value
FROM packages p
WHERE p.status = 'active'
GROUP BY p.site_id;

COMMENT ON VIEW inventory_value_summary IS 'Total inventory value summary with breakdown by category';

-- ============================================================================
-- LOW STOCK ALERTS
-- ============================================================================
CREATE OR REPLACE VIEW low_stock_alerts AS
SELECT 
    i.site_id,
    i.id as item_id,
    i.name as item_name,
    i.sku,
    i.category as item_category,
    i.inventory_category,
    i.reorder_point,
    i.reorder_quantity,
    i.safety_stock,
    i.lead_time_days,
    COALESCE(inv.current_quantity, 0) as current_quantity,
    COALESCE(inv.reserved_quantity, 0) as reserved_quantity,
    COALESCE(inv.current_quantity, 0) - COALESCE(inv.reserved_quantity, 0) as available_quantity,
    i.reorder_point - COALESCE(inv.current_quantity, 0) as shortage,
    CASE 
        WHEN COALESCE(inv.current_quantity, 0) <= COALESCE(i.safety_stock, 0) THEN 'critical'
        WHEN COALESCE(inv.current_quantity, 0) <= i.reorder_point THEN 'low'
        ELSE 'ok'
    END as stock_status,
    i.cost_price,
    (i.reorder_point - COALESCE(inv.current_quantity, 0)) * COALESCE(i.cost_price, 0) as shortage_value
FROM items i
LEFT JOIN (
    SELECT 
        item_id,
        SUM(quantity) as current_quantity,
        SUM(reserved_quantity) as reserved_quantity
    FROM packages
    WHERE status = 'active'
    GROUP BY item_id
) inv ON inv.item_id = i.id
WHERE i.reorder_point IS NOT NULL
  AND i.status = 'active'
  AND COALESCE(inv.current_quantity, 0) < i.reorder_point;

COMMENT ON VIEW low_stock_alerts IS 'Items below reorder point requiring attention';

-- ============================================================================
-- EXPIRING INVENTORY
-- ============================================================================
CREATE OR REPLACE VIEW expiring_inventory AS
SELECT 
    p.site_id,
    p.id as package_id,
    p.package_label,
    p.item_id,
    p.item_name,
    p.item_category,
    p.inventory_category,
    p.quantity,
    p.unit_of_measure,
    p.expiration_date,
    (p.expiration_date - CURRENT_DATE) as days_until_expiry,
    p.quantity * COALESCE(p.unit_cost, 0) as value_at_risk,
    p.location_id,
    p.location_name,
    p.grade,
    p.lab_testing_state,
    CASE 
        WHEN p.expiration_date <= CURRENT_DATE THEN 'expired'
        WHEN p.expiration_date <= CURRENT_DATE + INTERVAL '7 days' THEN 'critical'
        WHEN p.expiration_date <= CURRENT_DATE + INTERVAL '14 days' THEN 'warning'
        WHEN p.expiration_date <= CURRENT_DATE + INTERVAL '30 days' THEN 'upcoming'
        ELSE 'ok'
    END as expiry_status
FROM packages p
WHERE p.status = 'active' 
  AND p.expiration_date IS NOT NULL
  AND p.expiration_date <= CURRENT_DATE + INTERVAL '30 days'
ORDER BY p.expiration_date ASC;

COMMENT ON VIEW expiring_inventory IS 'Packages expiring within 30 days with value at risk';

-- ============================================================================
-- VALUE AT RISK
-- ============================================================================
CREATE OR REPLACE VIEW value_at_risk AS
SELECT 
    site_id,
    -- Expiring inventory
    COALESCE(SUM(CASE WHEN expiration_date <= CURRENT_DATE + INTERVAL '7 days' AND expiration_date > CURRENT_DATE 
        THEN quantity * COALESCE(unit_cost, 0) ELSE 0 END), 0) as expiring_7_days,
    COALESCE(SUM(CASE WHEN expiration_date <= CURRENT_DATE + INTERVAL '30 days' AND expiration_date > CURRENT_DATE + INTERVAL '7 days' 
        THEN quantity * COALESCE(unit_cost, 0) ELSE 0 END), 0) as expiring_30_days,
    COALESCE(SUM(CASE WHEN expiration_date <= CURRENT_DATE 
        THEN quantity * COALESCE(unit_cost, 0) ELSE 0 END), 0) as expired,
    -- Held inventory
    COALESCE(SUM(CASE WHEN status = 'on_hold' 
        THEN quantity * COALESCE(unit_cost, 0) ELSE 0 END), 0) as on_hold,
    -- By hold reason
    COALESCE(SUM(CASE WHEN hold_reason_code = 'coa_failed' 
        THEN quantity * COALESCE(unit_cost, 0) ELSE 0 END), 0) as coa_failed,
    COALESCE(SUM(CASE WHEN hold_reason_code = 'contamination' 
        THEN quantity * COALESCE(unit_cost, 0) ELSE 0 END), 0) as contaminated,
    COALESCE(SUM(CASE WHEN hold_reason_code = 'quality_issue' 
        THEN quantity * COALESCE(unit_cost, 0) ELSE 0 END), 0) as quality_issues,
    COALESCE(SUM(CASE WHEN lab_testing_state = 'test_pending' 
        THEN quantity * COALESCE(unit_cost, 0) ELSE 0 END), 0) as pending_lab,
    -- Total at risk
    COALESCE(SUM(CASE 
        WHEN status = 'on_hold' 
        OR expiration_date <= CURRENT_DATE + INTERVAL '30 days'
        OR lab_testing_state = 'test_failed'
        THEN quantity * COALESCE(unit_cost, 0) ELSE 0 END), 0) as total_at_risk
FROM packages
WHERE status IN ('active', 'on_hold')
GROUP BY site_id;

COMMENT ON VIEW value_at_risk IS 'Inventory value at risk due to expiration, holds, or quality issues';

-- ============================================================================
-- COGS BY PERIOD
-- ============================================================================
CREATE OR REPLACE VIEW cogs_by_period AS
SELECT 
    m.site_id,
    DATE_TRUNC('day', m.created_at)::date as period_date,
    DATE_TRUNC('week', m.created_at)::date as period_week,
    DATE_TRUNC('month', m.created_at)::date as period_month,
    m.movement_type,
    COUNT(*) as movement_count,
    SUM(m.quantity) as total_quantity,
    SUM(COALESCE(m.total_cost, m.quantity * COALESCE(m.unit_cost, 0))) as total_cost
FROM inventory_movements m
WHERE m.movement_type IN ('ship', 'process_input', 'destruction')
  AND m.status = 'completed'
GROUP BY m.site_id, 
    DATE_TRUNC('day', m.created_at)::date,
    DATE_TRUNC('week', m.created_at)::date,
    DATE_TRUNC('month', m.created_at)::date,
    m.movement_type;

COMMENT ON VIEW cogs_by_period IS 'Cost of goods sold aggregated by day/week/month for trend analysis';

-- ============================================================================
-- INVENTORY AGING ANALYSIS
-- ============================================================================
CREATE OR REPLACE VIEW inventory_aging AS
SELECT 
    p.site_id,
    p.id as package_id,
    p.package_label,
    p.item_id,
    p.item_name,
    p.inventory_category,
    p.quantity,
    p.unit_cost,
    p.quantity * COALESCE(p.unit_cost, 0) as total_value,
    p.packaged_date,
    p.received_date,
    COALESCE(p.packaged_date, p.received_date, p.created_at::date) as age_start_date,
    CURRENT_DATE - COALESCE(p.packaged_date, p.received_date, p.created_at::date) as age_days,
    CASE 
        WHEN CURRENT_DATE - COALESCE(p.packaged_date, p.received_date, p.created_at::date) <= 30 THEN '0-30'
        WHEN CURRENT_DATE - COALESCE(p.packaged_date, p.received_date, p.created_at::date) <= 60 THEN '31-60'
        WHEN CURRENT_DATE - COALESCE(p.packaged_date, p.received_date, p.created_at::date) <= 90 THEN '61-90'
        WHEN CURRENT_DATE - COALESCE(p.packaged_date, p.received_date, p.created_at::date) <= 180 THEN '91-180'
        ELSE '180+'
    END as age_bucket
FROM packages p
WHERE p.status = 'active';

COMMENT ON VIEW inventory_aging IS 'Inventory age analysis with buckets for FIFO management';

-- ============================================================================
-- AGING SUMMARY BY BUCKET
-- ============================================================================
CREATE OR REPLACE VIEW inventory_aging_summary AS
SELECT 
    site_id,
    inventory_category,
    age_bucket,
    COUNT(*) as package_count,
    SUM(quantity) as total_quantity,
    SUM(total_value) as total_value,
    AVG(age_days) as avg_age_days
FROM inventory_aging
GROUP BY site_id, inventory_category, age_bucket
ORDER BY site_id, inventory_category, 
    CASE age_bucket 
        WHEN '0-30' THEN 1 
        WHEN '31-60' THEN 2 
        WHEN '61-90' THEN 3 
        WHEN '91-180' THEN 4 
        ELSE 5 
    END;

COMMENT ON VIEW inventory_aging_summary IS 'Inventory aging summary by category and age bucket';

-- ============================================================================
-- INVENTORY TURNOVER
-- ============================================================================
CREATE OR REPLACE VIEW inventory_turnover AS
WITH period_cogs AS (
    SELECT 
        site_id,
        SUM(CASE WHEN created_at >= CURRENT_DATE - INTERVAL '30 days' THEN COALESCE(total_cost, quantity * COALESCE(unit_cost, 0)) ELSE 0 END) as cogs_30d,
        SUM(CASE WHEN created_at >= CURRENT_DATE - INTERVAL '90 days' THEN COALESCE(total_cost, quantity * COALESCE(unit_cost, 0)) ELSE 0 END) as cogs_90d,
        SUM(CASE WHEN created_at >= CURRENT_DATE - INTERVAL '365 days' THEN COALESCE(total_cost, quantity * COALESCE(unit_cost, 0)) ELSE 0 END) as cogs_365d
    FROM inventory_movements
    WHERE movement_type IN ('ship', 'process_input', 'destruction')
      AND status = 'completed'
    GROUP BY site_id
),
current_inventory AS (
    SELECT 
        site_id,
        SUM(quantity * COALESCE(unit_cost, 0)) as avg_inventory_value
    FROM packages
    WHERE status = 'active'
    GROUP BY site_id
)
SELECT 
    ci.site_id,
    ci.avg_inventory_value,
    COALESCE(pc.cogs_30d, 0) as cogs_last_30_days,
    COALESCE(pc.cogs_90d, 0) as cogs_last_90_days,
    COALESCE(pc.cogs_365d, 0) as cogs_last_year,
    -- Annualized turnover from 30-day COGS
    CASE WHEN ci.avg_inventory_value > 0 
        THEN (COALESCE(pc.cogs_30d, 0) * 12) / ci.avg_inventory_value 
        ELSE 0 END as turnover_rate_annualized,
    -- Days of inventory on hand (based on 30-day COGS run rate)
    CASE WHEN COALESCE(pc.cogs_30d, 0) > 0 
        THEN (ci.avg_inventory_value / (COALESCE(pc.cogs_30d, 0) / 30))::integer 
        ELSE NULL END as days_on_hand
FROM current_inventory ci
LEFT JOIN period_cogs pc ON pc.site_id = ci.site_id;

COMMENT ON VIEW inventory_turnover IS 'Inventory turnover metrics including annualized rate and days on hand';

-- ============================================================================
-- MOVEMENT SUMMARY BY TYPE
-- ============================================================================
CREATE OR REPLACE VIEW movement_summary_by_type AS
SELECT 
    site_id,
    movement_type,
    COUNT(*) FILTER (WHERE created_at >= CURRENT_DATE) as today_count,
    COUNT(*) FILTER (WHERE created_at >= CURRENT_DATE - INTERVAL '7 days') as week_count,
    COUNT(*) FILTER (WHERE created_at >= CURRENT_DATE - INTERVAL '30 days') as month_count,
    SUM(quantity) FILTER (WHERE created_at >= CURRENT_DATE - INTERVAL '30 days') as month_quantity,
    SUM(COALESCE(total_cost, quantity * COALESCE(unit_cost, 0))) FILTER (WHERE created_at >= CURRENT_DATE - INTERVAL '30 days') as month_value,
    COUNT(*) FILTER (WHERE status = 'pending') as pending_count,
    COUNT(*) FILTER (WHERE status = 'failed') as failed_count
FROM inventory_movements
GROUP BY site_id, movement_type;

COMMENT ON VIEW movement_summary_by_type IS 'Movement counts and values by type for dashboard';

-- ============================================================================
-- HOLDS SUMMARY
-- ============================================================================
CREATE OR REPLACE VIEW holds_summary AS
SELECT 
    site_id,
    COUNT(*) as total_holds,
    COUNT(*) FILTER (WHERE hold_reason_code = 'coa_failed') as coa_failed_count,
    COUNT(*) FILTER (WHERE hold_reason_code = 'coa_pending') as coa_pending_count,
    COUNT(*) FILTER (WHERE hold_reason_code = 'contamination') as contamination_count,
    COUNT(*) FILTER (WHERE hold_reason_code = 'quality_issue') as quality_issue_count,
    COUNT(*) FILTER (WHERE hold_reason_code = 'regulatory') as regulatory_count,
    COUNT(*) FILTER (WHERE requires_two_person_release = TRUE) as requires_approval_count,
    SUM(quantity * COALESCE(unit_cost, 0)) as total_hold_value,
    MIN(hold_placed_at) as oldest_hold_date
FROM packages
WHERE status = 'on_hold'
GROUP BY site_id;

COMMENT ON VIEW holds_summary IS 'Summary of inventory holds by reason code';




