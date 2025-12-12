# Backend Service Audit

**Audit Date:** December 4, 2025  
**Auditor:** AI Agent  
**Status:** Complete

---

## Executive Summary

The backend is built with .NET 8 using Clean Architecture (Domain/Application/Infrastructure/API layers). Authentication is currently **header-based** (development mode) and needs to be migrated to JWT validation for production. Most services are well-structured with proper authorization checks.

### Key Findings

| Finding | Severity | Impact |
|---------|----------|--------|
| Header-based auth (development only) | High | Not production-ready |
| Some controllers missing `[Authorize]` | High | Security vulnerability |
| Site context via `X-Site-Id` header | Medium | Needs JWT integration |
| Good ABAC implementation in Identity | ✅ | Production-ready pattern |

---

## 1. Service Architecture Overview

```
src/backend/services/
├── core-platform/
│   ├── identity/          # Auth, users, roles, badges, sessions
│   ├── genetics/          # Strains, batches, mother plants
│   ├── spatial/           # Rooms, locations, equipment
│   ├── inventory/         # Lots, movements, labels
│   ├── organizations/     # Org management
│   ├── processing/        # Manufacturing
│   └── ...
├── workflow-messaging/
│   └── tasks/             # Tasks, messaging, Slack
├── telemetry-controls/
│   └── telemetry/         # Sensor data, alerts
├── integrations/
│   ├── compliance-metrc/  # METRC sync
│   ├── quickbooks/        # QBO integration
│   └── labeling/          # Label generation
└── gateway/               # API gateway (planned)
```

---

## 2. Service-by-Service Analysis

### 2.1 Identity Service

**Location:** `src/backend/services/core-platform/identity/`

#### Controllers

| Controller | Route | Auth | Endpoints |
|------------|-------|------|-----------|
| `AuthController` | `/api/v1/auth` | Mixed | Badge login, logout, sessions |
| `UsersController` | `/api/v1/users` | ✅ | CRUD, suspend, unlock |
| `SitesController` | `/api/v1/sites` | ⚠️ Missing | Get all, create |
| `BadgesController` | `/api/v1/badges` | ✅ | CRUD, revoke |
| `PermissionsController` | `/api/v1/permissions` | ✅ | Check permissions |
| `SlackOAuthController` | `/api/v1/slack/oauth` | ✅ | OAuth flow |

#### Authentication Mechanism

**Current (Development):**
```csharp
// HeaderAuthenticationHandler.cs
var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
var roleHeader = Request.Headers["X-User-Role"].FirstOrDefault();
var siteHeader = Request.Headers["X-Site-Id"].FirstOrDefault();
```

**Required for Production:**
```csharp
// Supabase JWT validation
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = "https://your-project.supabase.co/auth/v1";
        // ... JWT validation config
    });
```

#### Issues Found

1. **SitesController missing `[Authorize]`** - Security vulnerability
2. **Header-based auth is insecure** - Anyone can spoof user identity
3. **Good:** ABAC permission checks are well-implemented

---

### 2.2 Genetics Service

**Location:** `src/backend/services/core-platform/genetics/`

#### Controllers

| Controller | Route | Auth | Endpoints |
|------------|-------|------|-----------|
| `GeneticsController` | `/api/v1/sites/{siteId}/genetics` | ✅ | CRUD |
| `StrainsController` | `/api/v1/sites/{siteId}/strains` | ✅ | CRUD, can-delete |
| `BatchesController` | `/api/v1/sites/{siteId}/batches` | ✅ | CRUD, lifecycle |
| `BatchStagesController` | `/api/v1/sites/{siteId}/batch-stages` | ✅ | Stage definitions |
| `BatchCodeRulesController` | `/api/v1/sites/{siteId}/batch-code-rules` | ✅ | Code rules |
| `MotherPlantsController` | `/api/v1/sites/{siteId}/mother-plants` | ✅ | Mother management |
| `PropagationController` | `/api/v1/sites/{siteId}/propagation` | ✅ | Propagation settings |

#### Route Pattern

All routes are **site-scoped**: `/api/v1/sites/{siteId}/...`

This is a good pattern that enforces site context in the URL.

#### Authentication Mechanism

```csharp
private Guid ResolveUserId()
{
    if (Request.Headers.TryGetValue("X-User-Id", out var userIdString) &&
        Guid.TryParse(userIdString, out var userId))
    {
        return userId;
    }
    return Guid.Empty;
}
```

**Issue:** Relies on `X-User-Id` header instead of JWT claims.

---

### 2.3 Spatial Service

**Location:** `src/backend/services/core-platform/spatial/`

#### Controllers

| Controller | Route | Auth | Endpoints |
|------------|-------|------|-----------|
| `RoomsController` | `/api/v1/sites/{siteId}/rooms` | ⚠️ | CRUD, status |
| `LocationsController` | `/api/v1/sites/{siteId}/locations` | ⚠️ | CRUD, hierarchy |
| `EquipmentController` | `/api/v1/sites/{siteId}/equipment` | ⚠️ | CRUD, channels |
| `CalibrationController` | `/api/v1/sites/{siteId}/calibrations` | ⚠️ | Calibration records |

