# Row-Level Security (RLS) & Attribute-Based Access Control (ABAC)

**Track A Implementation** - Site-scoped security by default with ABAC overlays for high-risk operations

## 🎯 Overview

Harvestry ERP implements a **defense-in-depth** security model combining:

1. **Row-Level Security (RLS)** - Site-scoped data isolation at the database level
2. **Attribute-Based Access Control (ABAC)** - Role and context-based gates for high-risk actions
3. **Audit Logging** - Tamper-evident trail of all access and modifications

**Security Principle:** Every table is site-scoped by default unless explicitly designed for cross-site access.

---

## 🔒 RLS Architecture

### Session Context

All database sessions must set these PostgreSQL session variables:

```sql
-- Set by application on each connection
SET app.current_user_id = 'uuid-of-authenticated-user';
SET app.user_role = 'operator' | 'manager' | 'admin' | 'service_account';
SET app.site_id = 'uuid-of-current-site'; -- Optional for single-site users
```

### Policy Hierarchy

1. **Site Isolation** (Primary) - Users can only access data for sites they're assigned to
2. **Service Account Bypass** - Allows background workers and system operations
3. **ABAC Gates** (Overlay) - Additional checks for high-risk operations

---

## 📋 Standard RLS Pattern

Apply this pattern to **every table** that contains site-scoped data:

```sql
-- Enable RLS on table
ALTER TABLE your_table ENABLE ROW LEVEL SECURITY;

-- Policy 1: Site-scoped access for regular users
CREATE POLICY your_table_site_isolation ON your_table
FOR ALL
USING (
    site_id IN (
        SELECT site_id 
        FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
    )
);

-- Policy 2: Service account bypass (for background workers)
CREATE POLICY your_table_service_account ON your_table
FOR ALL
USING (current_setting('app.user_role', TRUE) = 'service_account');

-- Policy 3 (Optional): Read-only cross-site for specific roles
CREATE POLICY your_table_cross_site_readonly ON your_table
FOR SELECT
USING (
    current_setting('app.user_role', TRUE) IN ('admin', 'executive')
);
```

---

## 🛡️ ABAC High-Risk Operations

### What Requires ABAC Gates?

Operations that could cause:
- **Regulatory violations** (e.g., destroying inventory without COA)
- **Financial loss** (e.g., approving high-value invoices)
- **Safety incidents** (e.g., overriding interlocks)
- **Data loss** (e.g., bulk deletions)

### ABAC Implementation Pattern

```sql
-- Function to check ABAC permission
CREATE OR REPLACE FUNCTION check_abac_permission(
    p_user_id UUID,
    p_action VARCHAR,
    p_resource_type VARCHAR,
    p_site_id UUID,
    p_context JSONB DEFAULT '{}'::JSONB
)
RETURNS BOOLEAN AS $$
DECLARE
    v_user_role VARCHAR;
    v_has_permission BOOLEAN := FALSE;
BEGIN
    -- Get user's role for this site
    SELECT role INTO v_user_role
    FROM user_sites
    WHERE user_id = p_user_id AND site_id = p_site_id;
    
    -- Check permission based on action and role
    SELECT EXISTS (
        SELECT 1 FROM abac_permissions
        WHERE action = p_action
          AND resource_type = p_resource_type
          AND role = v_user_role
          AND (conditions IS NULL OR conditions @> p_context)
    ) INTO v_has_permission;
    
    -- Log the authorization check
    INSERT INTO authorization_audit (
        user_id,
        site_id,
        action,
        resource_type,
        granted,
        context
    ) VALUES (
        p_user_id,
        p_site_id,
        p_action,
        p_resource_type,
        v_has_permission,
        p_context
    );
    
    RETURN v_has_permission;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;
```

### High-Risk Actions

| Action | Resource | Required Role(s) | Additional Conditions |
|--------|----------|------------------|----------------------|
| `approve_destruction` | `inventory_lot` | `manager`, `compliance_officer` | Two-person approval |
| `override_interlock` | `irrigation_program` | `manager`, `head_grower` | Must document reason |
| `enable_closed_loop` | `control_loop` | `admin`, `vp_product` | Promotion checklist completed |
| `approve_invoice` | `invoice` | `finance_manager` | > $10,000 requires dual approval |
| `revoke_license` | `user` | `admin` | Must document termination reason |
| `bulk_delete` | `any` | `admin` | Soft delete preferred |
| `modify_audit_log` | `audit_trail` | FORBIDDEN | Audit logs are immutable |

