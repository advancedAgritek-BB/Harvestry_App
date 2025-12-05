# FRP-04: Tasks, Messaging & Slack Integration - Implementation Plan

**Status:** ✅ COMPLETE  
**Estimated Effort:** 22-28 hours  
**Prerequisites:** ✅ FRP-01 Complete (Identity, RLS, ABAC)  
**Blocks:** None

---

## 📋 OVERVIEW

### Purpose
Establish a comprehensive task workflow engine with messaging capabilities and Slack integration for notifications. This system enables task management, dependency tracking, and team communication with compliance gating integrated from FRP-01.

### Key Features
1. **Task Lifecycle Management** - Create, assign, start, complete, and cancel tasks
2. **Task Dependencies** - Chain tasks with prerequisite validation
3. **Compliance Gating** - Integration with SOP/training requirements from FRP-01
4. **Conversation Threading** - Contextual messaging around tasks and objects
5. **Slack Bridge** - Notify-only integration (outbound notifications)
6. **Task Watchers** - Subscribe to task updates
7. **Audit Trail** - Complete history of task state changes

### Acceptance Criteria (from PRD)
- ✅ Task events notify Slack with p95 < 2s
- ✅ Blocked tasks show explicit reasons
- ✅ Task gating works end-to-end with SOP/training checks
- ✅ Message threads properly associate with tasks
- ✅ RLS blocks cross-site task access

---

## 📊 IMPLEMENTATION BREAKDOWN

### Phase 1: Database Schema (3-4 hours)

#### Migration 1: Task Management
**File:** `src/database/migrations/frp04/20251015_01_CreateTaskTables.sql`

**Highlights:**
- Creates `tasks`, `task_state_history`, `task_dependencies`, `task_watchers`, `task_time_entries`, `task_required_sops`, `task_required_training`
- Normalizes SOP/training requirements via join tables (no UUID arrays)
- Adds updated-at trigger plus indexes on `(site_id, status)`, `(site_id, assigned_to_user_id)`, and `due_date`
- Enables RLS across task tables with site-scoped policies and admin/service_account overrides

#### Migration 2: Messaging & Conversations
**File:** `src/database/migrations/frp04/20251015_02_CreateMessagingTables.sql`

**Status:** ✅ Completed (Oct 2) — conversations/messages schema with RLS committed

**Highlights:**
- Adds `conversations`, `conversation_participants`, `messages`, `message_attachments`, `message_read_receipts` with UUID PKs
- Uses `SMALLINT` enum columns for conversation type/status, participant role, attachment type to align with domain enums
- Establishes updated-at triggers for conversations/messages and cascades for dependent records
- Enables RLS on every table, inheriting site-scoped policies from `conversations`

#### Migration 3: Slack Integration
**File:** `src/database/migrations/frp04/20251015_03_CreateSlackIntegrationTables.sql`

