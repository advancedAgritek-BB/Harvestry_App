# Data Flow Mapping

**Audit Date:** December 4, 2025  
**Auditor:** AI Agent  
**Status:** Complete

---

## 1. Authentication Flow

### 1.1 Current Flow (Development - INSECURE)

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    Frontend     │     │  .NET Backend   │     │   PostgreSQL    │
│   (Next.js)     │     │    Services     │     │     + RLS       │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         │ 1. Page Load          │                       │
         │ (mock user loaded     │                       │
         │  from localStorage)   │                       │
         │                       │                       │
         │ 2. API Request        │                       │
         │ X-User-Id: <uuid>     │                       │
         │ X-User-Role: admin    │                       │
         │ X-Site-Id: <uuid>     │                       │
         │──────────────────────►│                       │
         │                       │ 3. Set Session Vars   │
         │                       │ SET app.current_user_id│
         │                       │ SET app.user_role     │
         │                       │ SET app.site_id       │
         │                       │──────────────────────►│
         │                       │                       │
         │                       │ 4. Query (RLS active) │
         │                       │──────────────────────►│
         │                       │◄──────────────────────│
         │                       │                       │
         │◄──────────────────────│                       │
         │      5. Response      │                       │
```

**Problems:**
- Headers can be spoofed by anyone
- No actual authentication
- No session management

### 1.2 Target Flow (Production with Supabase Auth)

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    Frontend     │     │  Supabase Auth  │     │  .NET Backend   │     │    AWS RDS      │
│   (Next.js)     │     │   (External)    │     │    Services     │     │   PostgreSQL    │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │                       │
         │ 1. Login Request      │                       │                       │
         │ (email, password)     │                       │                       │
         │──────────────────────►│                       │                       │
         │                       │                       │                       │
         │◄──────────────────────│                       │                       │
         │ 2. JWT Token          │                       │                       │
         │ (access + refresh)    │                       │                       │
         │                       │                       │                       │
         │ 3. API Request        │                       │                       │
         │ Authorization: Bearer <jwt>                   │                       │
         │────────────────────────────────────────────────►                       │
         │                       │                       │                       │
         │                       │ 4. Validate JWT       │                       │
         │                       │ (verify signature     │                       │
         │                       │  with Supabase secret)│                       │
         │                       │                       │                       │
         │                       │ 5. Extract Claims     │                       │
         │                       │ sub: user_id          │                       │
         │                       │ role: operator        │                       │
         │                       │                       │                       │
         │                       │ 6. Set Session Vars   │                       │
         │                       │ SET app.current_user_id│                       │
         │                       │ SET app.user_role     │                       │
         │                       │ SET app.site_id       │                       │
         │                       │──────────────────────────────────────────────►│
         │                       │                       │                       │
         │                       │ 7. Query (RLS active) │                       │
         │                       │──────────────────────────────────────────────►│
         │                       │◄──────────────────────────────────────────────│
         │                       │                       │                       │
         │◄─────────────────────────────────────────────│                       │
         │      8. Response      │                       │                       │
```

---

## 2. User Registration Flow (Target)

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    Frontend     │     │  Supabase Auth  │     │  .NET Backend   │     │    AWS RDS      │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │                       │
         │ 1. Sign Up Request    │                       │                       │
         │ (email, password,     │                       │                       │
         │  org_name, etc.)      │                       │                       │
         │──────────────────────►│                       │                       │
         │                       │                       │                       │
         │                       │ 2. Create User        │                       │
         │                       │ in auth.users         │                       │
         │                       │                       │                       │
         │                       │ 3. Webhook Trigger    │                       │
         │                       │────────────────────────►                       │
         │                       │                       │                       │
         │                       │                       │ 4. Create user record  │
         │                       │                       │ in public.users        │
         │                       │                       │──────────────────────►│
         │                       │                       │                       │
         │                       │                       │ 5. Create organization │
         │                       │                       │──────────────────────►│
         │                       │                       │                       │
         │                       │                       │ 6. Create site         │
         │                       │                       │──────────────────────►│
         │                       │                       │                       │
         │                       │                       │ 7. Create user_site    │
         │                       │                       │ assignment             │
         │                       │                       │──────────────────────►│
         │                       │                       │                       │
         │◄──────────────────────│                       │                       │
         │ 8. Confirmation Email │                       │                       │
         │                       │                       │                       │