**Issue:** Controllers are missing `[Authorize]` attribute - only checking headers.

#### RLS Context Middleware

```csharp
// RlsContextMiddleware.cs
public async Task InvokeAsync(HttpContext context)
{
    var userId = context.Request.Headers["X-User-Id"].FirstOrDefault();
    var role = context.Request.Headers["X-User-Role"].FirstOrDefault();
    var siteId = context.Request.Headers["X-Site-Id"].FirstOrDefault();
    
    // Set PostgreSQL session variables
    await SetSessionVariables(userId, role, siteId);
}
```

**Good:** RLS context is properly set. Just needs JWT integration.

---

### 2.4 Tasks Service

**Location:** `src/backend/services/workflow-messaging/tasks/`

#### Controllers

| Controller | Route | Auth | Endpoints |
|------------|-------|------|-----------|
| `TasksController` | `/api/v1/sites/{siteId}/tasks` | ✅ | CRUD, state machine |
| `TaskBlueprintsController` | `/api/v1/sites/{siteId}/blueprints` | ✅ | Task templates |
| `TaskLibraryController` | `/api/v1/sites/{siteId}/library` | ✅ | Task types |
| `TaskGenerationController` | `/api/v1/sites/{siteId}/generation` | ✅ | Auto-generation |
| `ConversationsController` | `/api/v1/sites/{siteId}/conversations` | ✅ | Messaging |
| `NotificationsController` | `/api/v1/sites/{siteId}/notifications` | ✅ | In-app notifications |
| `SlackController` | `/api/v1/sites/{siteId}/slack` | ✅ | Slack config |
| `SlackWebhookController` | `/api/v1/slack/webhooks` | Special | Webhook receiver |
| `SopsController` | `/api/v1/sites/{siteId}/sops` | ✅ | SOP management |

#### Authentication Mechanism

```csharp
// UserIdAuthenticationHandler.cs
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = "X-User-Id";
})
.AddScheme<AuthenticationSchemeOptions, UserIdAuthenticationHandler>("X-User-Id", null);
```

**Same issue:** Header-based authentication.

---

### 2.5 Telemetry Service

**Location:** `src/backend/services/telemetry-controls/telemetry/`

#### Controllers

| Controller | Route | Auth | Description |
|------------|-------|------|-------------|
| `SensorStreamsController` | `/api/v1/sites/{siteId}/streams` | ⚠️ | Stream config |
| `AlertsController` | `/api/v1/sites/{siteId}/alerts` | ⚠️ | Alert rules |
| `IngestionController` | `/api/v1/ingest` | Special | Device ingestion |

**Note:** Ingestion endpoints may need device auth, not user auth.

---

## 3. Current Authentication Flow

```
┌─────────────┐    ┌─────────────────────────────┐    ┌──────────────┐
│   Client    │───►│ API (Header Auth Handler)   │───►│   Database   │
│             │    │                             │    │    (RLS)     │
└─────────────┘    │ X-User-Id: <uuid>           │    └──────────────┘
                   │ X-User-Role: operator       │
                   │ X-Site-Id: <uuid>           │
                   │                             │
                   │ Sets PostgreSQL session:    │
                   │ app.current_user_id = <uuid>│
                   │ app.user_role = operator    │
                   │ app.site_id = <uuid>        │
                   └─────────────────────────────┘
```

**Problems:**
1. Headers can be spoofed
2. No token validation
3. No session management

---

## 4. Required Authentication Flow (Production)

```
┌─────────────┐    ┌──────────────┐    ┌─────────────────────────────┐    ┌──────────────┐
│   Client    │───►│  Supabase    │───►│ API (JWT Auth Handler)      │───►│   Database   │
│             │    │    Auth      │    │                             │    │    (RLS)     │
└─────────────┘    │              │    │ Authorization: Bearer <jwt> │    └──────────────┘
                   │ Returns JWT  │    │                             │
                   └──────────────┘    │ Validates JWT signature     │
                                       │ Extracts claims:            │
                                       │   sub: <user_id>            │
                                       │   role: operator            │
                                       │   site_id: <uuid>           │
                                       │                             │
                                       │ Sets PostgreSQL session:    │
                                       │ app.current_user_id = <uuid>│
                                       │ app.user_role = operator    │
                                       │ app.site_id = <uuid>        │
                                       └─────────────────────────────┘
```

---

## 5. Endpoint Inventory

### 5.1 Identity Service Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/v1/auth/badge-login` | Badge authentication | ❌ |
| POST | `/api/v1/auth/logout` | End session | ✅ |
| GET | `/api/v1/auth/sessions` | Get active sessions | ✅ |
| GET | `/api/v1/users/{id}` | Get user | ✅ |
| POST | `/api/v1/users` | Create user | ✅ |
| PUT | `/api/v1/users/{id}` | Update user | ✅ |
| PUT | `/api/v1/users/{id}/suspend` | Suspend user | ✅ |
| PUT | `/api/v1/users/{id}/unlock` | Unlock user | ✅ |
| GET | `/api/v1/sites` | List sites | ⚠️ |
| POST | `/api/v1/sites` | Create site | ⚠️ |