**Tables:**
```sql
-- Slack Workspaces (multi-tenant support)
slack_workspaces (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    workspace_id varchar(100) NOT NULL,
    workspace_name varchar(200) NOT NULL,
    bot_token_encrypted text NOT NULL,
    bot_user_id varchar(100),
    is_active boolean NOT NULL DEFAULT TRUE,
    installed_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    installed_by uuid NOT NULL REFERENCES users(id),
    last_verified_at timestamptz,
    UNIQUE(site_id, workspace_id)
)

-- Slack Channel Mappings (notification routing)
slack_channel_mappings (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    workspace_id uuid NOT NULL REFERENCES slack_workspaces(id) ON DELETE CASCADE,
    channel_id varchar(100) NOT NULL,
    channel_name varchar(200) NOT NULL,
    notification_type varchar(50) NOT NULL CHECK (notification_type IN (
        'task_created', 'task_assigned', 'task_completed', 'task_overdue',
        'task_blocked', 'conversation_mention', 'alert_critical', 'alert_warning'
    )),
    is_active boolean NOT NULL DEFAULT TRUE,
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(workspace_id, channel_id, notification_type)
)

-- Slack Message Bridge Log (idempotent tracking)
slack_message_bridge_log (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    workspace_id uuid NOT NULL REFERENCES slack_workspaces(id),
    internal_message_id uuid,
    internal_message_type varchar(50) NOT NULL,
    slack_channel_id varchar(100) NOT NULL,
    slack_message_ts varchar(50) NOT NULL,
    slack_thread_ts varchar(50),
    request_id varchar(100) NOT NULL,
    status varchar(20) NOT NULL DEFAULT 'pending' CHECK (status IN (
        'pending', 'sent', 'failed', 'retrying'
    )),
    attempt_count int NOT NULL DEFAULT 0,
    last_attempt_at timestamptz,
    error_message text,
    sent_at timestamptz,
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(workspace_id, slack_message_ts)
)

-- Slack Notification Queue (outbox pattern)
slack_notification_queue (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    workspace_id uuid NOT NULL REFERENCES slack_workspaces(id),
    notification_type varchar(50) NOT NULL,
    channel_id varchar(100) NOT NULL,
    payload jsonb NOT NULL,
    request_id varchar(100) NOT NULL,
    status varchar(20) NOT NULL DEFAULT 'pending' CHECK (status IN (
        'pending', 'processing', 'sent', 'failed', 'dead_letter'
    )),
    priority int NOT NULL DEFAULT 5 CHECK (priority BETWEEN 1 AND 10),
    attempt_count int NOT NULL DEFAULT 0,
    max_attempts int NOT NULL DEFAULT 3,
    next_attempt_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_error text,
    created_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP
)

CREATE INDEX idx_slack_queue_next_attempt ON slack_notification_queue(next_attempt_at, status)
    WHERE status IN ('pending', 'failed');
```

**RLS Policies:**
```sql
-- Enable RLS on all tables
ALTER TABLE tasks ENABLE ROW LEVEL SECURITY;
ALTER TABLE task_dependencies ENABLE ROW LEVEL SECURITY;
ALTER TABLE task_watchers ENABLE ROW LEVEL SECURITY;
ALTER TABLE task_state_history ENABLE ROW LEVEL SECURITY;
ALTER TABLE task_time_entries ENABLE ROW LEVEL SECURITY;
ALTER TABLE conversations ENABLE ROW LEVEL SECURITY;
ALTER TABLE messages ENABLE ROW LEVEL SECURITY;
ALTER TABLE message_attachments ENABLE ROW LEVEL SECURITY;
ALTER TABLE conversation_participants ENABLE ROW LEVEL SECURITY;
ALTER TABLE message_read_receipts ENABLE ROW LEVEL SECURITY;
ALTER TABLE slack_workspaces ENABLE ROW LEVEL SECURITY;
ALTER TABLE slack_channel_mappings ENABLE ROW LEVEL SECURITY;
ALTER TABLE slack_message_bridge_log ENABLE ROW LEVEL SECURITY;
ALTER TABLE slack_notification_queue ENABLE ROW LEVEL SECURITY;

-- Policy: Users can only access their site's tasks
CREATE POLICY tasks_site_access ON tasks
    FOR ALL
    USING (
        site_id::text = current_setting('app.site_id', TRUE)
        OR current_setting('app.user_role', TRUE) = 'admin'
        OR current_setting('app.user_role', TRUE) = 'service_account'
    );

-- Repeat for all other tables with appropriate permissions
```

**Indexes:**
```sql
CREATE INDEX idx_tasks_site_assigned ON tasks(site_id, assigned_to_user_id)
    WHERE status IN ('pending', 'in_progress');
CREATE INDEX idx_tasks_site_status ON tasks(site_id, status);
CREATE INDEX idx_tasks_due_date ON tasks(due_date)
    WHERE status IN ('pending', 'in_progress');
CREATE INDEX idx_task_dependencies_task ON task_dependencies(task_id);
CREATE INDEX idx_task_dependencies_depends_on ON task_dependencies(depends_on_task_id);
CREATE INDEX idx_task_watchers_user ON task_watchers(user_id);
CREATE INDEX idx_messages_conversation ON messages(conversation_id, created_at DESC);
CREATE INDEX idx_messages_sender ON messages(sender_user_id);
CREATE INDEX idx_message_attachments_message ON message_attachments(message_id);
CREATE INDEX idx_conversation_participants_user ON conversation_participants(user_id);
CREATE INDEX idx_slack_bridge_log_internal ON slack_message_bridge_log(internal_message_id);
CREATE INDEX idx_slack_channel_mappings_active ON slack_channel_mappings(workspace_id, is_active)
    WHERE is_active = TRUE;
```

