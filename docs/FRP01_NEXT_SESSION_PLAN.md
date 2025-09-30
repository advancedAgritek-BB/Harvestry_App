# FRP-01 Next Session - Implementation Plan

**Current Status:** ~40% Complete (Database + Domain + Interfaces)  
**Next Phase:** Application Service Implementations

---

## ✅ Completed This Session

1. **Database Layer** (100%)
   - 3 SQL migrations (identity, ABAC, training/SOP)
   - RLS policies, PostgreSQL functions
   - Hybrid database setup (Supabase + Timescale Cloud)

2. **Domain Layer** (100%)
   - Shared kernel (Entity, ValueObject base classes)
   - 6 domain entities (User, Role, Site, Badge, Session, UserSite)
   - 3 value objects (Email, PhoneNumber, BadgeCode)
   - 1 enums file (UserStatus, SiteStatus, BadgeStatus, etc.)

3. **Application DTOs & Interfaces** (100%)
   - PolicyEvaluationResult, TaskGatingResult, TwoPersonApprovalRequest
   - IPolicyEvaluationService, ITaskGatingService, IBadgeAuthService
   - BadgeLoginResult, SessionInfo

**Files Created:** 17 files, ~3,500 lines of production code

---

## 🎯 Next Session Goals

### Priority 1: Application Service Implementations

#### 1. PolicyEvaluationService.cs (Est: 2-3 hours)
**Location:** `src/backend/services/core-platform/identity/Application/Services/PolicyEvaluationService.cs`

**Responsibilities:**
- Evaluate ABAC permissions using database function
- Manage two-person approval workflow
- Log authorization decisions to audit trail

**Key Methods:**
```csharp
Task<PolicyEvaluationResult> EvaluatePermissionAsync(...)
Task<TwoPersonApprovalResponse> InitiateTwoPersonApprovalAsync(...)
Task<bool> ApproveTwoPersonRequestAsync(...)
Task<bool> RejectTwoPersonRequestAsync(...)
Task<IEnumerable<TwoPersonApprovalResponse>> GetPendingApprovalsAsync(...)
```

**Dependencies:**
- Database connection (Npgsql) to call `check_abac_permission()` function
- Repository for `two_person_approvals` table
- Repository for `authorization_audit` table

#### 2. BadgeAuthService.cs (Est: 2 hours)
**Location:** `src/backend/services/core-platform/identity/Application/Services/BadgeAuthService.cs`

**Responsibilities:**
- Authenticate users via badge scan
- Create and manage sessions
- Revoke badges and sessions

**Key Methods:**
```csharp
Task<BadgeLoginResult> LoginWithBadgeAsync(...)
Task<bool> RevokeBadgeAsync(...)
Task<IEnumerable<SessionInfo>> GetActiveSessionsAsync(...)
Task<bool> RevokeSessionAsync(...)
```

**Dependencies:**
- BadgeRepository (find by code, update usage)
- UserRepository (get user details)
- SessionRepository (create, list, revoke)
- Token generation service (JWT or secure random)

#### 3. TaskGatingService.cs (Est: 1-2 hours)
**Location:** `src/backend/services/core-platform/identity/Application/Services/TaskGatingService.cs`

**Responsibilities:**
- Check SOP/training prerequisites
- Return explicit denial reasons
- Call database `check_task_gating()` function

**Key Methods:**
```csharp
Task<TaskGatingResult> CheckTaskGatingAsync(...)
Task<IEnumerable<TaskGatingRequirement>> GetRequirementsForTaskTypeAsync(...)
```

**Dependencies:**
- Database connection to call `check_task_gating()` function
- Repository for `task_gating_requirements`
- Repository for `sop_signoffs` and `training_assignments`

---

### Priority 2: Infrastructure Layer (Repositories)

#### Repositories to Implement (Est: 3-4 hours)

**Location:** `src/backend/services/core-platform/identity/Infrastructure/Persistence/`

1. **UserRepository.cs**
   - CRUD for User entity
   - Find by email, ID
   - Set RLS context variables

2. **BadgeRepository.cs**
   - Find badge by code
   - Update last used timestamp
   - Revoke badge

3. **SessionRepository.cs**
   - Create session
   - List active sessions by user
   - Revoke session

4. **RoleRepository.cs**
   - Get role by name or ID
   - Check permissions

