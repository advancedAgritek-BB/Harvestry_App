# Database Schema Audit

**Audit Date:** December 4, 2025  
**Auditor:** AI Agent  
**Status:** Complete

---

## Executive Summary

The Harvestry database schema is well-designed with comprehensive support for multi-tenant SaaS operations. The schema uses PostgreSQL with TimescaleDB extensions for time-series data. Row-Level Security (RLS) is implemented across all tables using the `current_setting('app.current_user_id')` pattern, which is compatible with JWT-based authentication.

### Key Findings

| Metric | Count |
|--------|-------|
| Total Tables | 65+ |
| Schemas | 3 (public, genetics, tasks) |
| Tables with RLS Enabled | 60+ |
| Hypertables (TimescaleDB) | 5 |
| Materialized Views | 2 |

---

## 1. Identity Domain

### Tables

| Table | Description | RLS | Key Relationships |
|-------|-------------|-----|-------------------|
| `organizations` | Top-level business entities | ✅ | Parent of sites |
| `sites` | Physical locations/facilities | ✅ | FK to organizations |
| `users` | User accounts | ✅ | Self-referential auditing |
| `user_sites` | User-to-site assignments with roles | ✅ | FK to users, sites, roles |
| `roles` | Role definitions with permissions | ✅ | Referenced by user_sites |
| `badges` | Physical/virtual badges for auth | ✅ | FK to users, sites |
| `sessions` | Active user sessions | ✅ | FK to users, sites |

### ABAC Tables

| Table | Description | RLS |
|-------|-------------|-----|
| `abac_permissions` | Fine-grained permission definitions | ✅ |
| `authorization_audit` | Immutable authorization audit trail | ✅ |
| `two_person_approvals` | Dual-approval workflows | ✅ |

### Training & SOP Tables

| Table | Description | RLS |
|-------|-------------|-----|
| `sops` | Standard Operating Procedures | ✅ |
| `training_modules` | Training courses | ✅ |
| `quizzes` | Assessment quizzes | ✅ |
| `training_assignments` | User training tracking | ✅ |
| `sop_signoffs` | User attestations | ✅ |
| `task_gating_requirements` | Task prerequisites | ✅ |

### Audit Tables

| Table | Description | RLS |
|-------|-------------|-----|
| `audit_trail` | Tamper-evident audit log with hash chain | ✅ |
| `audit_verification_log` | Nightly verification results | ✅ |

---

## 2. Spatial Domain

### Tables

| Table | Description | RLS | Key Relationships |
|-------|-------------|-----|-------------------|
| `rooms` | Top-level room hierarchy | ✅ | FK to sites |
| `inventory_locations` | Universal location hierarchy (Zone/Rack/Bin/etc.) | ✅ | Self-referential, FK to rooms |

### Enums

- `room_type`: Veg, Flower, Mother, Clone, Dry, Cure, Extraction, Manufacturing, Vault, Custom
- `room_status`: Active, Inactive, Maintenance, Quarantine
- `location_type`: Room, Zone, SubZone, Row, Position, Rack, Shelf, Bin
- `location_status`: Active, Inactive, Full, Reserved, Quarantine

---

## 3. Equipment Domain

### Tables

| Table | Description | RLS | Key Relationships |
|-------|-------------|-----|-------------------|
| `equipment_type_registry` | Per-org equipment type definitions | ✅ | FK to organizations |
| `equipment` | Equipment inventory | ✅ | FK to sites, inventory_locations |
| `equipment_channels` | Multi-channel device support | ✅ | FK to equipment |
| `equipment_calibrations` | Calibration history | ✅ | FK to equipment |
| `valve_zone_mappings` | Valve-to-zone routing | ✅ | FK to equipment, inventory_locations |
| `fault_reason_codes` | Reference data for faults | ❌ (read-only ref) | None |

### Enums

- `core_equipment_type`: controller, sensor, actuator, injector, pump, valve, meter, ec_ph_controller, mix_tank
- `equipment_status`: Active, Inactive, Maintenance, Faulty
- `calibration_method`: Single, TwoPoint, MultiPoint
- `calibration_result`: Pass, Fail, WithinTolerance, OutOfTolerance

---

## 4. Genetics Domain (Schema: `genetics`)

### Tables

| Table | Description | RLS | Key Relationships |
|-------|-------------|-----|-------------------|
| `genetics.genetics` | Genetics master data | ✅ | FK to sites |
| `genetics.phenotypes` | Phenotype expressions | ✅ | FK to genetics |
| `genetics.strains` | Strain definitions | ✅ | FK to genetics, phenotypes |
| `genetics.batches` | Cultivation batches | ✅ | FK to strains, batch_stage_definitions |
| `genetics.batch_stage_definitions` | Lifecycle stages | ✅ | FK to sites |
| `genetics.batch_stage_transitions` | Stage transition rules | ✅ | FK to batch_stage_definitions |
| `genetics.batch_stage_history` | Stage change audit | ❌ | FK to batches |
| `genetics.batch_events` | Batch event audit | ✅ | FK to batches |
| `genetics.batch_relationships` | Parent-child batch links | ✅ | FK to batches |
| `genetics.batch_code_rules` | Batch code generation | ✅ | FK to sites |
| `genetics.mother_plants` | Mother plant registry | ✅ | FK to batches, strains |
| `genetics.mother_health_logs` | Mother health tracking | ✅ | FK to mother_plants |
| `genetics.mother_propagation_events` | Propagation records | ✅ | FK to mother_plants |
| `genetics.propagation_settings` | Site propagation limits | ✅ | FK to sites |
| `genetics.propagation_override_requests` | Override workflow | ✅ | FK to sites |