---

### Phase 2: Domain Layer (4-5 hours)

#### Domain Entities

**File Structure:**
```
src/backend/services/workflow-messaging/tasks/Domain/
├── Entities/
│   ├── Task.cs
│   ├── TaskDependency.cs
│   ├── TaskWatcher.cs
│   ├── TaskStateHistory.cs
│   ├── TaskTimeEntry.cs
│   ├── Conversation.cs
│   ├── Message.cs
│   ├── MessageAttachment.cs
│   ├── ConversationParticipant.cs
│   ├── SlackWorkspace.cs
│   ├── SlackChannelMapping.cs
│   └── SlackNotification.cs
├── ValueObjects/
│   ├── TaskGatingResult.cs
│   ├── TaskDependencyResult.cs
│   ├── SlackMessagePayload.cs
│   └── NotificationPreferences.cs
└── Enums/
    ├── TaskType.cs
    ├── TaskStatus.cs
    ├── TaskPriority.cs
    ├── DependencyType.cs
    ├── ConversationType.cs
    ├── MessageAttachmentType.cs
    ├── NotificationType.cs
    └── ParticipantRole.cs
```

**Key Domain Methods:**

**Task.cs:**
```csharp
public class Task : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public TaskType TaskType { get; private set; }
    public string? CustomTaskType { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public string? AssignedToRole { get; private set; }
    public Guid AssignedByUserId { get; private set; }
    public TaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? BlockingReason { get; private set; }
    public IReadOnlyCollection<Guid> RequiredSopIds { get; private set; }
    public IReadOnlyCollection<Guid> RequiredTrainingIds { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }

    private readonly List<TaskStateHistory> _stateHistory = new();
    public IReadOnlyCollection<TaskStateHistory> StateHistory => _stateHistory.AsReadOnly();

    // Methods
    public void Assign(Guid? userId, string? role, Guid assignedBy);
    public TaskGatingResult CheckGating(IReadOnlyCollection<Guid> completedSopIds, IReadOnlyCollection<Guid> completedTrainingIds);
    public TaskDependencyResult CheckDependencies(IReadOnlyCollection<Task> dependentTasks);
    public void Start(Guid userId);
    public void Block(string reason, Guid userId);
    public void Unblock(Guid userId);
    public void Complete(Guid userId);
    public void Cancel(string reason, Guid userId);
    public void UpdatePriority(TaskPriority priority, Guid userId);
    public void UpdateDueDate(DateTimeOffset? dueDate, Guid userId);
    public void AddStateHistory(TaskStatus fromStatus, TaskStatus toStatus, Guid changedBy, string? reason = null);
    public bool IsOverdue();
    public bool CanStart(TaskGatingResult gatingResult, TaskDependencyResult dependencyResult);
    public TimeSpan? GetTimeToComplete();
    public static Task FromPersistence(...);
}
```

**Conversation.cs:**
```csharp
public class Conversation : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public ConversationType ConversationType { get; private set; }
    public string? Title { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public ConversationStatus Status { get; private set; }
    public DateTimeOffset? LastMessageAt { get; private set; }

    private readonly List<Message> _messages = new();
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    private readonly List<ConversationParticipant> _participants = new();
    public IReadOnlyCollection<ConversationParticipant> Participants => _participants.AsReadOnly();

    // Methods
    public void AddParticipant(Guid userId, ParticipantRole role, Guid addedBy);
    public void RemoveParticipant(Guid userId, Guid removedBy);
    public Message PostMessage(Guid senderUserId, string content, Guid? parentMessageId = null);
    public void Archive(Guid archivedBy);
    public void Lock(Guid lockedBy);
    public void Unlock(Guid unlockedBy);
    public bool CanUserPost(Guid userId);
    public int GetUnreadCount(Guid userId);
    public static Conversation FromPersistence(...);
}
```

