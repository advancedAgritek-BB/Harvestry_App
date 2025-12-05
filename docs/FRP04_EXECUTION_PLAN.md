# FRP-04 Execution Plan - Tasks, Messaging & Slack Integration

**Date:** October 7, 2025  
**Status:** Implementation Complete - Validation In Progress
**Approach:** Delivered via 3 vertical slices (see execution summary)
**Estimated Time:** 29.5 hours (accounts for shared prep, configuration, and Slack integration)

---

## ✅ Execution Summary

- Task lifecycle, conversation messaging, and Slack notification services implemented across Domain, Application, Infrastructure, and API layers (`src/backend/services/workflow-messaging/tasks/*`).
- Database migrations for tasks, conversations, Slack workspaces, notification queue, and bridge log committed with RLS enforcement (`src/database/migrations/frp04`).
- Background workers for Slack outbox, dependency resolver, and overdue monitor registered in the Tasks API host (`ServiceCollectionExtensions.cs`, `Program.cs`).
- Automated test suite (22 specs) covering lifecycle gating, Slack dedupe, workers, and repositories passing (`tests/backend/services/workflow-messaging/tasks/TaskApplicationTests`).
- Outstanding operational follow-up: Day 2 Slack smoke test, staging promotion of migrations, production rollout scheduling (see `docs/FRP04_CURRENT_STATUS.md#overall-status`).

> The detailed slice plan below is retained for historical context and future onboarding.

## 📊 Current State Summary

### ✅ PREREQUISITES COMPLETE

- ✅ **FRP-01 Complete** - Identity, RLS, ABAC foundation with SOP/training gating
- ✅ **Database Infrastructure** - Supabase with RLS policies
- ✅ **API Infrastructure** - ASP.NET Core with established patterns
- ✅ **Test Infrastructure** - Integration test automation

### 🎯 TARGET DELIVERABLES

- 🎯 **Task Workflow Engine** - Complete lifecycle with dependencies and gating
- 🎯 **Messaging System** - Threaded conversations with attachments
- 🎯 **Slack Integration** - Notify-only bridge with retry/idempotency
- 🎯 **Compliance Gating** - Integration with FRP-01 SOP/training requirements

---

## 🎯 Execution Strategy

### Why Vertical Slices?

Instead of building all services → all repos → all controllers, we build **complete vertical slices**:

**Slice = Service + Repository + Controller + Validators + Tests**

**Benefits:**

- ✅ Each slice is independently testable
- ✅ Demonstrates progress incrementally
- ✅ Easier to review and validate
- ✅ Reduces integration risk
- ✅ Can deploy slice-by-slice

**Note:** Total estimate is 26 hours over 5 days (accounts for infrastructure setup, implementation, Slack integration, testing, and deployment wiring).

---

## 📋 THE 3 SLICES

```
SLICE 1: Task Workflow Engine
├── Service: TaskLifecycleService, TaskGatingResolverService
├── Repos: TaskRepository, TaskDependencyRepository, TaskWatcherRepository
├── Controllers: TasksController
├── Validators: TaskValidators
└── Tests: Unit + Integration

SLICE 2: Messaging & Conversations
├── Service: ConversationService
├── Repos: ConversationRepository, MessageRepository
├── Controller: ConversationsController
├── Validators: ConversationValidators
└── Tests: Unit + Integration

SLICE 3: Slack Integration
├── Service: SlackNotificationService
├── Workers: SlackNotificationWorker
├── Repos: SlackWorkspaceRepository, SlackChannelMappingRepository, SlackNotificationQueueRepository
├── Controller: SlackController
├── Validators: SlackValidators
└── Tests: Unit + Integration
```

---

## 🧰 Pre-Slice Setup (90 min)

Complete these shared tasks before starting the feature slices:

1. **Domain Rehydration Helpers (45 min)**
   - Add static `FromPersistence(...)` factories to `Task`, `Conversation`, `Message`, and `SlackNotification`
   - Keep persistence-specific guardrails inside the factory to centralize validation