```

---

## 3. Task Creation Flow

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    Frontend     │     │  Tasks Service  │     │    Database     │
│   Task Form     │     │  (.NET)         │     │   (RLS)         │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         │ 1. POST /tasks        │                       │
         │ {title, description,  │                       │
         │  assignee, dueDate}   │                       │
         │ Authorization: JWT    │                       │
         │──────────────────────►│                       │
         │                       │                       │
         │                       │ 2. Validate JWT       │
         │                       │                       │
         │                       │ 3. Check ABAC         │
         │                       │ (tasks:create)        │
         │                       │──────────────────────►│
         │                       │◄──────────────────────│
         │                       │                       │
         │                       │ 4. Check Task Gating  │
         │                       │ (required SOPs,       │
         │                       │  training)            │
         │                       │──────────────────────►│
         │                       │◄──────────────────────│
         │                       │                       │
         │                       │ 5. INSERT task        │
         │                       │ (RLS: site_id check)  │
         │                       │──────────────────────►│
         │                       │◄──────────────────────│
         │                       │                       │
         │                       │ 6. Queue Notification │
         │                       │ (outbox pattern)      │
         │                       │──────────────────────►│
         │                       │                       │
         │◄──────────────────────│                       │
         │ 7. Task Response      │                       │
```

---

## 4. Batch/Lot Creation Flow

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    Frontend     │     │ Genetics Svc    │     │ Inventory Svc   │     │    Database     │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │                       │
         │ 1. Create Batch       │                       │                       │
         │ POST /batches         │                       │                       │
         │──────────────────────►│                       │                       │
         │                       │                       │                       │
         │                       │ 2. Validate strain    │                       │
         │                       │    exists             │                       │
         │                       │──────────────────────────────────────────────►│
         │                       │◄──────────────────────────────────────────────│
         │                       │                       │                       │
         │                       │ 3. Generate batch     │                       │
         │                       │    code (rules)       │                       │
         │                       │──────────────────────────────────────────────►│
         │                       │                       │                       │
         │                       │ 4. INSERT batch       │                       │
         │                       │──────────────────────────────────────────────►│
         │                       │                       │                       │
         │                       │ 5. Create initial     │                       │
         │                       │    stage history      │                       │
         │                       │──────────────────────────────────────────────►│
         │                       │                       │                       │
         │◄──────────────────────│                       │                       │
         │ 6. Batch Created      │                       │                       │
         │                       │                       │                       │
         │ 7. Create Inventory   │                       │                       │
         │    Lot                │                       │                       │
         │─────────────────────────────────────────────►│                       │
         │                       │                       │                       │
         │                       │                       │ 8. Link to batch      │
         │                       │                       │──────────────────────►│
         │                       │                       │                       │
         │◄─────────────────────────────────────────────│                       │
         │ 9. Lot Created        │                       │                       │
```

---

## 5. Telemetry Ingestion Flow

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  IoT Device     │     │ Telemetry Svc   │     │  TimescaleDB    │     │  Alert Engine   │
│  (Sensor)       │     │  Ingestion      │     │  (Hypertable)   │     │  (Background)   │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │                       │
         │ 1. MQTT/HTTP          │                       │                       │
         │ {stream_id, value,    │                       │                       │
         │  timestamp}           │                       │                       │
         │──────────────────────►│                       │                       │
         │                       │                       │                       │
         │                       │ 2. Validate stream    │                       │
         │                       │    (device auth)      │                       │
         │                       │                       │                       │
         │                       │ 3. Batch INSERT       │                       │
         │                       │    into hypertable    │                       │
         │                       │──────────────────────►│                       │
         │                       │                       │                       │
         │◄──────────────────────│                       │                       │
         │ 4. ACK                │                       │                       │
         │                       │                       │                       │
         │                       │                       │ 5. Continuous         │
         │                       │                       │    Aggregate          │
         │                       │                       │    (1h rollups)       │
         │                       │                       │                       │
         │                       │                       │                       │
         │                       │                       │────────────────────────►
         │                       │                       │ 6. Check Alert Rules   │
         │                       │                       │                        │
         │                       │                       │◄────────────────────────
         │                       │                       │ 7. Fire Alert if       │
         │                       │                       │    threshold exceeded  │
```

