# RLS Policy Audit

**Audit Date:** December 4, 2025  
**Auditor:** AI Agent  
**Status:** Complete

---

## Executive Summary

Row-Level Security (RLS) is implemented across the database but uses **inconsistent patterns** that need to be standardized before production deployment. There are three different RLS approaches in use, and some reference `auth.uid()` which is a Supabase-specific function that won't work on AWS RDS.

### Critical Findings

| Finding | Severity | Impact |
|---------|----------|--------|
| Inconsistent session variable names | High | May cause access issues |
| `auth.uid()` references in telemetry | High | Won't work on AWS RDS |
| Missing RLS on some tables | Medium | Potential data leakage |
| No admin bypass on some policies | Low | Operational friction |

---

## 1. Session Variable Patterns Used

The schema uses multiple session variable patterns:

### Pattern A: User ID from Session (Most Common)
```sql
current_setting('app.current_user_id', TRUE)::UUID
```
**Used by:** Identity tables, spatial tables, tasks, messaging

### Pattern B: Direct Site ID Match
```sql
current_setting('app.site_id', TRUE)::UUID
```
**Used by:** Irrigation tables, FRP-06 tables

### Pattern C: Alternative Site ID Naming
```sql
current_setting('app.current_site_id', TRUE)::UUID
```
**Used by:** METRC tables (inconsistent with Pattern B)

### Pattern D: Supabase Auth Function
```sql
auth.uid()
```
**Used by:** Telemetry tables
**⚠️ CRITICAL:** This won't work on AWS RDS - requires migration

---

## 2. RLS Coverage by Domain

### 2.1 Identity Domain

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `users` | ✅ | A | Self-access + admin |
| `roles` | ✅ | Special | Read-all, admin-modify |
| `organizations` | ✅ | A | Via user_sites |
| `sites` | ✅ | A | Via user_sites |
| `user_sites` | ✅ | A | Self + admin |
| `badges` | ✅ | A | Site-scoped |
| `sessions` | ✅ | A | Self + admin |
| `abac_permissions` | ✅ | Special | Admin-only |
| `authorization_audit` | ✅ | A | Site-scoped, immutable |
| `two_person_approvals` | ✅ | A | Site-scoped |

### 2.2 Training/SOP Domain

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `sops` | ✅ | Special | Read-all, admin-modify |
| `training_modules` | ✅ | Special | Read-all, admin-modify |
| `quizzes` | ✅ | Special | Read-all, admin-modify |
| `training_assignments` | ✅ | A | User-scoped |
| `sop_signoffs` | ✅ | A | User-scoped |
| `task_gating_requirements` | ✅ | Special | Read-all, admin-modify |

### 2.3 Spatial Domain

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `rooms` | ✅ | A | Site-scoped |
| `inventory_locations` | ✅ | A | Site-scoped |

### 2.4 Equipment Domain

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `equipment_type_registry` | ✅ | A | Org-scoped via sites |
| `equipment` | ✅ | A | Site-scoped |
| `equipment_channels` | ✅ | A | Via equipment |
| `equipment_calibrations` | ✅ | A | Via equipment |
| `valve_zone_mappings` | ✅ | A | Site-scoped |
| `fault_reason_codes` | ❌ | None | Reference data (OK) |

### 2.5 Genetics Domain

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `genetics.genetics` | ✅ | Helper | `can_access_site()` |
| `genetics.phenotypes` | ✅ | Helper | `can_access_site()` |
| `genetics.strains` | ✅ | Helper | `can_access_site()` |
| `genetics.batches` | ✅ | Helper | `can_access_site()` |
| `genetics.batch_stage_definitions` | ✅ | Helper | `can_access_site()` |
| `genetics.batch_stage_transitions` | ✅ | Helper | `can_access_site()` |
| `genetics.batch_stage_history` | ✅ | Helper | `can_access_batch()` |
| `genetics.batch_events` | ✅ | Helper | `can_access_site()` |
| `genetics.batch_relationships` | ✅ | Helper | `can_access_site()` |
| `genetics.batch_code_rules` | ✅ | Helper | `can_access_site()` |
| `genetics.mother_plants` | ✅ | Helper | `can_access_site()` |
| `genetics.mother_health_logs` | ✅ | Helper | `can_access_site()` |
| `genetics.mother_propagation_events` | ✅ | Helper | `can_access_site()` |
| `genetics.propagation_settings` | ✅ | Helper | `can_access_site()` |
| `genetics.propagation_override_requests` | ✅ | Helper | `can_access_site()` |