2. **DTO Mapping Profiles (20 min)**
   - Create an AutoMapper profile (or dedicated mapper class) under `Application/Mappers`
   - Ensures controllers return DTOs rather than exposing domain types directly

3. **Configuration & DI Checklist (25 min)**
   - Register services, repositories, validators, and mappers in API `Program.cs`
   - Wire up Slack webhook configuration (encrypted bot tokens)
   - Document environment variables in `docs/infra/environment-variables.md`

4. **Secrets Validation (10 min)**
   - Verify `slack_tasks_dev` secret exposes `TASKS_SLACK_BOT_TOKEN`, `TASKS_SLACK_REFRESH_TOKEN`, `TASKS_SLACK_WORKSPACE_ID`, `SLACK_CLIENT_ID`, `SLACK_CLIENT_SECRET`
   - Add configuration binding to read the secret at startup (ConfigurationBuilder or AWS Secrets Manager provider)
   - Plan refresh job to call `oauth.v2.access` with the stored refresh token before the 11h expiry
5. **Smoke-Test Fixtures (15 min)**
   - Apply `src/database/migrations/frp04/20251001_01_SeedTrainingAndTaskFixtures.sql` to seed SOP/training data
   - Confirm `equipment_calibration` gating passes while `smoke_test_day_2` blocks without the readiness module
   - Capture evidence in QA notes prior to Day 2 rehearsal

---

## 🔧 SLICE 1: TASK WORKFLOW ENGINE

**Goal:** Complete task lifecycle with gating and dependencies  
**Time:** 9-11 hours (after shared pre-work)  
**Owner:** Workflow & Messaging Squad

### Task 1.1: Create Folder Structure (5 min)

```bash
# Create directories
mkdir -p src/backend/services/workflow-messaging/tasks/Application/Interfaces
mkdir -p src/backend/services/workflow-messaging/tasks/Application/DTOs
mkdir -p src/backend/services/workflow-messaging/tasks/Application/Services
mkdir -p src/backend/services/workflow-messaging/tasks/Infrastructure/Persistence
mkdir -p src/backend/services/workflow-messaging/tasks/API/Controllers
mkdir -p src/backend/services/workflow-messaging/tasks/API/Validators
```

### Task 1.2: Create Service Interfaces (20 min)

**File:** `Application/Interfaces/ITaskLifecycleService.cs`

```csharp
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Entities;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.Interfaces;

/// <summary>
/// Service for managing task lifecycle operations
/// </summary>
public interface ITaskLifecycleService
{
    // Task CRUD
    Task<TaskResponse> CreateTaskAsync(Guid siteId, CreateTaskRequest request, Guid userId, CancellationToken ct = default);
    Task<TaskResponse?> GetTaskByIdAsync(Guid siteId, Guid taskId, CancellationToken ct = default);
    Task<List<TaskResponse>> GetTasksBySiteAsync(Guid siteId, TaskStatus? status, Guid? assignedToUserId, CancellationToken ct = default);
    Task<List<TaskResponse>> GetOverdueTasksAsync(Guid siteId, CancellationToken ct = default);
    Task UpdateTaskAsync(Guid siteId, Guid taskId, UpdateTaskRequest request, Guid userId, CancellationToken ct = default);
    
    // Task lifecycle operations
    Task<TaskResponse> AssignTaskAsync(Guid siteId, Guid taskId, AssignTaskRequest request, Guid userId, CancellationToken ct = default);
    Task<TaskWithGatingResponse> StartTaskAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken ct = default);
    Task<TaskResponse> CompleteTaskAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken ct = default);
    Task<TaskResponse> CancelTaskAsync(Guid siteId, Guid taskId, CancelTaskRequest request, Guid userId, CancellationToken ct = default);
    
    // Task management
    Task<TaskResponse> UpdatePriorityAsync(Guid siteId, Guid taskId, TaskPriority priority, Guid userId, CancellationToken ct = default);
    Task<List<TaskStateHistory>> GetTaskHistoryAsync(Guid siteId, Guid taskId, CancellationToken ct = default);
    Task AddWatcherAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken ct = default);
    Task RemoveWatcherAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken ct = default);
    
    // Task dependencies
    Task AddDependencyAsync(Guid siteId, Guid taskId, Guid dependsOnTaskId, DependencyType type, Guid userId, CancellationToken ct = default);
    Task RemoveDependencyAsync(Guid siteId, Guid taskId, Guid dependsOnTaskId, Guid userId, CancellationToken ct = default);
}

/// <summary>
/// Service for resolving task gating with SOP/training requirements
/// </summary>
public interface ITaskGatingResolverService
{
    Task<TaskGatingResult> CheckTaskGatingAsync(Task task, Guid userId, CancellationToken ct = default);
    Task<List<Guid>> GetCompletedSopsForUserAsync(Guid userId, IReadOnlyCollection<Guid> requiredSopIds, CancellationToken ct = default);
    Task<List<Guid>> GetCompletedTrainingsForUserAsync(Guid userId, IReadOnlyCollection<Guid> requiredTrainingIds, CancellationToken ct = default);
}
```