---

## 🔧 RLS Policy Templates

### Template 1: Site-Scoped Table (Standard)

```sql
-- Example: tasks table
ALTER TABLE tasks ENABLE ROW LEVEL SECURITY;

CREATE POLICY tasks_site_isolation ON tasks
FOR ALL
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
    )
);

CREATE POLICY tasks_service_account ON tasks
FOR ALL
USING (current_setting('app.user_role', TRUE) = 'service_account');
```

### Template 2: User-Scoped Table (Personal Data)

```sql
-- Example: user_preferences table
ALTER TABLE user_preferences ENABLE ROW LEVEL SECURITY;

CREATE POLICY user_preferences_owner ON user_preferences
FOR ALL
USING (user_id = current_setting('app.current_user_id', TRUE)::UUID);

CREATE POLICY user_preferences_service_account ON user_preferences
FOR ALL
USING (current_setting('app.user_role', TRUE) = 'service_account');
```

### Template 3: Cross-Site with Role Filter

```sql
-- Example: organizations table (admins can see all)
ALTER TABLE organizations ENABLE ROW LEVEL SECURITY;

-- Regular users see only their org
CREATE POLICY organizations_member ON organizations
FOR SELECT
USING (
    id IN (
        SELECT org_id FROM user_organizations
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
    )
);

-- Admins see all orgs
CREATE POLICY organizations_admin ON organizations
FOR ALL
USING (current_setting('app.user_role', TRUE) = 'admin');
```

### Template 4: Time-Based Access

```sql
-- Example: archived_data table (read-only after archive)
ALTER TABLE archived_data ENABLE ROW LEVEL SECURITY;

CREATE POLICY archived_data_readonly ON archived_data
FOR SELECT
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
    )
);

-- No INSERT/UPDATE/DELETE allowed once archived
CREATE POLICY archived_data_no_modify ON archived_data
FOR INSERT, UPDATE, DELETE
USING (FALSE);
```

### Template 5: ABAC-Gated Operations

```sql
-- Example: inventory_lot_destructions (requires two-person approval)
ALTER TABLE inventory_lot_destructions ENABLE ROW LEVEL SECURITY;

-- Site isolation for reads
CREATE POLICY inventory_destructions_read ON inventory_lot_destructions
FOR SELECT
USING (
    site_id IN (
        SELECT site_id FROM user_sites
        WHERE user_id = current_setting('app.current_user_id', TRUE)::UUID
    )
);

-- ABAC gate for creates (must have permission)
CREATE POLICY inventory_destructions_create ON inventory_lot_destructions
FOR INSERT
WITH CHECK (
    check_abac_permission(
        current_setting('app.current_user_id', TRUE)::UUID,
        'approve_destruction',
        'inventory_lot',
        site_id,
        jsonb_build_object('lot_id', lot_id, 'reason', reason)
    )
);

-- No updates/deletes (immutable audit trail)
CREATE POLICY inventory_destructions_immutable ON inventory_lot_destructions
FOR UPDATE, DELETE
USING (FALSE);
```

---

## 🧪 Testing RLS Policies

### Unit Tests

```sql
-- Test 1: User can access only their site's data
BEGIN;
SET app.current_user_id = 'user-1-uuid';
SET app.user_role = 'operator';

SELECT COUNT(*) FROM tasks; -- Should return only site-scoped tasks

-- Attempt to query another site's data (should return 0)
SELECT COUNT(*) FROM tasks WHERE site_id = 'other-site-uuid';
-- Expected: 0

ROLLBACK;

-- Test 2: Service account can access all sites
BEGIN;
SET app.user_role = 'service_account';

SELECT COUNT(*) FROM tasks; -- Should return all tasks

ROLLBACK;

-- Test 3: ABAC gate blocks unauthorized action
BEGIN;
SET app.current_user_id = 'user-2-uuid';
SET app.user_role = 'operator';

-- Attempt high-risk operation (should fail)
INSERT INTO inventory_lot_destructions (lot_id, site_id, reason)
VALUES ('lot-uuid', 'site-uuid', 'expired');
-- Expected: ERROR or INSERT 0 (blocked by policy)

ROLLBACK;
```

### Integration Tests