**Note:** Genetics uses the cleanest pattern with helper functions.

### 2.6 Tasks/Workflow Domain

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `tasks` | ✅ | B | Site ID match |
| `task_state_history` | ✅ | B | Via tasks |
| `task_dependencies` | ✅ | B | Via tasks |
| `task_watchers` | ✅ | B | Via tasks |
| `task_time_entries` | ✅ | B | Via tasks |
| `task_required_sops` | ✅ | B | Via tasks |
| `task_required_training` | ✅ | B | Via tasks |

### 2.7 Messaging Domain

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `conversations` | ✅ | B | Site-scoped |
| `conversation_participants` | ✅ | B | Via conversations |
| `messages` | ✅ | B | Via conversations |
| `message_attachments` | ✅ | B | Via messages |
| `message_read_receipts` | ✅ | B | Via messages |

### 2.8 Slack Integration (tasks schema)

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `tasks.slack_workspaces` | ✅ | B | Site-scoped |
| `tasks.slack_channel_mappings` | ✅ | B | Site-scoped |
| `tasks.slack_message_bridge_log` | ✅ | B | Site-scoped |
| `tasks.slack_notification_queue` | ✅ | B | Site-scoped |

### 2.9 Telemetry Domain ⚠️

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `sensor_streams` | ✅ | D | **Uses auth.uid()** |
| `sensor_readings` | ✅ | D | **Uses auth.uid()** |
| `alert_rules` | ✅ | D | **Uses auth.uid()** |
| `alert_instances` | ✅ | D | **Uses auth.uid()** |
| `ingestion_sessions` | ✅ | D | **Uses auth.uid()** |
| `ingestion_errors` | ✅ | D | **Uses auth.uid()** |

**⚠️ CRITICAL:** All telemetry RLS policies reference `auth.uid()` and `auth.get_user_site_ids()` which are Supabase-specific functions. These must be migrated.

### 2.10 Irrigation Domain

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `irrigation_step_runs` | ✅ | A | Site-scoped + service |
| `irrigation_settings` | ✅ | B | Site-scoped |
| `zone_emitter_configurations` | ✅ | B | Site-scoped |
| `queued_irrigation_events` | ✅ | B | Site-scoped |

### 2.11 METRC/Compliance Domain ⚠️

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `plants` | ✅ | C | **Uses app.current_site_id** |
| `plant_events` | ✅ | C | **Uses app.current_site_id** |
| `harvests` | ✅ | C | **Uses app.current_site_id** |
| `harvest_plants` | ✅ | C | Via harvests |
| `harvest_waste` | ✅ | C | Via harvests |
| `packages` | ✅ | C | **Uses app.current_site_id** |
| `package_adjustments` | ✅ | C | Via packages |
| `package_remediations` | ✅ | C | Via packages |
| `items` | ✅ | C | **Uses app.current_site_id** |
| `lab_test_batches` | ✅ | C | **Uses app.current_site_id** |
| `lab_test_results` | ✅ | C | Via lab_test_batches |
| `processing_job_types` | ✅ | C | **Uses app.current_site_id** |
| `processing_jobs` | ✅ | C | **Uses app.current_site_id** |
| `processing_job_inputs` | ✅ | C | Via processing_jobs |
| `processing_job_outputs` | ✅ | C | Via processing_jobs |
| `metrc_sync_queue` | ✅ | C | **Uses app.current_site_id** |
| `metrc_sync_events` | ✅ | C | **Uses app.current_site_id** |
| `metrc_sync_errors` | ✅ | C | **Uses app.current_site_id** |
| `metrc_inbound_cache` | ✅ | C | **Uses app.current_site_id** |
| `metrc_tags` | ✅ | C | **Uses app.current_site_id** |