### Task 1.3: Create DTOs (35 min)

**File:** `Application/DTOs/TaskDtos.cs`

```csharp
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.DTOs;

public record CreateTaskRequest(
    TaskType TaskType,
    string? CustomTaskType,
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    string? AssignedToRole,
    TaskPriority Priority,
    DateTimeOffset? DueDate,
    List<Guid>? RequiredSopIds,
    List<Guid>? RequiredTrainingIds,
    string? RelatedEntityType,
    Guid? RelatedEntityId
);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTimeOffset? DueDate
);

public record AssignTaskRequest(
    Guid? UserId,
    string? Role
);

public record CancelTaskRequest(
    string Reason
);

public record TaskResponse(
    Guid Id,
    Guid SiteId,
    TaskType TaskType,
    string? CustomTaskType,
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    string? AssignedToRole,
    Guid AssignedByUserId,
    TaskStatus Status,
    TaskPriority Priority,
    DateTimeOffset? DueDate,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? BlockingReason,
    List<Guid>? RequiredSopIds,
    List<Guid>? RequiredTrainingIds,
    string? RelatedEntityType,
    Guid? RelatedEntityId,
    bool IsOverdue,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record TaskWithGatingResponse(
    TaskResponse Task,
    TaskGatingResult GatingResult
);

public record TaskGatingResult(
    bool IsGated,
    List<string> BlockingReasons,
    List<Guid> MissingSopIds,
    List<Guid> MissingTrainingIds
);

public record TaskStateHistoryResponse(
    Guid Id,
    Guid TaskId,
    TaskStatus? FromStatus,
    TaskStatus ToStatus,
    Guid ChangedBy,
    DateTimeOffset ChangedAt,
    string? Reason
);
```

### Task 1.4: Implement Services (2.5 hours)

**File:** `Application/Services/TaskLifecycleService.cs`

**Pattern to follow:** Look at `identity/Application/Services/BadgeAuthService.cs` for:
- Constructor DI pattern
- Validation approach
- Exception handling
- Async/await usage

**Key implementation notes:**
- Validate task assignments (user or role, not both)
- Check dependencies before starting task
- Integrate with TaskGatingResolverService for SOP/training checks
- Emit events for watchers
- Use repository methods for data access

**Estimated lines:** ~500

---

**File:** `Application/Services/TaskGatingResolverService.cs`

**Key implementation notes:**
- Call FRP-01 repositories to check SOP completion
- Call FRP-01 repositories to check training completion
- Build comprehensive blocking reasons
- Return structured gating result

**Estimated lines:** ~250

### Task 1.5: Create Repositories (2.5 hours)

**File:** `Infrastructure/Persistence/TaskRepository.cs`

**Pattern to follow:** `identity/Infrastructure/Persistence/UserRepository.cs`