---

## 5. Plants & Harvests Domain (METRC)

### Tables

| Table | Description | RLS | Key Relationships |
|-------|-------------|-----|-------------------|
| `plants` | Individual tagged plants | ⚠️ | FK to sites |
| `plant_events` | Plant lifecycle events | ⚠️ | FK to plants |
| `harvests` | Harvest records | ⚠️ | FK to sites |
| `harvest_plants` | Plants in harvests | ⚠️ | FK to harvests |
| `harvest_waste` | Harvest waste tracking | ⚠️ | FK to harvests |

### Enums

- `plant_growth_phase`: immature, vegetative, flowering, mother, harvested, destroyed, inactive
- `plant_status`: active, on_hold, harvested, destroyed, inactive
- `plant_destroy_reason`: disease, quality_failure, regulatory_compliance, etc.
- `waste_method`: grinder, compost, incinerator, mixed_waste, other
- `harvest_type`: whole_plant, manicure
- `harvest_status`: active, on_hold, finished

⚠️ **Gap Identified**: RLS policies may be missing on plants/harvests tables (see Section 10).

---

## 6. Packages & Items Domain (METRC)

### Tables

| Table | Description | RLS | Key Relationships |
|-------|-------------|-----|-------------------|
| `packages` | Cannabis packages | ⚠️ | FK to sites |
| `package_adjustments` | Quantity adjustments | ⚠️ | FK to packages |
| `package_remediations` | Remediation records | ⚠️ | FK to packages |
| `items` | Item/product definitions | ⚠️ | FK to sites |
| `lab_tests` | Lab test results | ⚠️ | FK to packages |
| `processing_jobs` | Processing records | ⚠️ | FK to sites |

### Enums

- `package_status`: active, on_hold, in_transit, finished, inactive
- `lab_testing_state`: not_submitted, test_pending, test_passed, test_failed, not_required
- `package_type`: product, immature_plant, vegetative_plant, seeds
- `adjustment_reason`: drying, scale_variance, entry_error, moisture_loss, etc.

---

## 7. Tasks & Workflow Domain

### Tables

| Table | Description | RLS | Key Relationships |
|-------|-------------|-----|-------------------|
| `tasks` | Task records | ✅ | FK to sites, users |
| `task_state_history` | State change audit | ✅ | FK to tasks |
| `task_dependencies` | Task prerequisites | ✅ | FK to tasks |
| `task_watchers` | Task subscriptions | ✅ | FK to tasks, users |
| `task_time_entries` | Time tracking | ✅ | FK to tasks, users |
| `task_required_sops` | Required SOP links | ✅ | FK to tasks, sops |
| `task_required_training` | Required training links | ✅ | FK to tasks, training_modules |

---

## 8. Messaging Domain

### Tables

| Table | Description | RLS | Key Relationships |
|-------|-------------|-----|-------------------|
| `conversations` | Conversation threads | ✅ | FK to sites |
| `conversation_participants` | Conversation members | ✅ | FK to conversations, users |
| `messages` | Message content | ✅ | FK to conversations |
| `message_attachments` | File attachments | ✅ | FK to messages |
| `message_read_receipts` | Read tracking | ✅ | FK to messages |

### Slack Integration (Schema: `tasks`)

| Table | Description | RLS |
|-------|-------------|-----|
| `tasks.slack_workspaces` | Slack workspace connections | ✅ |
| `tasks.slack_channel_mappings` | Channel-notification mappings | ✅ |
| `tasks.slack_message_bridge_log` | Outbound message tracking | ✅ |
| `tasks.slack_notification_queue` | Notification outbox | ✅ |

---

## 9. Telemetry Domain

### Tables

| Table | Description | RLS | TimescaleDB |
|-------|-------------|-----|-------------|
| `sensor_streams` | Sensor configuration | ⚠️ | No |
| `sensor_readings` | Time-series readings | ⚠️ | Hypertable |
| `alert_rules` | Alert definitions | ⚠️ | No |
| `alert_instances` | Fired alerts | ⚠️ | No |
| `ingestion_sessions` | Device sessions | ⚠️ | No |
| `ingestion_errors` | Ingestion errors | ⚠️ | No |

### Irrigation Tables

| Table | Description | RLS | TimescaleDB |
|-------|-------------|-----|-------------|
| `irrigation_step_runs` | Irrigation execution | ✅ | Hypertable |
| `irrigation_settings` | Site flow rate config | ✅ | No |
| `zone_emitter_configurations` | Emitter specs | ✅ | No |
| `queued_irrigation_events` | Flow rate queue | ✅ | No |