**SlackNotification.cs:**
```csharp
public class SlackNotification : Entity<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public string ChannelId { get; private set; }
    public SlackMessagePayload Payload { get; private set; }
    public string RequestId { get; private set; }
    public NotificationStatus Status { get; private set; }
    public int Priority { get; private set; }
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; }
    public DateTimeOffset NextAttemptAt { get; private set; }
    public string? LastError { get; private set; }

    // Methods
    public void MarkProcessing();
    public void MarkSent();
    public void MarkFailed(string error);
    public void MarkDeadLetter();
    public void IncrementAttempt();
    public bool CanRetry();
    public void ScheduleRetry(TimeSpan backoff);
    public static SlackNotification FromPersistence(...);
}
```

**TaskGatingResult.cs (Value Object):**
```csharp
public readonly record struct TaskGatingResult(
    bool IsGated,
    IReadOnlyCollection<string> BlockingReasons,
    IReadOnlyCollection<Guid> MissingSopIds,
    IReadOnlyCollection<Guid> MissingTrainingIds
)
{
    public static TaskGatingResult NotGated() => new(false, Array.Empty<string>(), Array.Empty<Guid>(), Array.Empty<Guid>());
    public static TaskGatingResult Gated(IReadOnlyCollection<Guid> missingSops, IReadOnlyCollection<Guid> missingTrainings, IReadOnlyCollection<string> reasons) 
        => new(true, reasons, missingSops, missingTrainings);
}
```

---

### Phase 3: Application Layer (3-4 hours)

#### Application Services

**Files:**
```
src/backend/services/workflow-messaging/tasks/Application/
├── Services/
│   ├── TaskLifecycleService.cs
│   ├── TaskGatingResolverService.cs
│   ├── TaskDependencyService.cs
│   ├── ConversationService.cs
│   └── SlackNotificationService.cs
├── DTOs/
│   ├── CreateTaskRequest.cs
│   ├── UpdateTaskRequest.cs
│   ├── AssignTaskRequest.cs
│   ├── StartTaskRequest.cs
│   ├── CompleteTaskRequest.cs
│   ├── CancelTaskRequest.cs
│   ├── TaskResponse.cs
│   ├── TaskWithGatingResponse.cs
│   ├── CreateConversationRequest.cs
│   ├── PostMessageRequest.cs
│   ├── ConversationResponse.cs
│   ├── MessageResponse.cs
│   ├── SlackWorkspaceRequest.cs
│   ├── SlackChannelMappingRequest.cs
│   └── NotificationSettingsResponse.cs
└── Interfaces/
    ├── ITaskLifecycleService.cs
    ├── ITaskGatingResolverService.cs
    ├── IConversationService.cs
    └── ISlackNotificationService.cs
```

**Key Service Methods:**

**ITaskLifecycleService:**
```csharp
public interface ITaskLifecycleService
{
    Task<TaskResponse> CreateTaskAsync(Guid siteId, CreateTaskRequest request, Guid userId, CancellationToken ct);
    Task<TaskResponse?> GetTaskByIdAsync(Guid siteId, Guid taskId, CancellationToken ct);
    Task<IReadOnlyList<TaskResponse>> GetTasksBySiteAsync(Guid siteId, TaskStatus? status, Guid? assignedToUserId, CancellationToken ct);
    Task<IReadOnlyList<TaskResponse>> GetOverdueTasksAsync(Guid siteId, CancellationToken ct);
    Task<TaskResponse> AssignTaskAsync(Guid siteId, Guid taskId, AssignTaskRequest request, Guid userId, CancellationToken ct);
    Task<TaskWithGatingResponse> StartTaskAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken ct);
    Task<TaskResponse> CompleteTaskAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken ct);
    Task<TaskResponse> CancelTaskAsync(Guid siteId, Guid taskId, CancelTaskRequest request, Guid userId, CancellationToken ct);
    Task<TaskResponse> UpdatePriorityAsync(Guid siteId, Guid taskId, TaskPriority priority, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<TaskStateHistory>> GetTaskHistoryAsync(Guid siteId, Guid taskId, CancellationToken ct);
    Task AddWatcherAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken ct);
    Task RemoveWatcherAsync(Guid siteId, Guid taskId, Guid userId, CancellationToken ct);
}
```