**Key methods:**
```csharp
- GetByIdAsync (with RLS context)
- GetByIdWithHistoryAsync (JOIN task_state_history)
- GetBySiteAndStatusAsync
- GetByAssignedUserAsync
- GetOverdueTasksAsync (WHERE due_date < NOW() AND status IN ('pending', 'in_progress'))
- GetByEntityAsync (for related entity queries)
- GetDependentTasksAsync (via task_dependencies)
- InsertAsync
- UpdateAsync
- DeleteAsync
```

**RLS Context:** 
```csharp
const string setRlsSql = "SET LOCAL app.current_user_id = @userId";
```

**Estimated lines:** ~350

---

**File:** `Infrastructure/Persistence/TaskDependencyRepository.cs` (~150 lines)
**File:** `Infrastructure/Persistence/TaskWatcherRepository.cs` (~150 lines)

### Task 1.6: Create Controller (1.5 hours)

**File:** `API/Controllers/TasksController.cs`

**Pattern to follow:** `identity/API/Controllers/UsersController.cs`

**Endpoints to implement:**
- POST /api/sites/{siteId}/tasks
- GET /api/sites/{siteId}/tasks
- GET /api/sites/{siteId}/tasks/{taskId}
- PUT /api/sites/{siteId}/tasks/{taskId}
- PUT /api/sites/{siteId}/tasks/{taskId}/assign
- POST /api/sites/{siteId}/tasks/{taskId}/start
- POST /api/sites/{siteId}/tasks/{taskId}/complete
- POST /api/sites/{siteId}/tasks/{taskId}/cancel
- PUT /api/sites/{siteId}/tasks/{taskId}/priority
- GET /api/sites/{siteId}/tasks/{taskId}/history
- POST /api/sites/{siteId}/tasks/{taskId}/watchers
- DELETE /api/sites/{siteId}/tasks/{taskId}/watchers/{userId}

**Estimated lines:** ~350

### Task 1.7: Create Validators (45 min)

**File:** `API/Validators/TaskValidators.cs`

**Pattern to follow:** `identity/API/Validators/UserRequestValidators.cs`

**Validators needed:**
- CreateTaskRequestValidator
- UpdateTaskRequestValidator
- AssignTaskRequestValidator
- CancelTaskRequestValidator

**Estimated lines:** ~200 total

### Task 1.8: Unit Tests (1.5 hours)

**File:** `Tests/Unit/Domain/TaskTests.cs`

**Tests needed:**
- Constructor validation
- Status transitions (Start, Complete, Cancel, Block)
- Gating checks
- Dependency validation
- Watcher management
- Overdue detection

**Estimated lines:** ~350

---

**File:** `Tests/Unit/Services/TaskLifecycleServiceTests.cs` (~300 lines)

### Task 1.9: Integration Tests (1.5 hours)

**File:** `Tests/Integration/TaskLifecycleIntegrationTests.cs`

**Tests needed:**
- Create task → assign → start → complete flow
- Task with dependencies (cannot start until dependency complete)
- Task with SOP requirements (blocked until SOP completed)
- Task with training requirements (blocked until training completed)
- RLS: cross-site task access blocked
- Watcher notifications

**Estimated lines:** ~400

### Slice 1 Summary

**Total Time:** 9-11 hours  
**Total Files:** 12 files  
**Total Lines:** ~2,800 lines

**Deliverables:**
- ✅ Service interface + implementation
- ✅ 3 repositories with RLS
- ✅ 1 controller with 12 endpoints
- ✅ 4 validator files
- ✅ 3 test files (unit + integration)

---

## 🔧 SLICE 2: MESSAGING & CONVERSATIONS

**Goal:** Complete messaging with threaded conversations  
**Time:** 6-8 hours  
**Dependencies:** None (can run in parallel with Slice 1)

### Quick Overview (Detailed steps similar to Slice 1)

**Service:** `ConversationService.cs` (~400 lines)

- CreateConversation, GetConversation, PostMessage
- AddParticipant, RemoveParticipant
- MarkMessageRead, GetUnreadCount
- ArchiveConversation, LockConversation