### 5.2 Genetics Service Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/sites/{siteId}/genetics` | List genetics |
| POST | `/api/v1/sites/{siteId}/genetics` | Create genetics |
| GET | `/api/v1/sites/{siteId}/strains` | List strains |
| POST | `/api/v1/sites/{siteId}/strains` | Create strain |
| GET | `/api/v1/sites/{siteId}/strains/{id}` | Get strain |
| PUT | `/api/v1/sites/{siteId}/strains/{id}` | Update strain |
| DELETE | `/api/v1/sites/{siteId}/strains/{id}` | Delete strain |
| GET | `/api/v1/sites/{siteId}/batches` | List batches |
| POST | `/api/v1/sites/{siteId}/batches` | Create batch |
| PUT | `/api/v1/sites/{siteId}/batches/{id}/stage` | Advance stage |

### 5.3 Spatial Service Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/sites/{siteId}/rooms` | List rooms |
| POST | `/api/v1/sites/{siteId}/rooms` | Create room |
| GET | `/api/v1/sites/{siteId}/rooms/{id}` | Get room |
| PUT | `/api/v1/sites/{siteId}/rooms/{id}` | Update room |
| PATCH | `/api/v1/sites/{siteId}/rooms/{id}/status` | Change status |
| GET | `/api/v1/sites/{siteId}/locations` | List locations |
| POST | `/api/v1/sites/{siteId}/locations` | Create location |
| GET | `/api/v1/sites/{siteId}/equipment` | List equipment |
| POST | `/api/v1/sites/{siteId}/equipment` | Register equipment |

### 5.4 Tasks Service Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/sites/{siteId}/tasks` | List tasks |
| POST | `/api/v1/sites/{siteId}/tasks` | Create task |
| GET | `/api/v1/sites/{siteId}/tasks/{id}` | Get task |
| PUT | `/api/v1/sites/{siteId}/tasks/{id}` | Update task |
| POST | `/api/v1/sites/{siteId}/tasks/{id}/start` | Start task |
| POST | `/api/v1/sites/{siteId}/tasks/{id}/complete` | Complete task |
| GET | `/api/v1/sites/{siteId}/conversations` | List conversations |
| POST | `/api/v1/sites/{siteId}/conversations/{id}/messages` | Send message |

---

## 6. Migration Requirements

### 6.1 Add Supabase JWT Validation

For each service's `Program.cs`:

```csharp
// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Supabase:Url"],
            ValidateAudience = true,
            ValidAudience = "authenticated",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Supabase:JwtSecret"])),
            ValidateLifetime = true
        };
    });
```

### 6.2 Update RLS Context Middleware

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Extract from JWT claims instead of headers
    var userId = context.User.FindFirst("sub")?.Value;
    var role = context.User.FindFirst("role")?.Value ?? "operator";
    var siteId = context.User.FindFirst("site_id")?.Value 
                 ?? context.Request.Headers["X-Site-Id"].FirstOrDefault();
    
    await SetSessionVariables(userId, role, siteId);
    await _next(context);
}
```

### 6.3 Add `[Authorize]` to Missing Controllers

- `SitesController.cs`
- `RoomsController.cs`
- `LocationsController.cs`
- `EquipmentController.cs`

---

## 7. Configuration Requirements

### Current (`appsettings.json`)

```json
{
  "Security": {
    "JWT": {
      "Secret": "dev-secret-key-do-not-use-in-production",
      "Issuer": "harvestry-dev",
      "Audience": "harvestry-api"
    }
  }
}
```

### Required for Production

```json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "JwtSecret": "your-supabase-jwt-secret",
    "AnonKey": "your-anon-key"
  },
  "ConnectionStrings": {
    "PostgreSQL": "Host=your-rds-endpoint;..."
  }
}
```

---

## 8. Recommendations

### Critical (Must Fix)

1. **Add `[Authorize]` to all controllers** that are missing it
2. **Implement Supabase JWT validation** in all services
3. **Remove header-based auth handler** (or keep as fallback for dev only)

### High Priority

1. **Standardize user ID extraction** - use JWT claims
2. **Add site context to JWT** or validate via database lookup
3. **Implement refresh token handling**

### Medium Priority

1. **Add rate limiting** on authentication endpoints
2. **Implement audit logging** for auth events
3. **Add CORS configuration** for production domains

---

## Appendix: Files to Modify

### Identity Service

- `Program.cs` - Add JWT auth
- `HeaderAuthenticationHandler.cs` - Replace with JWT handler
- `SitesController.cs` - Add `[Authorize]`

### Genetics Service

- `Program.cs` - Add JWT auth
- All controllers - Update `ResolveUserId()` to use claims

### Spatial Service

- `Program.cs` - Add JWT auth
- All controllers - Add `[Authorize]`, update user resolution
- `RlsContextMiddleware.cs` - Use JWT claims

### Tasks Service

- `Program.cs` - Add JWT auth
- `UserIdAuthenticationHandler.cs` - Replace with JWT handler

### All Services

- `appsettings.json` - Add Supabase configuration
- Add `Supabase:JwtSecret` to secrets/environment

---

*End of Backend Service Audit*