**ITaskGatingResolverService:**
```csharp
public interface ITaskGatingResolverService
{
    Task<TaskGatingResult> CheckTaskGatingAsync(Task task, Guid userId, CancellationToken ct);
    Task<IReadOnlyCollection<Guid>> GetCompletedSopsForUserAsync(Guid userId, IReadOnlyCollection<Guid> requiredSopIds, CancellationToken ct);
    Task<IReadOnlyCollection<Guid>> GetCompletedTrainingsForUserAsync(Guid userId, IReadOnlyCollection<Guid> requiredTrainingIds, CancellationToken ct);
}
```

**IConversationService:**
```csharp
public interface IConversationService
{
    Task<ConversationResponse> CreateConversationAsync(Guid siteId, CreateConversationRequest request, Guid userId, CancellationToken ct);
    Task<ConversationResponse?> GetConversationByIdAsync(Guid siteId, Guid conversationId, CancellationToken ct);
    Task<IReadOnlyList<ConversationResponse>> GetConversationsByEntityAsync(Guid siteId, string entityType, Guid entityId, CancellationToken ct);
    Task<MessageResponse> PostMessageAsync(Guid siteId, Guid conversationId, PostMessageRequest request, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(Guid siteId, Guid conversationId, int? limit, DateTimeOffset? since, CancellationToken ct);
    Task MarkMessageReadAsync(Guid siteId, Guid messageId, Guid userId, CancellationToken ct);
    Task AddParticipantAsync(Guid siteId, Guid conversationId, Guid userId, Guid addedBy, CancellationToken ct);
    Task RemoveParticipantAsync(Guid siteId, Guid conversationId, Guid userId, Guid removedBy, CancellationToken ct);
}
```

**ISlackNotificationService:**
```csharp
public interface ISlackNotificationService
{
    Task<string> SendNotificationAsync(Guid siteId, NotificationType type, object payload, CancellationToken ct);
    Task<IReadOnlyList<SlackChannelMapping>> GetChannelMappingsAsync(Guid siteId, CancellationToken ct);
    Task<SlackChannelMapping> CreateChannelMappingAsync(Guid siteId, SlackChannelMappingRequest request, Guid userId, CancellationToken ct);
    Task DeleteChannelMappingAsync(Guid siteId, Guid mappingId, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<SlackNotification>> GetFailedNotificationsAsync(Guid siteId, CancellationToken ct);
    Task RetryFailedNotificationAsync(Guid notificationId, CancellationToken ct);
}
```

---

### Phase 4: Infrastructure Layer (3-4 hours)

#### Repositories

**Files:**
```
src/backend/services/workflow-messaging/tasks/Infrastructure/Persistence/
├── TasksDbContext.cs
├── TaskRepository.cs
├── TaskDependencyRepository.cs
├── TaskWatcherRepository.cs
├── TaskStateHistoryRepository.cs
├── ConversationRepository.cs
├── MessageRepository.cs
├── SlackWorkspaceRepository.cs
├── SlackChannelMappingRepository.cs
└── SlackNotificationQueueRepository.cs
```

**Key Repository Methods:**