**⚠️ WARNING:** METRC tables use `app.current_site_id` while other tables use `app.site_id`. This inconsistency could cause issues.

### 2.12 Infrastructure Tables

| Table | RLS Enabled | Pattern | Notes |
|-------|-------------|---------|-------|
| `audit_trail` | ✅ | Special | Read-only, insert via function |
| `audit_verification_log` | ❌ | None | System table |
| `outbox_messages` | ✅ | A | Service account only |
| `dead_letter_queue` | ✅ | A | Service account only |
| `outbox_processing_metrics` | ❌ | None | Metrics table |

---

## 3. Session Variable Standardization Required

### Current State

| Variable | Tables Using | Notes |
|----------|--------------|-------|
| `app.current_user_id` | Most tables | Primary pattern |
| `app.user_role` | All | For admin bypass |
| `app.site_id` | Tasks, messaging, irrigation | Newer tables |
| `app.current_site_id` | METRC tables | Inconsistent naming |

### Recommended Standard

| Variable | Purpose |
|----------|---------|
| `app.current_user_id` | Authenticated user's UUID |
| `app.user_role` | User's role for bypass checks |
| `app.site_id` | Current site context (optional) |

**Action Required:** Update METRC table policies to use `app.site_id` instead of `app.current_site_id`.

---

## 4. Migration Requirements for AWS RDS

### 4.1 Replace Supabase-Specific Functions

The telemetry domain references these Supabase functions that don't exist on AWS RDS:

```sql
-- These need to be replaced:
auth.uid()
auth.get_user_site_ids()
auth.is_admin()
```

### 4.2 Replacement Implementation

Create equivalent functions using session variables:

```sql
-- Create auth schema if not exists
CREATE SCHEMA IF NOT EXISTS auth;

-- Replace auth.uid() with session variable
CREATE OR REPLACE FUNCTION auth.uid()
RETURNS UUID AS $$
BEGIN
    RETURN NULLIF(current_setting('app.current_user_id', TRUE), '')::UUID;
EXCEPTION WHEN OTHERS THEN
    RETURN NULL;
END;
$$ LANGUAGE plpgsql STABLE SECURITY DEFINER;

-- Replace auth.get_user_site_ids()
CREATE OR REPLACE FUNCTION auth.get_user_site_ids()
RETURNS UUID[] AS $$
DECLARE
    user_sites UUID[];
    user_id UUID;
BEGIN
    user_id := auth.uid();
    IF user_id IS NULL THEN
        RETURN ARRAY[]::UUID[];
    END IF;
    
    SELECT ARRAY_AGG(site_id)
    INTO user_sites
    FROM user_sites
    WHERE user_id = user_id AND revoked_at IS NULL;
    
    RETURN COALESCE(user_sites, ARRAY[]::UUID[]);
END;
$$ LANGUAGE plpgsql STABLE SECURITY DEFINER;

-- Replace auth.is_admin()
CREATE OR REPLACE FUNCTION auth.is_admin()
RETURNS BOOLEAN AS $$
BEGIN
    RETURN current_setting('app.user_role', TRUE) IN ('admin', 'service_account');
END;
$$ LANGUAGE plpgsql STABLE;
```

---

## 5. Policy Patterns Comparison

### 5.1 Genetics Pattern (Best Practice)

The genetics schema uses helper functions which is the cleanest approach:

```sql
-- Helper function encapsulates access logic
CREATE OR REPLACE FUNCTION genetics.can_access_site(target_site UUID)
RETURNS BOOLEAN AS $$
BEGIN
    IF current_setting('app.user_role', TRUE) IN ('admin', 'service_account') THEN
        RETURN TRUE;
    END IF;
    
    RETURN EXISTS (
        SELECT 1 FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
          AND site_id = target_site
          AND revoked_at IS NULL
    );
END;
$$ LANGUAGE plpgsql STABLE;

-- Clean policy using helper
CREATE POLICY site_access ON genetics.batches
    FOR ALL USING (genetics.can_access_site(site_id));
```

### 5.2 Standard Pattern (Most Common)