```csharp
[Fact]
public async Task RLS_EnforcesS iteScopedAccess()
{
    // Arrange
    var user1Sites = new[] { SiteA, SiteB };
    var user2Sites = new[] { SiteC };
    
    // Act
    var user1Tasks = await _taskRepository.GetAllAsync(user1);
    var user2Tasks = await _taskRepository.GetAllAsync(user2);
    
    // Assert
    Assert.All(user1Tasks, t => Assert.Contains(t.SiteId, user1Sites));
    Assert.All(user2Tasks, t => Assert.Contains(t.SiteId, user2Sites));
    Assert.Empty(user1Tasks.Where(t => t.SiteId == SiteC));
}

[Fact]
public async Task ABAC_BlocksUnauthorizedHighRiskOperation()
{
    // Arrange
    var operatorUser = CreateUser(role: "operator");
    var lot = CreateInventoryLot();
    
    // Act & Assert
    await Assert.ThrowsAsync<UnauthorizedException>(async () =>
    {
        await _inventoryService.ApproveDestructionAsync(lot.Id, "expired", operatorUser);
    });
}
```

---

## 📊 Monitoring RLS/ABAC

### Key Metrics

1. **Authorization Audit Trail**
   - Track all ABAC checks (granted/denied)
   - Alert on repeated denials (potential attack)

2. **RLS Policy Violations**
   - Log any attempts to bypass RLS
   - Monitor for missing session context

3. **Performance Impact**
   - Measure query latency with RLS enabled
   - Optimize policies if p95 > 100ms overhead

### Alerts

```promql
# Alert on high ABAC denial rate
rate(abac_authorization_denied_total[5m]) > 10

# Alert on missing session context
rate(postgres_rls_context_missing_total[5m]) > 0
```

---

## 🔐 Security Best Practices

### ✅ Do's

- **Always** enable RLS on new tables
- **Always** set session context in application connection pool
- **Always** use service accounts for background workers
- **Always** log ABAC decisions to audit trail
- **Test** RLS policies with multiple user roles
- **Review** ABAC permissions quarterly

### ❌ Don'ts

- **Never** disable RLS in production
- **Never** use `FORCE ROW LEVEL SECURITY` (breaks service accounts)
- **Never** store sensitive data without encryption
- **Never** grant `BYPASSRLS` privilege to application users
- **Never** hard-code user IDs in policies
- **Never** allow SQL injection (use parameterized queries)

---

## 📚 Reference Tables

### ABAC Permissions Table

```sql
CREATE TABLE IF NOT EXISTS abac_permissions (
    permission_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    role VARCHAR(50) NOT NULL,
    action VARCHAR(100) NOT NULL,
    resource_type VARCHAR(100) NOT NULL,
    conditions JSONB, -- Additional context requirements
    created_at TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE (role, action, resource_type)
);

-- Example permissions
INSERT INTO abac_permissions (role, action, resource_type, conditions) VALUES
('manager', 'approve_destruction', 'inventory_lot', NULL),
('compliance_officer', 'approve_destruction', 'inventory_lot', NULL),
('admin', 'enable_closed_loop', 'control_loop', '{"promotion_checklist_completed": true}'),
('finance_manager', 'approve_invoice', 'invoice', '{"amount_lte": 10000}'),
('vp_finance', 'approve_invoice', 'invoice', NULL);
```

### Authorization Audit Table

```sql
CREATE TABLE IF NOT EXISTS authorization_audit (
    audit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    site_id UUID,
    action VARCHAR(100) NOT NULL,
    resource_type VARCHAR(100) NOT NULL,
    resource_id UUID,
    granted BOOLEAN NOT NULL,
    context JSONB,
    occurred_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX ix_authz_audit_user_occurred ON authorization_audit (user_id, occurred_at DESC);
CREATE INDEX ix_authz_audit_denied ON authorization_audit (occurred_at DESC) WHERE granted = FALSE;
```

---

## 🆘 Troubleshooting

### Issue: "RLS policy violation" errors

**Cause:** Session context not set  
**Fix:** Ensure application sets `app.current_user_id` and `app.user_role` on every connection

### Issue: Service account can't access data

**Cause:** Missing service account policy  
**Fix:** Add service account bypass policy to affected tables

### Issue: ABAC checks failing unexpectedly

**Cause:** Missing permissions or incorrect context  
**Fix:** Check `abac_permissions` table and review context requirements

### Issue: Queries slow with RLS enabled

**Cause:** RLS policies force sequential scans  
**Fix:** Add indexes on `site_id` and optimize `user_sites` join

---

**✅ Track A Objective:** Zero-trust security model with site isolation by default and ABAC gates for high-risk operations.