**ITaskRepository:**
```csharp
public interface ITaskRepository : IRepository<Task, Guid>
{
    Task<Task?> GetByIdWithHistoryAsync(Guid siteId, Guid id, CancellationToken ct);
    Task<IReadOnlyList<Task>> GetBySiteAndStatusAsync(Guid siteId, TaskStatus? status, CancellationToken ct);
    Task<IReadOnlyList<Task>> GetByAssignedUserAsync(Guid siteId, Guid userId, TaskStatus? status, CancellationToken ct);
    Task<IReadOnlyList<Task>> GetOverdueTasksAsync(Guid siteId, CancellationToken ct);
    Task<IReadOnlyList<Task>> GetByEntityAsync(Guid siteId, string entityType, Guid entityId, CancellationToken ct);
    Task<IReadOnlyList<Task>> GetDependentTasksAsync(Guid siteId, Guid taskId, CancellationToken ct);
}
```

**IConversationRepository:**
```csharp
public interface IConversationRepository
{
    Task AddAsync(Conversation conversation, CancellationToken ct);
    Task UpdateAsync(Conversation conversation, CancellationToken ct);
    Task<Conversation?> GetByIdAsync(Guid siteId, Guid id, CancellationToken ct);
    Task<IReadOnlyList<Conversation>> GetBySiteAsync(Guid siteId, ConversationType? type, CancellationToken ct);
    Task<IReadOnlyList<Conversation>> GetByRelatedEntityAsync(Guid siteId, string entityType, Guid entityId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
```

> ✅ Base repository implementation + unit coverage landed (`ConversationRepository`, Oct 2)

**ISlackNotificationQueueRepository:**
```csharp
public interface ISlackNotificationQueueRepository
{
    Task<SlackNotification> EnqueueAsync(SlackNotification notification, CancellationToken ct);
    Task<IReadOnlyList<SlackNotification>> GetPendingNotificationsAsync(int batchSize, CancellationToken ct);
    Task UpdateAsync(SlackNotification notification, CancellationToken ct);
    Task<IReadOnlyList<SlackNotification>> GetFailedNotificationsAsync(Guid siteId, CancellationToken ct);
}
```

---

### Phase 5: API Layer (2-3 hours)

#### Controllers

**Files:**
```
src/backend/services/workflow-messaging/tasks/API/Controllers/
├── TasksController.cs
├── ConversationsController.cs
└── SlackController.cs
```

**Key Endpoints:**

**TasksController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/tasks")]
public class TasksController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TaskResponse>> CreateTask(Guid siteId, CreateTaskRequest request);
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetTasks(Guid siteId, [FromQuery] TaskStatus? status, [FromQuery] Guid? assignedTo);
    
    [HttpGet("{taskId}")]
    public async Task<ActionResult<TaskResponse>> GetTask(Guid siteId, Guid taskId);
    
    [HttpPut("{taskId}/assign")]
    public async Task<ActionResult<TaskResponse>> AssignTask(Guid siteId, Guid taskId, AssignTaskRequest request);
    
    [HttpPost("{taskId}/start")]
    public async Task<ActionResult<TaskWithGatingResponse>> StartTask(Guid siteId, Guid taskId);
    
    [HttpPost("{taskId}/complete")]
    public async Task<ActionResult<TaskResponse>> CompleteTask(Guid siteId, Guid taskId);
    
    [HttpPost("{taskId}/cancel")]
    public async Task<ActionResult<TaskResponse>> CancelTask(Guid siteId, Guid taskId, CancelTaskRequest request);
    
    [HttpPut("{taskId}/priority")]
    public async Task<ActionResult<TaskResponse>> UpdatePriority(Guid siteId, Guid taskId, [FromBody] TaskPriority priority);
    
    [HttpGet("{taskId}/history")]
    public async Task<ActionResult<IReadOnlyList<TaskStateHistory>>> GetHistory(Guid siteId, Guid taskId);
    
    [HttpPost("{taskId}/watchers")]
    public async Task<IActionResult> AddWatcher(Guid siteId, Guid taskId, [FromBody] Guid userId);
    
    [HttpDelete("{taskId}/watchers/{userId}")]
    public async Task<IActionResult> RemoveWatcher(Guid siteId, Guid taskId, Guid userId);
}
```

**ConversationsController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/conversations")]
public class ConversationsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ConversationResponse>> CreateConversation(Guid siteId, CreateConversationRequest request);
    
    [HttpGet("{conversationId}")]
    public async Task<ActionResult<ConversationResponse>> GetConversation(Guid siteId, Guid conversationId);
    
    [HttpPost("{conversationId}/messages")]
    public async Task<ActionResult<MessageResponse>> PostMessage(Guid siteId, Guid conversationId, PostMessageRequest request);
    
    [HttpGet("{conversationId}/messages")]
    public async Task<ActionResult<IReadOnlyList<MessageResponse>>> GetMessages(Guid siteId, Guid conversationId, [FromQuery] int? limit);
    
    [HttpPost("messages/{messageId}/read")]
    public async Task<IActionResult> MarkMessageRead(Guid siteId, Guid messageId);
    
    [HttpPost("{conversationId}/participants")]
    public async Task<IActionResult> AddParticipant(Guid siteId, Guid conversationId, [FromBody] Guid userId);
}
```