**Repository:** `ConversationRepository.cs` (~350 lines), `MessageRepository.cs` (~250 lines)

- Standard CRUD with RLS
- Message threading (parent_message_id)
- Read receipt tracking
- Participant management

**Controller:** `ConversationsController.cs` (~280 lines)

- POST /api/sites/{siteId}/conversations
- GET /api/sites/{siteId}/conversations/{conversationId}
- POST /api/sites/{siteId}/conversations/{conversationId}/messages
- GET /api/sites/{siteId}/conversations/{conversationId}/messages
- POST /api/sites/{siteId}/conversations/messages/{messageId}/read
- POST /api/sites/{siteId}/conversations/{conversationId}/participants

**Validators:** `ConversationValidators.cs` (~150 lines)

**Tests:** 3 files (~500 lines total)

- ConversationTests.cs (unit)
- ConversationServiceTests.cs (unit)
- ConversationIntegrationTests.cs (integration with RLS)

---

## 🔧 SLICE 3: SLACK INTEGRATION

**Goal:** Notify-only Slack bridge with queue and retry  
**Time:** 7-9 hours  
**Dependencies:** Slice 1 (Task events to notify)

### Quick Overview

**Service:** `SlackNotificationService.cs` (~450 lines)

- SendNotificationAsync (queue for delivery)
- GetChannelMappings, CreateChannelMapping
- GetFailedNotifications, RetryFailedNotification
- Format notification payloads for Slack API

**Worker:** `SlackNotificationWorker.cs` (~350 lines)

- Background service running every 5 seconds
- Fetch pending notifications from queue
- Call Slack API with retry/backoff
- Handle rate limiting (Tier 3: 50+ req/min)
- Dead-letter queue after max attempts

**Repositories:** 
- `SlackWorkspaceRepository.cs` (~200 lines)
- `SlackChannelMappingRepository.cs` (~200 lines)
- `SlackNotificationQueueRepository.cs` (~300 lines)

**Controller:** `SlackController.cs` (~200 lines)

- POST /api/sites/{siteId}/slack/workspaces
- GET /api/sites/{siteId}/slack/workspaces
- POST /api/sites/{siteId}/slack/channel-mappings
- GET /api/sites/{siteId}/slack/channel-mappings
- DELETE /api/sites/{siteId}/slack/channel-mappings/{mappingId}
- GET /api/sites/{siteId}/slack/failed-notifications
- POST /api/sites/{siteId}/slack/notifications/{notificationId}/retry

**External Client:** `SlackApiClient.cs` (~250 lines)

- OAuth2 token management
- Rate limiting with adaptive backoff
- Request-ID for idempotency
- Webhook formatting

**Validators:** `SlackValidators.cs` (~120 lines)

**Tests:** 3 files (~450 lines total)

- SlackNotificationTests.cs (unit - retry logic, backoff)
- SlackNotificationServiceTests.cs (unit)
- SlackIntegrationTests.cs (integration - mock Slack API)

---

## 📅 RECOMMENDED TIMELINE

### Day 1 (6 hours) - Foundation + Slice 1 Part 1

- Morning (1.5h): Pre-slice setup (rehydration factories, mapper profile, configuration)
- Midday (2h): Tasks 1.1-1.3 (folder structure, service interfaces, DTOs)
- Afternoon (2.5h): Begin Task 1.4 (service implementations)

### Day 2 (6 hours) - Slice 1 Part 2

- Morning (3h): Complete Task 1.4 (TaskLifecycleService + TaskGatingResolverService)
- Afternoon (3h): Task 1.5 (TaskRepository + TaskDependencyRepository + TaskWatcherRepository)

### Day 3 (5.5 hours) - Slice 1 Part 3

- Morning (2h): Task 1.6 (controller with explicit routing + DTO responses)
- Midday (1h): Task 1.7 (validators)
- Afternoon (2.5h): Tasks 1.8-1.9 (unit + integration tests)

