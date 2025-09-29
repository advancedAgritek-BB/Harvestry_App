# Data Model Baseline & Migrations

## Baseline Entities (site-scoped unless noted)
Identity: users, user_roles, user_sites, badge_credentials, device_sessions, audit_logs
Spatial: sites, rooms, zones, racks, bins, inventory_locations
Equipment & Telemetry: equipment, equipment_links, sensor_streams, sensor_readings (hypertables), rollups
Cultivation: genetics, phenotypes, mother_plants, clone_trays, clone_cells, clone_batches, propagation_logs, clone_metrics, batches, plant_movements
Tasking & Messaging: task_blueprints, tasks, task_assignments, task_dependencies, task_checklists, task_form_responses, task_approvals, task_delegations, task_events; conversations, messages (+ Slack mapping)
Environment & Alerts: environment_targets, environment_overrides, alert_thresholds, alerts
Irrigation & Fertigation: mix_tanks, injector_channels, nutrient_products, stock_solution_lots, feed_recipes, feed_recipe_versions, feed_recipe_components, feed_targets, irrigation_groups, irrigation_programs, irrigation_schedules, irrigation_runs, irrigation_step_runs, control_loops, interlocks
Inventory & Labels: inventory_lots, inventory_balances, inventory_movements, inventory_adjustments, lot_relationships, uom_definitions, uom_conversions, barcode_settings, label_templates
Processing: process_definitions, process_runs, process_steps, labor_logs, waste_events
Compliance & Labs: compliance_integrations, sync_queue, sync_events, destruction_events, two_person_signoffs, holds, labs, lab_orders, lab_results, batch_coa_attachments, jurisdiction_rules
AI & Sustainability & PdM: ai_predictions, ai_feedback, feature_logs, crop_coefficients, et0_inputs, energy_meters, energy_readings, emissions_factors, sustainability_reports, equipment_usage_counters, equipment_failures, maintenance_predictions, work_orders
Accounting & Reporting: accounting_integrations, qbo_*_map, accounting_queue, accounting_events, saved_reports, report_runs, email_schedules
Queues & Outbox: outbox, sync_queue, accounting_queue

## Migrations Strategy
- Zero-downtime: expand → backfill → flip → contract.
- Hypertables for time-series (TimescaleDB) with continuous aggregates (1m/5m/1h).
- Partitioning by (site_id, month); BRIN and B-Tree indices per access patterns.
- Idempotent writers keyed by (device_id, metric, sequence/timestamp).