```sql
CREATE POLICY site_isolation ON rooms
    FOR ALL
    USING (
        site_id IN (
            SELECT site_id FROM user_sites
            WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
              AND revoked_at IS NULL
        )
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    );
```

### 5.3 Direct Match Pattern (Simpler)

```sql
CREATE POLICY site_access ON tasks
    FOR ALL
    USING (
        site_id::text = current_setting('app.site_id', TRUE)
        OR current_setting('app.user_role', TRUE) IN ('admin', 'service_account')
    );
```

---

## 6. Recommendations

### 6.1 Critical (Must Fix Before Production)

1. **Create `auth.uid()` replacement** for AWS RDS
2. **Standardize session variable names** - use `app.site_id` not `app.current_site_id`
3. **Update telemetry RLS policies** to use session variables

### 6.2 High Priority

1. **Adopt genetics helper function pattern** across all domains for consistency
2. **Add service account bypass** to all policies that are missing it
3. **Document session variable contract** for backend middleware

### 6.3 Medium Priority

1. **Create RLS verification script** to run as part of CI/CD
2. **Add integration tests** for RLS isolation
3. **Review policies for performance** (subquery vs join patterns)

---

## 7. Backend Middleware Requirements

The .NET backend must set these session variables before database queries:

```csharp
public async Task SetRlsContext(DbConnection connection, ClaimsPrincipal user)
{
    var userId = user.FindFirst("sub")?.Value; // From Supabase JWT
    var role = user.FindFirst("role")?.Value ?? "operator";
    var siteId = user.FindFirst("site_id")?.Value;
    
    await connection.ExecuteAsync(@"
        SELECT set_config('app.current_user_id', @userId, true);
        SELECT set_config('app.user_role', @role, true);
        SELECT set_config('app.site_id', @siteId, true);
    ", new { userId, role, siteId });
}
```

---

## 8. Verification Queries

### Check RLS Status

```sql
SELECT 
    schemaname,
    tablename,
    rowsecurity
FROM pg_tables
WHERE schemaname IN ('public', 'genetics', 'tasks')
ORDER BY schemaname, tablename;
```

### List All Policies

```sql
SELECT 
    schemaname,
    tablename,
    policyname,
    cmd,
    qual
FROM pg_policies
WHERE schemaname IN ('public', 'genetics', 'tasks')
ORDER BY schemaname, tablename;
```

### Test Isolation

```sql
-- Set context as User A
SET app.current_user_id = 'user-a-uuid';
SET app.site_id = 'site-1-uuid';
SET app.user_role = 'operator';

-- Should only return Site 1 data
SELECT * FROM tasks LIMIT 10;

-- Set context as User B (different site)
SET app.current_user_id = 'user-b-uuid';
SET app.site_id = 'site-2-uuid';

-- Should only return Site 2 data
SELECT * FROM tasks LIMIT 10;
```

---

## Appendix A: Complete Policy Inventory

### Policies Using Pattern A (current_user_id lookup)

- users_self_access
- roles_read_all, roles_admin_*
- sites_member_access
- user_sites_self_access, user_sites_admin_*
- badges_site_scoped
- sessions_*
- abac_permissions_admin_only
- authz_audit_*
- two_person_site_scoped
- rooms_site_isolation
- locations_site_isolation
- equipment_*
- irrigation_step_runs_*
- outbox_messages_*
- dead_letter_queue_*

### Policies Using Pattern B (direct site_id match)

- tasks_site_access
- task_*_site_access
- conversations_site_access
- messages_site_access
- slack_*_site_access
- irrigation_settings_site_isolation
- zone_emitter_site_isolation
- queued_events_site_isolation

### Policies Using Pattern C (current_site_id)

- plants_site_isolation
- plant_events_site_isolation
- harvests_site_isolation
- packages_site_isolation
- items_site_isolation
- lab_test_*_site_isolation
- processing_*_site_isolation
- metrc_*_site_isolation

### Policies Using Pattern D (auth.uid())

- sensor_streams_*_policy
- sensor_readings_*_policy
- alert_rules_*_policy
- alert_instances_*_policy
- ingestion_sessions_*_policy
- ingestion_errors_*_policy

---

*End of RLS Policy Audit*