### Day 4 (5 hours) - Slice 2

- Morning (2.5h): Conversation service + repositories
- Afternoon (2.5h): Conversations controller + validators + tests

### Day 5 (7 hours) - Slice 3

- Morning (3h): Slack service + worker + repositories
- Afternoon (2.5h): Slack controller + external client
- Evening (1.5h): Validators + integration tests with mocked Slack API

**Total:** 29.5 hours over 5 days (includes pre-slice setup, implementation, Slack integration, testing, and configuration/DI wiring)

---

## 🎯 Definition of Done

Each slice is complete when:

- ✅ Service implemented with all methods
- ✅ Repository implemented with RLS context
- ✅ Controller with all endpoints and OpenAPI docs
- ✅ FluentValidation validators
- ✅ Unit tests passing (≥90% coverage)
- ✅ Integration tests passing (including RLS verification)
- ✅ No linter errors
- ✅ Manual API testing via Swagger
- ✅ Program.cs, appsettings, and deployment secrets updated/validated

---

## 🔧 Helper Commands

### Run Tests

```bash
# Run all tests with coverage
dotnet test /p:CollectCoverage=true

# Run integration tests only
dotnet test --filter Category=Integration

# Run with test automation script
./scripts/test/run-with-local-postgres.sh
```

### Start API locally

```bash
cd src/backend/services/workflow-messaging/tasks/API
dotnet run
```

### Monitor Slack Queue

```bash
# Check queue depth
psql -c "SELECT status, COUNT(*) FROM slack_notification_queue GROUP BY status;"

# View failed notifications
psql -c "SELECT id, notification_type, last_error FROM slack_notification_queue WHERE status = 'failed' ORDER BY created_at DESC LIMIT 10;"
```

---

## 📋 Progress Tracking

After each task, update:

1. `docs/TRACK_B_COMPLETION_CHECKLIST.md` - Mark checkboxes
2. `docs/FRP04_CURRENT_STATUS.md` - Update % complete
3. Git commit with meaningful message

Example commit messages:

```
feat(frp04): implement task lifecycle service
feat(frp04): add task and conversation repositories with RLS
feat(frp04): add tasks controller with 12 endpoints
feat(frp04): implement slack notification worker with retry
test(frp04): add integration tests for task gating
```

---

## 🚦 Quality Gates

Before marking a slice complete, verify:

1. ✅ All tests passing
2. ✅ Code coverage ≥90% for services
3. ✅ RLS verification tests passing
4. ✅ Swagger UI loads without errors
5. ✅ Manual testing of happy path works
6. ✅ No linter warnings
7. ✅ Follows FRP-01/FRP-02/FRP-03 patterns

---

## ⚠️ Key Risks & Mitigations

- **Slack credentials not provisioned in time:** Block Slice 3 rollout behind a feature flag and coordinate with DevOps during Day 1 to confirm bot token storage so task events do not enqueue undeliverable notifications.
- **Slack token rotation:** Capture `TASKS_SLACK_REFRESH_TOKEN` from install and wire a refresh step into the Slack worker before production so short-lived access tokens don’t expire mid-run.
- **Gating data out of sync with FRP-01:** Add Day 2 checkpoint to import representative SOP/training fixtures and run the Slice 1 integration tests so blocked tasks surface actionable reasons rather than generic errors.
- **Notification throughput surprises:** During Slice 3 testing, simulate 50+ concurrent task events and watch queue metrics; tune retry/backoff thresholds before promoting to staging.

---

## 🎯 Success Metrics

FRP-04 is successfully complete when:

- ✅ All 3 slices delivered and tested
- ✅ All checklist items marked complete
- ✅ All acceptance criteria met
- ✅ Performance targets met (Slack notify p95 < 2s)
- ✅ Documentation updated
- ✅ Ready for FRP-06 (Irrigation) and FRP-15 (Notifications)

---

**Ready to start?** Begin with **Pre-Slice Setup** then **Slice 1, Task 1.1** 🚀