5. **SiteRepository.cs**
   - Get site by ID
   - Check site policies

**Key Infrastructure Concerns:**
- **RLS Context:** Must set `app.current_user_id` and `app.user_role` on every query
- **Npgsql + Dapper:** Use Dapper for simple queries, Npgsql for PostgreSQL functions
- **Connection Management:** Use connection pooling, dispose properly
- **Transactions:** Use for multi-step operations

---

### Priority 3: Dependency Injection Setup

**Location:** `src/backend/services/core-platform/identity/API/Program.cs` or `Startup.cs`

**Register Services:**
```csharp
// Application Services
services.AddScoped<IPolicyEvaluationService, PolicyEvaluationService>();
services.AddScoped<IBadgeAuthService, BadgeAuthService>();
services.AddScoped<ITaskGatingService, TaskGatingService>();

// Repositories
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IBadgeRepository, BadgeRepository>();
services.AddScoped<ISessionRepository, SessionRepository>();
services.AddScoped<IRoleRepository, RoleRepository>();
services.AddScoped<ISiteRepository, SiteRepository>();

// Database Connection Factory
// Register connection string for repositories to create connections per operation
services.AddScoped<Func<IDbConnection>>(sp => 
{
    var connectionString = Configuration.GetConnectionString("Supabase");
    return () => new NpgsqlConnection(connectionString);
});

// Token Generation
services.AddScoped<ITokenGenerationService, TokenGenerationService>();
```

---

### Priority 4: Unit Tests (Est: 4-5 hours)

**Target: ≥90% coverage for application services**

**Location:** `src/backend/services/core-platform/identity/Tests/Unit/`

#### Test Files to Create:

1. **PolicyEvaluationServiceTests.cs**
   - Test permission grant/deny scenarios
   - Test two-person approval workflow
   - Test approval expiration
   - Mock database calls

2. **BadgeAuthServiceTests.cs**
   - Test successful badge login
   - Test expired badge
   - Test revoked badge
   - Test session creation

3. **TaskGatingServiceTests.cs**
   - Test allowed task (all requirements met)
   - Test blocked task (missing SOP signoff)
   - Test blocked task (missing training)
   - Test explicit denial reasons

4. **Domain Entity Tests:**
   - UserTests.cs (password, lockout, site assignments)
   - BadgeTests.cs (usage, revocation, expiration)
   - SessionTests.cs (token validation, expiration)
   - RoleTests.cs (permission checking)
   - SiteTests.cs (license validation, policies)

**Testing Tools:**
- xUnit for test framework
- Moq for mocking
- FluentAssertions for readable assertions
- AutoFixture for test data generation

---

### Priority 5: Integration Tests (Est: 3-4 hours)

**Location:** `src/backend/services/core-platform/identity/Tests/Integration/`

#### Test Files to Create:

1. **RlsFuzzTests.cs** (20+ scenarios)
   - Test cross-site data access (should be blocked)
   - Test service account bypass
   - Test user can only see own data
   - Test admin cross-site access

2. **BadgeLoginFlowTests.cs**
   - End-to-end badge login
   - Session creation and validation
   - Session revocation

3. **TwoPersonApprovalIntegrationTests.cs**
   - Create approval request
   - Approve with different user
   - Reject approval
   - Expiration handling

**Integration Test Setup:**
- Use real Supabase connection (test database)
- Set up test data via migrations
- Clean up after tests
- Use transactions for isolation

---

## 📋 Implementation Checklist

### Application Services
- [ ] PolicyEvaluationService.cs implementation
- [ ] BadgeAuthService.cs implementation
- [ ] TaskGatingService.cs implementation

### Infrastructure (Repositories)
- [ ] UserRepository.cs
- [ ] BadgeRepository.cs
- [ ] SessionRepository.cs
- [ ] RoleRepository.cs
- [ ] SiteRepository.cs
- [ ] Database context with RLS

### Dependency Injection
- [ ] Register services in DI container
- [ ] Configure database connections
- [ ] Configure token generation

### Unit Tests (≥90% coverage)
- [ ] PolicyEvaluationServiceTests.cs
- [ ] BadgeAuthServiceTests.cs
- [ ] TaskGatingServiceTests.cs
- [ ] Domain entity tests (User, Badge, Session, Role, Site)

### Integration Tests
- [ ] RlsFuzzTests.cs (20+ scenarios)
- [ ] BadgeLoginFlowTests.cs
- [ ] TwoPersonApprovalIntegrationTests.cs