---

## 6. Multi-Site Switching Flow

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    Frontend     │     │  .NET Backend   │     │    Database     │
│  Site Selector  │     │                 │     │   (RLS)         │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         │ 1. User clicks        │                       │
         │    site selector      │                       │
         │                       │                       │
         │ 2. Show available     │                       │
         │    sites from JWT     │                       │
         │    (site_permissions) │                       │
         │                       │                       │
         │ 3. User selects       │                       │
         │    "Oakdale"          │                       │
         │                       │                       │
         │ 4. Update local       │                       │
         │    currentSiteId      │                       │
         │                       │                       │
         │ 5. Refetch data       │                       │
         │ X-Site-Id: oakdale    │                       │
         │──────────────────────►│                       │
         │                       │                       │
         │                       │ 6. Set session vars   │
         │                       │ app.site_id = oakdale │
         │                       │──────────────────────►│
         │                       │                       │
         │                       │ 7. RLS filters to     │
         │                       │    Oakdale data only  │
         │                       │◄──────────────────────│
         │                       │                       │
         │◄──────────────────────│                       │
         │ 8. Oakdale data       │                       │
```

---

## 7. METRC Sync Flow

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    Harvestry    │     │ METRC Sync Svc  │     │    Database     │     │   METRC API     │
│    (Action)     │     │  (Background)   │     │   (Outbox)      │     │   (External)    │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │                       │
         │ 1. Create Package     │                       │                       │
         │    in Harvestry       │                       │                       │
         │──────────────────────────────────────────────►│                       │
         │                       │                       │                       │
         │                       │                       │ 2. Trigger enqueue    │
         │                       │                       │    outbox message     │
         │                       │                       │                       │
         │                       │ 3. Poll outbox        │                       │
         │                       │◄──────────────────────│                       │
         │                       │                       │                       │
         │                       │ 4. POST to METRC      │                       │
         │                       │──────────────────────────────────────────────►│
         │                       │                       │                       │
         │                       │◄──────────────────────────────────────────────│
         │                       │ 5. METRC Response     │                       │
         │                       │    (metrc_package_id) │                       │
         │                       │                       │                       │
         │                       │ 6. Update local       │                       │
         │                       │    record with        │                       │
         │                       │    metrc_package_id   │                       │
         │                       │──────────────────────►│                       │
         │                       │                       │                       │
         │                       │ 7. Mark outbox        │                       │
         │                       │    complete           │                       │
         │                       │──────────────────────►│                       │
         │                       │                       │                       │
         │                       │ 8. Log sync event     │                       │
         │                       │──────────────────────►│                       │
```

---

## 8. Key Session Variables Contract

For RLS to work, the backend MUST set these PostgreSQL session variables:

| Variable | Source | Required |
|----------|--------|----------|
| `app.current_user_id` | JWT `sub` claim | ✅ Always |
| `app.user_role` | JWT `role` claim or DB lookup | ✅ Always |
| `app.site_id` | Request header or JWT claim | Optional |

**Setting Order:**
1. Validate JWT signature
2. Extract `sub` claim → `app.current_user_id`
3. Lookup user role from DB (or JWT if embedded) → `app.user_role`
4. Get site_id from request header → `app.site_id`
5. Execute query (RLS now active)

---

*End of Data Flow Mapping*