---

### Phase 6: Validators (1 hour)

**Files:**
```
src/backend/services/workflow-messaging/tasks/API/Validators/
├── CreateTaskRequestValidator.cs
├── UpdateTaskRequestValidator.cs
├── AssignTaskRequestValidator.cs
├── PostMessageRequestValidator.cs
└── SlackChannelMappingRequestValidator.cs
```

---

### Phase 7: Background Workers (2 hours)

**Files:**
```
src/backend/services/workflow-messaging/tasks/Infrastructure/Workers/
├── SlackNotificationWorker.cs
├── TaskOverdueMonitorWorker.cs
└── TaskDependencyResolverWorker.cs
```

**SlackNotificationWorker.cs:**
- Processes notification queue every 5 seconds
- Exponential backoff for retries
- Dead-letter queue after max attempts
- Rate limiting per workspace

---

### Phase 8: Unit Tests (2-3 hours)

**Files:**
```
src/backend/services/workflow-messaging/tasks/Tests/Unit/
├── Domain/
│   ├── TaskTests.cs
│   ├── TaskDependencyTests.cs
│   ├── ConversationTests.cs
│   └── SlackNotificationTests.cs
└── Services/
    ├── TaskLifecycleServiceTests.cs
    ├── TaskGatingResolverServiceTests.cs
    ├── ConversationServiceTests.cs
    └── SlackNotificationServiceTests.cs
```

---

### Phase 9: Integration Tests (2-3 hours)

**Files:**
```
src/backend/services/workflow-messaging/tasks/Tests/Integration/
├── TaskLifecycleIntegrationTests.cs
├── TaskGatingIntegrationTests.cs
├── ConversationIntegrationTests.cs
├── SlackIntegrationTests.cs
└── RlsTasksTests.cs
```

---

## 📊 TASK BREAKDOWN WITH ESTIMATES

| Phase | Task | Est. Hours | Owner |
|-------|------|------------|-------|
| **1. Database** | Migration 1: Tasks | 1.5-2 | Backend |
| | Migration 2: Messaging | 1-1.5 | Backend |
| | Migration 3: Slack | 0.5-1 | Backend |
| **2. Domain** | 12 entity files | 2-2.5 | Backend |
| | 4 value object files | 0.5-1 | Backend |
| | 8 enum files | 0.5-0.75 | Backend |
| | Domain logic methods | 1.5-2 | Backend |
| **3. Application** | 5 service implementations | 2-2.5 | Backend |
| | 15 DTO files | 1.5-2 | Backend |
| | 4 interface files | 0.5-0.75 | Backend |
| **4. Infrastructure** | DbContext + 9 repositories | 2-2.5 | Backend |
| | RLS context integration | 0.75-1 | Backend |
| | Connection/retry logic | 0.5 | Backend |
| **5. API** | 3 controllers (~450 lines) | 1.5-2 | Backend |
| | Program.cs DI registration | 0.5 | Backend |
| **6. Validators** | 5 validator files | 1 | Backend |
| **7. Workers** | 3 background worker files | 2 | Backend |
| **8. Unit Tests** | 8 test files | 2-2.5 | Backend |
| **9. Integration Tests** | 5 test files | 2-2.5 | Backend |
| **TOTAL** | | **22-28** | |