### Materialized Views

- `irrigation_rollups_1h`: Hourly irrigation metrics (continuous aggregate)

---

## 10. Compliance Domain (METRC Sync)

### Tables

| Table | Description | RLS | Key Relationships |
|-------|-------------|-----|-------------------|
| `metrc_sync_queue` | Outbound sync requests | ⚠️ | FK to sites |
| `metrc_sync_events` | Sync audit log | ⚠️ | FK to sites |
| `metrc_sync_errors` | Error tracking | ⚠️ | FK to sites |
| `metrc_inbound_cache` | Cached METRC data | ⚠️ | FK to sites |
| `metrc_tags` | Tag inventory | ⚠️ | FK to sites |

---

## 11. Infrastructure Tables

### Outbox Pattern

| Table | Description | RLS |
|-------|-------------|-----|
| `outbox_messages` | Transactional outbox | ✅ |
| `dead_letter_queue` | Failed messages | ✅ |
| `outbox_processing_metrics` | Worker metrics | ❌ |

---

## 12. RLS Policy Patterns

The schema uses two primary RLS patterns:

### Pattern 1: User-based Site Access

```sql
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
        AND revoked_at IS NULL
    )
    OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
)
```

### Pattern 2: Direct Site ID Match

```sql
USING (
    site_id::text = current_setting('app.site_id', TRUE)
    OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
)
```

### Session Variables Used

| Variable | Description |
|----------|-------------|
| `app.current_user_id` | Current authenticated user UUID |
| `app.user_role` | User's role (operator, supervisor, manager, admin, service_account) |
| `app.site_id` | Current site context (optional) |

---

## 13. Gaps & Recommendations

### Critical Gaps

1. **RLS on METRC Tables**: The `plants`, `harvests`, `packages`, and METRC sync tables reference `sites(id)` but RLS policies are defined in a separate migration (`011_AddRlsPolicies.sql`) that should be verified.

2. **Telemetry RLS**: The `sensor_streams`, `sensor_readings`, and related tables have RLS defined in `004_rls_policies.sql` - needs verification.

3. **Foreign Key Inconsistency**: Some tables reference `sites(id)` while others reference `sites(site_id)` - this appears to be an inconsistency between migration sets.

### Recommendations

1. **Verify RLS Coverage**: Run audit query to ensure all tables with `site_id` have RLS enabled and policies defined.

2. **Standardize FK References**: Ensure consistent foreign key naming (`site_id` vs `id`).

3. **TimescaleDB Extensions**: Verify TimescaleDB extension is available on AWS RDS (requires manual enablement).

4. **Index Optimization**: Review indexes for production query patterns.

---

## 14. Schema Compatibility with Supabase Auth

The current RLS pattern using `current_setting('app.current_user_id')` is **compatible** with the planned architecture:

1. Supabase Auth issues JWTs with `sub` claim containing user UUID
2. Backend middleware extracts `user_id` from JWT
3. Backend sets PostgreSQL session variables before queries
4. RLS policies evaluate using session variables

**No RLS policy changes required** - the existing pattern works with JWT-based authentication.

---

## Appendix A: Complete Table Inventory

### Public Schema

```
audit_trail
audit_verification_log
organizations
sites
users
user_sites
roles
badges
sessions
abac_permissions
authorization_audit
two_person_approvals
sops
training_modules
quizzes
training_assignments
sop_signoffs
task_gating_requirements
rooms
inventory_locations
equipment_type_registry
equipment
equipment_channels
equipment_calibrations
valve_zone_mappings
fault_reason_codes
tasks
task_state_history
task_dependencies
task_watchers
task_time_entries
task_required_sops
task_required_training
conversations
conversation_participants
messages
message_attachments
message_read_receipts
outbox_messages
dead_letter_queue
outbox_processing_metrics
sensor_streams
sensor_readings
alert_rules
alert_instances
ingestion_sessions
ingestion_errors
irrigation_step_runs
irrigation_settings
zone_emitter_configurations
queued_irrigation_events
plants
plant_events
harvests
harvest_plants
harvest_waste
packages
package_adjustments
package_remediations
items
lab_tests
processing_jobs
metrc_sync_queue
metrc_sync_events
metrc_sync_errors
metrc_inbound_cache
metrc_tags
```

### Genetics Schema

```
genetics.genetics
genetics.phenotypes
genetics.strains
genetics.batches
genetics.batch_stage_definitions
genetics.batch_stage_transitions
genetics.batch_stage_history
genetics.batch_events
genetics.batch_relationships
genetics.batch_code_rules
genetics.mother_plants
genetics.mother_health_logs
genetics.mother_propagation_events
genetics.propagation_settings
genetics.propagation_override_requests
```

### Tasks Schema

```
tasks.slack_workspaces
tasks.slack_channel_mappings
tasks.slack_message_bridge_log
tasks.slack_notification_queue
```

---

*End of Database Schema Audit*