---

## 🛠️ Technical Decisions Needed

### 1. Token Generation Strategy
**Options:**
- **Option A:** JWT tokens (stateless, self-contained)
- **Option B:** Random tokens (stored in database)
- **Recommendation:** Option B for MVP (simpler, revocable)

### 2. Password Hashing
**Options:**
- **Option A:** BCrypt (battle-tested, slower)
- **Option B:** Argon2 (modern, OWASP recommended)
- **Recommendation:** Option B (Argon2id)

### 3. Session Expiration
**Options:**
- **Badge Sessions:** 12 hours (operators work shifts)
- **Password Sessions:** 8 hours (office workers)
- **Sliding Expiration:** Extend on activity
- **Recommendation:** Badge=12h, Password=8h, Sliding enabled

### 4. ABAC Audit Logging
**Options:**
- **Option A:** Log all checks (verbose, good for compliance)
- **Option B:** Log only denials (less storage)
- **Recommendation:** Option A for MVP (full audit trail)

---

## 📊 Estimated Timeline

| Phase | Est. Hours | Priority |
|-------|-----------|----------|
| Application Services | 5-7 | High |
| Repositories | 3-4 | High |
| DI Setup | 1 | High |
| Unit Tests | 4-5 | High |
| Integration Tests | 3-4 | Medium |
| **Total** | **16-21 hours** | - |

**Current Progress:** ~40% of FRP-01  
**After Next Session:** ~75% of FRP-01  
**Remaining:** API Controllers, Background Jobs, E2E Tests

---

## 🚀 Quick Start Commands

```bash
# Navigate to identity service
cd src/backend/services/core-platform/identity

# Run unit tests
dotnet test Tests/Unit/

# Run integration tests (requires database)
dotnet test Tests/Integration/

# Check code coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportsFormat=lcov

# Build the service
dotnet build

# Run the service locally
dotnet run --project API/
```

---

## 📁 File Structure Preview

```
src/backend/services/core-platform/identity/
├── Application/
│   ├── DTOs/
│   │   └── PolicyEvaluationResult.cs ✅
│   ├── Interfaces/
│   │   └── IPolicyEvaluationService.cs ✅
│   └── Services/
│       ├── PolicyEvaluationService.cs [NEXT]
│       ├── BadgeAuthService.cs [NEXT]
│       └── TaskGatingService.cs [NEXT]
├── Infrastructure/
│   └── Persistence/
│       ├── UserRepository.cs [NEXT]
│       ├── BadgeRepository.cs [NEXT]
│       ├── SessionRepository.cs [NEXT]
│       ├── RoleRepository.cs [NEXT]
│       └── SiteRepository.cs [NEXT]
├── Domain/
│   ├── Entities/ ✅ (6 files)
│   ├── ValueObjects/ ✅ (3 files)
│   └── Enums/ ✅ (1 file)
└── Tests/
    ├── Unit/ [NEXT]
    │   ├── PolicyEvaluationServiceTests.cs
    │   ├── BadgeAuthServiceTests.cs
    │   └── TaskGatingServiceTests.cs
    └── Integration/ [NEXT]
        ├── RlsFuzzTests.cs
        └── BadgeLoginFlowTests.cs
```

---

## 💡 Implementation Tips

1. **Start Small:** Implement one service at a time, test thoroughly
2. **Mock Early:** Create repository interfaces, mock for unit tests
3. **Database First:** Test database functions directly before wrapping in services
4. **RLS Validation:** Run `psql` queries manually to verify RLS policies work
5. **Logging:** Add structured logging from the start (Serilog recommended)
6. **Error Handling:** Use Result<T> pattern or custom exceptions
7. **Async All the Way:** All database operations should be async

---

## 🎯 Session Success Criteria

**Next session is successful if:**
- [ ] All 3 application services implemented and compiling
- [ ] At least 3 repositories implemented (User, Badge, Session)
- [ ] Unit tests passing for at least one service (≥90% coverage)
- [ ] Integration test for RLS policies (at least 5 scenarios passing)
- [ ] Badge login flow working end-to-end (even without API)

---

**Ready to continue!** 🚀  
**Next Action:** Implement `PolicyEvaluationService.cs`  
**Est. Time to Complete FRP-01:** 16-21 hours remaining