---

## ✅ QUALITY GATES

1. ✅ All repositories with RLS
2. ✅ Unit test coverage ≥90%
3. ✅ API endpoints operational
4. ✅ Integration tests passing
5. ✅ Health checks configured
6. ✅ Swagger documentation
7. ✅ Production polish (CORS, validators, logging)
8. ✅ Acceptance criteria met

---

## 🎯 ACCEPTANCE CRITERIA VALIDATION

### From PRD:
- ✅ **Task events notify Slack p95 < 2s** - Implemented via queue worker with monitoring
- ✅ **Blocked tasks show explicit reasons** - TaskGatingResult provides detailed blocking info
- ✅ **Task gating works end-to-end** - Integration with FRP-01 SOP/training system
- ✅ **Message threads associate properly** - Conversation/message relationship tracking
- ✅ **RLS blocks cross-site access** - Integration tests validate

---

## 🚀 DEPENDENCIES & BLOCKING

### Prerequisites (All Met ✅)
- ✅ FRP-01 Complete (Identity, RLS, ABAC, SOP/Training system)

### Blocks (After FRP-04 Complete)
- **FRP-06: Irrigation** - Needs task system for workflow orchestration
- **FRP-15: Notifications** - Builds on notification infrastructure

---

## 📝 DESIGN DECISIONS

1. **Slack Integration:** ✅ **Notify-only (outbound)** - Phase 1 is one-way notifications; interactive bot deferred
2. **Message Storage:** ✅ **Full history in database** - Slack is notification channel, not system of record
3. **Queue Pattern:** ✅ **Outbox with retry** - Guaranteed delivery with idempotency via request-id
4. **Task Gating:** ✅ **Integration with FRP-01** - Reuse existing SOP/training completion tracking
5. **Conversation Threading:** ✅ **Flexible entity association** - Can attach to any entity type
6. **Credential Storage:** ✅ **AWS Secrets Manager (`slack_tasks_dev`)** - Store bot access + refresh tokens, workspace ID, and client credentials for runtime retrieval and token refresh

---

## 🎯 SUCCESS CRITERIA

**Definition of Done:**
- ✅ All 8 quality gates passed
- ✅ Task lifecycle operational
- ✅ Gating integration with FRP-01 working
- ✅ Slack notifications sending
- ✅ Message threading functional
- ✅ RLS validated (cross-site blocked)
- ✅ Integration tests passing
- ✅ Swagger docs published

**Expected Outcome:**
- 40-50 C# files created
- ~3,500-4,500 lines of code
- Complete task workflow engine
- Production-ready API
- FRP-06, FRP-15 unblocked

---

## ⚠️ RISKS & MITIGATIONS

- **Slack credential management:** Workspace bot tokens must be stored in KMS/secrets manager before enabling Slice 3; coordinate with DevOps to provision secrets and add feature flags so task events do not enqueue Slack notifications until tokens exist.
- **Slack token rotation:** Slack now issues short-lived bot tokens; store the refresh token in AWS Secrets Manager (`TASKS_SLACK_REFRESH_TOKEN`) and run the `SlackTokenRefreshJob` so bot tokens refresh automatically and updates persist back to the secret.
- **Gating data freshness:** Task gating depends on FRP-01 SOP/training completion data; keep `20251001_01_SeedTrainingAndTaskFixtures.sql` applied in lower envs and schedule end-to-end smoke tests so prerequisites stay realistic and false blocks surface early.
- **Notification volume spikes:** Large watcher lists can burst Slack API calls; implement queue backoff metrics, add per-site rate-limit guardrails, and monitor `slack_notification_queue` saturation in staging prior to enabling production.

---

**Status:** ⏳ READY FOR REVIEW & APPROVAL  
**Next Step:** Review plan → Get approval → Begin implementation  
**Estimated Completion:** 22-28 hours from start
