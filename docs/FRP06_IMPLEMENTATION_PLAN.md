# FRP-06 Implementation Plan — Irrigation Orchestration & HIL Validation

**Version:** 1.0  
**Date:** October 7, 2025  
**Status:** 📋 Planning Complete, Ready for Development  
**Owner:** Telemetry & Controls Squad  
**Dependencies:** FRP-02 (Spatial ✅), FRP-05 (Telemetry 93%)

---

## Executive Summary

FRP-06 delivers production-safe irrigation/fertigation control with:
- **Groups/Programs/Schedules** - Time-based and sensor-triggered irrigation
- **Safety Interlocks** - 7 hardware interlocks (E-STOP, door, tank level, EC/pH, CO₂, max runtime, concurrency)
- **Command Orchestration** - MQTT command queue with acknowledgment tracking
- **Abort Saga** - Safe abort compensator on interlock trip
- **HIL Validation** - Hardware-In-Loop chaos drills with firmware sign-off gate

**Critical Path:** FRP-05 load testing → FRP-06 development → Golden Harness build → HIL drills → Firmware sign-off

**Estimated Effort:** 24-28 hours development + 80 hours golden harness + HIL validation

---

## Architecture Overview

### Clean Architecture Layers

```
FRP-06 Irrigation/
├── Domain/
│   ├── Entities/
│   │   ├── IrrigationGroup.cs           # Aggregate root (zones, valves, pump)
│   │   ├── IrrigationProgram.cs         # Aggregate root (steps, schedules)
│   │   ├── IrrigationSchedule.cs        # Time/sensor triggers
│   │   ├── IrrigationRun.cs             # Run instance (audit trail)
│   │   ├── IrrigationStep.cs            # Step (shot/cycle-soak/flush)
│   │   ├── IrrigationStepRun.cs         # Step execution record
│   │   ├── InterlockEvent.cs            # Interlock trip record
│   │   ├── DeviceCommand.cs             # Command queue item
│   │   ├── MixTank.cs                   # Nutrient tank
│   │   └── InjectorChannel.cs           # Nutrient injector
│   ├── ValueObjects/
│   │   ├── StepDuration.cs              # Time-based shot
│   │   ├── TargetVolume.cs              # Volume-based shot
│   │   ├── EcPhTarget.cs                # Nutrient targets
│   │   ├── InterlockReason.cs           # Interlock trip reason
│   │   └── CommandAck.cs                # Device acknowledgment
│   └── Enums/
│       ├── RunStatus.cs                 # Queued/Running/Completed/Aborted/Faulted
│       ├── StepType.cs                  # Shot/CycleSoak/Flush
│       ├── InterlockType.cs             # EStop/Door/TankLow/EcPh/CO2/Runtime/Concurrency
│       ├── CommandStatus.cs             # Pending/Sent/Acked/Failed
│       ├── CommandPriority.cs           # Normal/Emergency
│       └── CommandScope.cs              # Device/Zone/Broadcast
├── Application/
│   ├── Services/
│   │   ├── IrrigationOrchestratorService.cs    # Command queue + dispatch
│   │   ├── RunExecutionService.cs              # Step sequencing + ack gating
│   │   ├── ScheduleTriggerService.cs           # Time/sensor trigger evaluation
│   │   ├── ManualApprovalService.cs            # Optional ABAC gating
│   │   ├── InterlockEvaluationService.cs       # Safety checks
│   │   └── NutrientStockService.cs             # Stock deduction
│   ├── Sagas/
│   │   └── IrrigationAbortSaga.cs              # Safe abort compensator
│   ├── DTOs/
│   │   ├── CreateIrrigationGroupRequest.cs
│   │   ├── CreateIrrigationProgramRequest.cs
│   │   ├── StartIrrigationRunRequest.cs
│   │   ├── AbortIrrigationRunRequest.cs
│   │   └── IrrigationRunResponse.cs
│   └── Validators/
│       ├── CreateIrrigationGroupValidator.cs
│       ├── CreateIrrigationProgramValidator.cs
│       └── StartIrrigationRunValidator.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── IrrigationDbContext.cs              # EF Core context
│   │   ├── IrrigationGroupRepository.cs
│   │   ├── IrrigationProgramRepository.cs
│   │   ├── IrrigationRunRepository.cs
│   │   ├── DeviceCommandRepository.cs
│   │   └── InterlockEventRepository.cs
│   ├── DeviceAdapters/
│   │   ├── MqttCommandAdapter.cs               # HydroCore/RoomHub MQTT
│   │   └── DeviceCommandQueue.cs               # Outbox pattern
│   ├── Queues/
│   │   └── RunExecutionQueue.cs                # Run execution work queue
│   └── BackgroundWorkers/
│       ├── ScheduleTriggerWorker.cs            # Evaluate schedules
│       ├── RunExecutionWorker.cs               # Execute runs step-by-step
│       ├── DeviceCommandWorker.cs              # Process command queue
│       └── InterlockMonitorWorker.cs           # Monitor telemetry for interlocks
└── API/
    └── Controllers/
        ├── IrrigationGroupsController.cs       # CRUD groups
        ├── IrrigationProgramsController.cs     # CRUD programs
        ├── IrrigationSchedulesController.cs    # CRUD schedules
        └── IrrigationRunsController.cs         # Start/abort/status
```

---

## Database Schema

### Core Tables

#### irrigation_groups
```sql
CREATE TABLE irrigation_groups (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    code VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    max_concurrent_valves INT NOT NULL DEFAULT 6,
    pump_equipment_id UUID REFERENCES equipment(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(site_id, code)
);

CREATE INDEX idx_irrigation_groups_site ON irrigation_groups(site_id);
```

#### irrigation_group_zones
```sql
CREATE TABLE irrigation_group_zones (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    group_id UUID NOT NULL REFERENCES irrigation_groups(id) ON DELETE CASCADE,
    zone_id UUID NOT NULL REFERENCES zones(id),
    priority INT NOT NULL DEFAULT 1,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(group_id, zone_id)
);

CREATE INDEX idx_group_zones_group ON irrigation_group_zones(group_id);
```

#### irrigation_programs
```sql
CREATE TABLE irrigation_programs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    group_id UUID NOT NULL REFERENCES irrigation_groups(id),
    code VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    max_runtime_seconds INT NOT NULL DEFAULT 3600,
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(site_id, code)
);

CREATE INDEX idx_programs_site ON irrigation_programs(site_id);
CREATE INDEX idx_programs_group ON irrigation_programs(group_id);
```

#### irrigation_steps
```sql
CREATE TABLE irrigation_steps (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    program_id UUID NOT NULL REFERENCES irrigation_programs(id) ON DELETE CASCADE,
    step_order INT NOT NULL,
    step_type VARCHAR(50) NOT NULL, -- Shot/CycleSoak/Flush
    duration_seconds INT,           -- For time-based shots
    target_volume_liters DECIMAL(10, 2), -- For volume-based shots
    cycle_count INT,                -- For cycle/soak
    soak_duration_seconds INT,      -- For cycle/soak
    zone_ids UUID[],                -- Specific zones (or NULL for all)
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(program_id, step_order)
);

CREATE INDEX idx_steps_program ON irrigation_steps(program_id);
```

#### irrigation_schedules
```sql
CREATE TABLE irrigation_schedules (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    program_id UUID NOT NULL REFERENCES irrigation_programs(id),
    name VARCHAR(255) NOT NULL,
    trigger_type VARCHAR(50) NOT NULL, -- Time/Sensor/Hybrid
    cron_expression VARCHAR(100),      -- For time-based
    sensor_stream_id UUID REFERENCES sensor_streams(id), -- For sensor-triggered
    sensor_threshold DECIMAL(10, 2),
    enabled BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_schedules_site ON irrigation_schedules(site_id);
CREATE INDEX idx_schedules_program ON irrigation_schedules(program_id);
CREATE INDEX idx_schedules_enabled ON irrigation_schedules(enabled) WHERE enabled = true;
```

#### irrigation_runs
```sql
CREATE TABLE irrigation_runs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    program_id UUID NOT NULL REFERENCES irrigation_programs(id),
    schedule_id UUID REFERENCES irrigation_schedules(id), -- NULL if manual
    status VARCHAR(50) NOT NULL, -- Queued/Running/Completed/Aborted/Faulted
    started_by UUID REFERENCES users(id),
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    abort_reason TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_runs_site ON irrigation_runs(site_id);
CREATE INDEX idx_runs_program ON irrigation_runs(program_id);
CREATE INDEX idx_runs_status ON irrigation_runs(status);
CREATE INDEX idx_runs_started_at ON irrigation_runs(started_at);
```

#### irrigation_step_runs
```sql
CREATE TABLE irrigation_step_runs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    run_id UUID NOT NULL REFERENCES irrigation_runs(id) ON DELETE CASCADE,
    step_id UUID NOT NULL REFERENCES irrigation_steps(id),
    step_order INT NOT NULL,
    status VARCHAR(50) NOT NULL, -- Pending/Running/Completed/Aborted
    started_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    actual_duration_seconds INT,
    actual_volume_liters DECIMAL(10, 2),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_step_runs_run ON irrigation_step_runs(run_id);
```

#### interlock_events
```sql
CREATE TABLE interlock_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    run_id UUID REFERENCES irrigation_runs(id),
    interlock_type VARCHAR(50) NOT NULL, -- EStop/Door/TankLow/EcPh/CO2/Runtime/Concurrency
    reason TEXT NOT NULL,
    device_id UUID REFERENCES equipment(id),
    telemetry_value JSONB,
    fault_latched BOOLEAN NOT NULL DEFAULT false,
    rearmed_at TIMESTAMPTZ,
    rearmed_by UUID REFERENCES users(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_interlock_events_site ON interlock_events(site_id);
CREATE INDEX idx_interlock_events_run ON interlock_events(run_id);
CREATE INDEX idx_interlock_events_type ON interlock_events(interlock_type);
CREATE INDEX idx_interlock_events_fault_latched ON interlock_events(fault_latched) WHERE fault_latched = true;
```

#### device_commands
```sql
CREATE TABLE device_commands (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    run_id UUID REFERENCES irrigation_runs(id),
    step_id UUID REFERENCES irrigation_steps(id),
    sequence_number INT NOT NULL,
    command_scope VARCHAR(20) NOT NULL, -- Device/Zone/Broadcast
    target_device_id UUID REFERENCES equipment(id),
    target_zone_ids UUID[],
    command_type VARCHAR(50) NOT NULL, -- OpenValve/CloseValve/StartPump/StopPump/CloseAllValves
    command_payload JSONB NOT NULL,
    priority VARCHAR(20) NOT NULL DEFAULT 'Normal', -- Normal/Emergency
    status VARCHAR(50) NOT NULL, -- Pending/Sent/Acked/Failed
    sent_at TIMESTAMPTZ,
    acked_at TIMESTAMPTZ,
    error_message TEXT,
    retry_count INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CHECK (
        (command_scope = 'Device' AND target_device_id IS NOT NULL)
        OR (command_scope = 'Zone' AND array_length(target_zone_ids, 1) IS NOT NULL)
        OR (command_scope = 'Broadcast')
    )
);

CREATE UNIQUE INDEX idx_device_commands_run_sequence ON device_commands(run_id, sequence_number);
CREATE INDEX idx_device_commands_site ON device_commands(site_id);
CREATE INDEX idx_device_commands_run ON device_commands(run_id);
CREATE INDEX idx_device_commands_step ON device_commands(step_id);
CREATE INDEX idx_device_commands_scope ON device_commands(command_scope);
CREATE INDEX idx_device_commands_status ON device_commands(status);
CREATE INDEX idx_device_commands_pending ON device_commands(status, created_at) WHERE status = 'Pending';
```

`command_scope` and the optional `target_*` columns let us represent single-device, zone fan-out, and broadcast commands without violating NOT NULL constraints. `sequence_number` maps acknowledgments back to the original program step, while `priority` is used by the queue worker to fast-track emergency actions such as `CloseAllValves`.

#### mix_tanks
```sql
CREATE TABLE mix_tanks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    code VARCHAR(50) NOT NULL,
    name VARCHAR(255) NOT NULL,
    capacity_liters DECIMAL(10, 2) NOT NULL,
    level_sensor_stream_id UUID REFERENCES sensor_streams(id),
    ec_sensor_stream_id UUID REFERENCES sensor_streams(id),
    ph_sensor_stream_id UUID REFERENCES sensor_streams(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(site_id, code)
);

CREATE INDEX idx_mix_tanks_site ON mix_tanks(site_id);
```

#### injector_channels
```sql
CREATE TABLE injector_channels (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id UUID NOT NULL REFERENCES sites(id),
    mix_tank_id UUID NOT NULL REFERENCES mix_tanks(id),
    channel_number INT NOT NULL,
    nutrient_product_id UUID REFERENCES inventory_lots(id),
    ml_per_second DECIMAL(6, 2) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(mix_tank_id, channel_number)
);

CREATE INDEX idx_injector_channels_tank ON injector_channels(mix_tank_id);
```

### RLS Policies

All tables have site-scoped RLS policies:

```sql
-- irrigation_groups
CREATE POLICY irrigation_groups_site_isolation ON irrigation_groups
    USING (site_id = current_setting('app.current_site_id', true)::UUID);

-- irrigation_programs
CREATE POLICY irrigation_programs_site_isolation ON irrigation_programs
    USING (site_id = current_setting('app.current_site_id', true)::UUID);

-- irrigation_runs
CREATE POLICY irrigation_runs_site_isolation ON irrigation_runs
    USING (site_id = current_setting('app.current_site_id', true)::UUID);

-- interlock_events
CREATE POLICY interlock_events_site_isolation ON interlock_events
    USING (site_id = current_setting('app.current_site_id', true)::UUID);

-- device_commands
CREATE POLICY device_commands_site_isolation ON device_commands
    USING (site_id = current_setting('app.current_site_id', true)::UUID);

-- mix_tanks
CREATE POLICY mix_tanks_site_isolation ON mix_tanks
    USING (site_id = current_setting('app.current_site_id', true)::UUID);

-- injector_channels
CREATE POLICY injector_channels_site_isolation ON injector_channels
    USING (site_id = current_setting('app.current_site_id', true)::UUID);

-- Service account bypass
ALTER TABLE irrigation_groups ENABLE ROW LEVEL SECURITY;
-- ... repeat for all tables
```

---

## Domain Model

### Irrigation Group Aggregate

```csharp
public class IrrigationGroup : Entity<Guid>
{
    public Guid SiteId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public int MaxConcurrentValves { get; private set; }
    public Guid? PumpEquipmentId { get; private set; }
    
    private readonly List<IrrigationGroupZone> _zones = new();
    public IReadOnlyCollection<IrrigationGroupZone> Zones => _zones.AsReadOnly();
    
    private IrrigationGroup() { } // EF Core
    
    public static IrrigationGroup Create(
        Guid siteId,
        string code,
        string name,
        int maxConcurrentValves = 6,
        Guid? pumpEquipmentId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (maxConcurrentValves < 1 || maxConcurrentValves > 24)
            throw new ArgumentException("Max concurrent valves must be 1-24", nameof(maxConcurrentValves));
        
        return new IrrigationGroup
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            Code = code,
            Name = name,
            MaxConcurrentValves = maxConcurrentValves,
            PumpEquipmentId = pumpEquipmentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public void AddZone(Guid zoneId, int priority = 1)
    {
        if (_zones.Any(z => z.ZoneId == zoneId))
            throw new InvalidOperationException($"Zone {zoneId} already in group");
        
        _zones.Add(new IrrigationGroupZone
        {
            Id = Guid.NewGuid(),
            GroupId = Id,
            ZoneId = zoneId,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        });
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveZone(Guid zoneId)
    {
        var zone = _zones.FirstOrDefault(z => z.ZoneId == zoneId);
        if (zone != null)
        {
            _zones.Remove(zone);
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    public void UpdateMaxConcurrentValves(int maxConcurrentValves)
    {
        if (maxConcurrentValves < 1 || maxConcurrentValves > 24)
            throw new ArgumentException("Max concurrent valves must be 1-24");
        
        MaxConcurrentValves = maxConcurrentValves;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class IrrigationGroupZone : Entity<Guid>
{
    public Guid GroupId { get; set; }
    public Guid ZoneId { get; set; }
    public int Priority { get; set; }
}
```

### Irrigation Program Aggregate

```csharp
public class IrrigationProgram : Entity<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid GroupId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int MaxRuntimeSeconds { get; private set; }
    public Guid CreatedBy { get; private set; }
    
    private readonly List<IrrigationStep> _steps = new();
    public IReadOnlyCollection<IrrigationStep> Steps => _steps.AsReadOnly();
    
    private IrrigationProgram() { } // EF Core
    
    public static IrrigationProgram Create(
        Guid siteId,
        Guid groupId,
        string code,
        string name,
        string description,
        int maxRuntimeSeconds,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (maxRuntimeSeconds < 60 || maxRuntimeSeconds > 7200)
            throw new ArgumentException("Max runtime must be 60-7200 seconds", nameof(maxRuntimeSeconds));
        
        return new IrrigationProgram
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            GroupId = groupId,
            Code = code,
            Name = name,
            Description = description,
            MaxRuntimeSeconds = maxRuntimeSeconds,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public void AddStep(
        int stepOrder,
        StepType stepType,
        int? durationSeconds = null,
        decimal? targetVolumeLiters = null,
        int? cycleCount = null,
        int? soakDurationSeconds = null,
        Guid[] zoneIds = null)
    {
        if (_steps.Any(s => s.StepOrder == stepOrder))
            throw new InvalidOperationException($"Step order {stepOrder} already exists");
        
        var step = IrrigationStep.Create(
            Id,
            stepOrder,
            stepType,
            durationSeconds,
            targetVolumeLiters,
            cycleCount,
            soakDurationSeconds,
            zoneIds);
        
        _steps.Add(step);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveStep(int stepOrder)
    {
        var step = _steps.FirstOrDefault(s => s.StepOrder == stepOrder);
        if (step != null)
        {
            _steps.Remove(step);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

public class IrrigationStep : Entity<Guid>
{
    public Guid ProgramId { get; private set; }
    public int StepOrder { get; private set; }
    public StepType StepType { get; private set; }
    public int? DurationSeconds { get; private set; }
    public decimal? TargetVolumeLiters { get; private set; }
    public int? CycleCount { get; private set; }
    public int? SoakDurationSeconds { get; private set; }
    public Guid[] ZoneIds { get; private set; }
    
    private IrrigationStep() { } // EF Core
    
    public static IrrigationStep Create(
        Guid programId,
        int stepOrder,
        StepType stepType,
        int? durationSeconds,
        decimal? targetVolumeLiters,
        int? cycleCount,
        int? soakDurationSeconds,
        Guid[] zoneIds)
    {
        // Validation based on step type
        switch (stepType)
        {
            case StepType.Shot:
                if (durationSeconds == null && targetVolumeLiters == null)
                    throw new ArgumentException("Shot step requires duration or volume");
                break;
            case StepType.CycleSoak:
                if (cycleCount == null || durationSeconds == null || soakDurationSeconds == null)
                    throw new ArgumentException("CycleSoak requires cycle count, duration, and soak duration");
                break;
            case StepType.Flush:
                if (durationSeconds == null)
                    throw new ArgumentException("Flush step requires duration");
                break;
        }
        
        return new IrrigationStep
        {
            Id = Guid.NewGuid(),
            ProgramId = programId,
            StepOrder = stepOrder,
            StepType = stepType,
            DurationSeconds = durationSeconds,
            TargetVolumeLiters = targetVolumeLiters,
            CycleCount = cycleCount,
            SoakDurationSeconds = soakDurationSeconds,
            ZoneIds = zoneIds ?? Array.Empty<Guid>(),
            CreatedAt = DateTime.UtcNow
        };
    }
}

public enum StepType
{
    Shot,
    CycleSoak,
    Flush
}
```

### Irrigation Run Aggregate

```csharp
public class IrrigationRun : Entity<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid ProgramId { get; private set; }
    public Guid? ScheduleId { get; private set; }
    public RunStatus Status { get; private set; }
    public Guid? StartedBy { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string AbortReason { get; private set; }
    
    private readonly List<IrrigationStepRun> _stepRuns = new();
    public IReadOnlyCollection<IrrigationStepRun> StepRuns => _stepRuns.AsReadOnly();
    
    private IrrigationRun() { } // EF Core
    
    public static IrrigationRun Create(
        Guid siteId,
        Guid programId,
        Guid? scheduleId,
        Guid? startedBy)
    {
        return new IrrigationRun
        {
            Id = Guid.NewGuid(),
            SiteId = siteId,
            ProgramId = programId,
            ScheduleId = scheduleId,
            Status = RunStatus.Queued,
            StartedBy = startedBy,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public void Start()
    {
        if (Status != RunStatus.Queued)
            throw new InvalidOperationException($"Cannot start run in status {Status}");
        
        Status = RunStatus.Running;
        StartedAt = DateTime.UtcNow;
    }
    
    public void Complete()
    {
        if (Status != RunStatus.Running)
            throw new InvalidOperationException($"Cannot complete run in status {Status}");
        
        Status = RunStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
    
    public void Abort(string reason)
    {
        if (Status == RunStatus.Completed || Status == RunStatus.Aborted || Status == RunStatus.Faulted)
            throw new InvalidOperationException($"Cannot abort run in status {Status}");
        
        Status = RunStatus.Aborted;
        AbortReason = reason;
        CompletedAt = DateTime.UtcNow;
    }
    
    public void Fault(string reason)
    {
        Status = RunStatus.Faulted;
        AbortReason = reason;
        CompletedAt = DateTime.UtcNow;
    }
    
    public void AddStepRun(IrrigationStepRun stepRun)
    {
        _stepRuns.Add(stepRun);
    }
}

public enum RunStatus
{
    Queued,
    Running,
    Completed,
    Aborted,
    Faulted
}
```

### Device Command Entity

```csharp
public class DeviceCommand : Entity<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid? RunId { get; private set; }
    public Guid? StepId { get; private set; }
    public int SequenceNumber { get; private set; }
    public CommandScope Scope { get; private set; }
    public Guid? TargetDeviceId { get; private set; }
    public Guid[] TargetZoneIds { get; private set; }
    public CommandType CommandType { get; private set; }
    public CommandPriority Priority { get; private set; }
    public CommandStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? AckedAt { get; private set; }
    public string CommandPayload { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static DeviceCommand CreateDevice(Guid siteId, Guid runId, Guid stepId, Guid deviceId, int sequenceNumber, CommandType commandType, CommandPriority priority, string payload)
        => new DeviceCommand(siteId, runId, stepId, sequenceNumber, commandType, priority, payload)
        {
            Scope = CommandScope.Device,
            TargetDeviceId = deviceId
        };

    public static DeviceCommand CreateZone(Guid siteId, Guid runId, Guid stepId, Guid[] zoneIds, int sequenceNumber, CommandType commandType, CommandPriority priority, string payload)
        => new DeviceCommand(siteId, runId, stepId, sequenceNumber, commandType, priority, payload)
        {
            Scope = CommandScope.Zone,
            TargetZoneIds = zoneIds ?? Array.Empty<Guid>()
        };

    public static DeviceCommand CreateBroadcast(Guid siteId, Guid runId, Guid? stepId, int sequenceNumber, CommandType commandType, CommandPriority priority, string payload)
        => new DeviceCommand(siteId, runId, stepId, sequenceNumber, commandType, priority, payload)
        {
            Scope = CommandScope.Broadcast
        };

    public void MarkSent(DateTime sentAt)
    {
        Status = CommandStatus.Sent;
        SentAt = sentAt;
    }

    public void MarkAcked(DateTime ackedAt)
    {
        Status = CommandStatus.Acked;
        AckedAt = ackedAt;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = CommandStatus.Failed;
        RetryCount++;
        ErrorMessage = errorMessage;
    }

    private DeviceCommand(Guid siteId, Guid? runId, Guid? stepId, int sequenceNumber, CommandType commandType, CommandPriority priority, string payload)
    {
        SiteId = siteId;
        RunId = runId;
        StepId = stepId;
        SequenceNumber = sequenceNumber;
        CommandType = commandType;
        Priority = priority;
        Status = CommandStatus.Pending;
        TargetZoneIds = Array.Empty<Guid>();
        CommandPayload = payload;
        CreatedAt = DateTime.UtcNow;
    }

    private DeviceCommand() { } // EF Core
}
```

`CommandPayload` stores the canonical JSON envelope we publish to MQTT so acknowledgments can be correlated deterministically between the broker and the outbox.

---

## Application Services

### Irrigation Orchestrator Service

```csharp
public interface IIrrigationOrchestratorService
{
    Task<Guid> StartRunAsync(Guid programId, Guid? scheduleId, Guid? startedBy, CancellationToken cancellationToken = default);
    Task AbortRunAsync(Guid runId, string reason, CancellationToken cancellationToken = default);
    Task<IrrigationRunResponse> GetRunStatusAsync(Guid runId, CancellationToken cancellationToken = default);
}

public class IrrigationOrchestratorService : IIrrigationOrchestratorService
{
    private readonly IIrrigationRunRepository _runRepository;
    private readonly IIrrigationProgramRepository _programRepository;
    private readonly IInterlockEvaluationService _interlockService;
    private readonly IRunExecutionService _runExecutionService;
    private readonly IIrrigationAbortSaga _abortSaga;
    private readonly ILogger<IrrigationOrchestratorService> _logger;
    
    public async Task<Guid> StartRunAsync(
        Guid programId,
        Guid? scheduleId,
        Guid? startedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting irrigation run for program {ProgramId}", programId);
        
        var program = await _programRepository.GetByIdAsync(programId, cancellationToken);
        if (program == null)
            throw new NotFoundException($"Program {programId} not found");
        
        var interlockResult = await _interlockService.EvaluateAsync(program.SiteId, program.GroupId, cancellationToken);
        if (!interlockResult.Allowed)
        {
            _logger.LogWarning("Run rejected due to interlock: {Reason}", interlockResult.DenyReason);
            throw new InterlockViolationException(interlockResult.DenyReason);
        }
        
        var run = IrrigationRun.Create(program.SiteId, programId, scheduleId, startedBy);
        await _runRepository.AddAsync(run, cancellationToken);
        
        var executionPlan = _runExecutionService.BuildPlan(program);

        run.Start();
        await _runRepository.UpdateAsync(run, cancellationToken);

        await _runExecutionService.EnqueueAsync(run.Id, executionPlan, cancellationToken);
        
        _logger.LogInformation("Irrigation run {RunId} started", run.Id);
        return run.Id;
    }
    
    public async Task AbortRunAsync(Guid runId, string reason, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Aborting irrigation run {RunId}: {Reason}", runId, reason);
        
        var run = await _runRepository.GetByIdAsync(runId, cancellationToken);
        if (run == null)
            throw new NotFoundException($"Run {runId} not found");
        
        var abortOutcome = await _abortSaga.ExecuteAsync(run, reason, cancellationToken);

        if (abortOutcome.Faulted)
        {
            run.Fault(abortOutcome.FailureReason);
        }
        else
        {
            run.Abort(reason);
        }

        await _runRepository.UpdateAsync(run, cancellationToken);
        
        _logger.LogInformation("Irrigation run {RunId} aborted", runId);
    }
}
```

### Run Execution Service

```csharp
public interface IRunExecutionService
{
    ExecutionPlan BuildPlan(IrrigationProgram program);
    Task EnqueueAsync(Guid runId, ExecutionPlan plan, CancellationToken cancellationToken = default);
}

public record ExecutionPlanStep(
    Guid StepId,
    int SequenceNumber,
    TimeSpan? TargetDuration,
    decimal? TargetVolume,
    Guid[] ZoneIds);
public record ExecutionPlan(Guid ProgramId, ExecutionPlanStep[] Steps);

public class RunExecutionService : IRunExecutionService
{
    private readonly IRunExecutionQueue _runExecutionQueue;

    public RunExecutionService(IRunExecutionQueue runExecutionQueue)
    {
        _runExecutionQueue = runExecutionQueue;
    }

    public ExecutionPlan BuildPlan(IrrigationProgram program)
    {
        var sequence = 0;
        var steps = program.Steps
            .OrderBy(s => s.StepOrder)
            .Select(step => new ExecutionPlanStep(
                step.Id,
                ++sequence,
                step.DurationSeconds.HasValue ? TimeSpan.FromSeconds(step.DurationSeconds.Value) : null,
                step.TargetVolumeLiters,
                step.ZoneIds))
            .ToArray();

        return new ExecutionPlan(program.Id, steps);
    }

    public Task EnqueueAsync(Guid runId, ExecutionPlan plan, CancellationToken cancellationToken = default)
        => _runExecutionQueue.EnqueueAsync(new RunExecutionRequest(runId, plan), cancellationToken);
}
```

The `RunExecutionWorker` consumes queued `RunExecutionRequest` items and coordinates with the `DeviceCommandWorker` to:
- Dispatch pump enable and valve open commands with `command_scope` set appropriately.
- Await acknowledgments for each `sequence_number` before moving to the next command.
- Schedule valve close commands or volume cutoffs when `TargetDuration`/`TargetVolume` thresholds are met.
- Auto-issue pump start/stop brackets and final zone flush closeouts.
- Emit `IrrigationStepRun` updates and fault the run if retries are exhausted.

### Interlock Evaluation Service

```csharp
public interface IInterlockEvaluationService
{
    Task<InterlockResult> EvaluateAsync(Guid siteId, Guid groupId, CancellationToken cancellationToken = default);
}

public class InterlockEvaluationService : IInterlockEvaluationService
{
    private readonly ITelemetryQueryService _telemetryService;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly ILogger<InterlockEvaluationService> _logger;
    
    public async Task<InterlockResult> EvaluateAsync(
        Guid siteId,
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var results = new List<InterlockCheckResult>();
        
        // Check E-STOP
        results.Add(await CheckEStopAsync(siteId, cancellationToken));
        
        // Check door interlock
        results.Add(await CheckDoorAsync(siteId, cancellationToken));
        
        // Check tank level
        results.Add(await CheckTankLevelAsync(siteId, cancellationToken));
        
        // Check EC/pH bounds
        results.Add(await CheckEcPhBoundsAsync(siteId, cancellationToken));
        
        // Check CO₂ lockout
        results.Add(await CheckCo2LockoutAsync(siteId, cancellationToken));
        
        // Check concurrency cap
        results.Add(await CheckConcurrencyCapAsync(groupId, cancellationToken));
        
        // Aggregate results
        var failed = results.FirstOrDefault(r => !r.Passed);
        if (failed != null)
        {
            return InterlockResult.Deny(failed.InterlockType, failed.Reason);
        }
        
        return InterlockResult.Allow();
    }
    
    private async Task<InterlockCheckResult> CheckEStopAsync(Guid siteId, CancellationToken cancellationToken)
    {
        // Query latest E-STOP telemetry
        var estopStream = await _telemetryService.GetStreamByCodeAsync(siteId, "ESTOP_STATUS", cancellationToken);
        if (estopStream == null)
        {
            _logger.LogWarning("E-STOP stream not found for site {SiteId}", siteId);
            return InterlockCheckResult.Deny(InterlockType.EStop, "E-STOP status unknown");
        }
        
        var latestReading = await EnsureFreshTelemetryAsync(estopStream.Id, TimeSpan.FromSeconds(30), cancellationToken);
        if (latestReading == null)
        {
            _logger.LogWarning("E-STOP reading stale for site {SiteId}", siteId);
            return InterlockCheckResult.Deny(InterlockType.EStop, "E-STOP telemetry stale (> 30s)");
        }
        
        if (latestReading.Value == 0) // 0 = OPEN (faulted)
        {
            return InterlockCheckResult.Deny(InterlockType.EStop, "E-STOP is OPEN");
        }
        
        return InterlockCheckResult.Pass(InterlockType.EStop);
    }
    
    private async Task<InterlockCheckResult> CheckTankLevelAsync(Guid siteId, CancellationToken cancellationToken)
    {
        // Query tank level sensor
        var tankStream = await _telemetryService.GetStreamByCodeAsync(siteId, "TANK_LEVEL", cancellationToken);
        if (tankStream == null)
            return InterlockCheckResult.Pass(InterlockType.TankLow); // No tank level sensor = skip check
        
        var latestReading = await EnsureFreshTelemetryAsync(tankStream.Id, TimeSpan.FromMinutes(2), cancellationToken);
        if (latestReading == null)
            return InterlockCheckResult.Deny(InterlockType.TankLow, "Tank level unknown or stale (> 2 min)");
        
        const decimal MIN_LEVEL_LITERS = 50.0m;
        if (latestReading.Value < MIN_LEVEL_LITERS)
        {
            return InterlockCheckResult.Deny(InterlockType.TankLow, $"Tank level {latestReading.Value:F1}L < {MIN_LEVEL_LITERS}L");
        }
        
        return InterlockCheckResult.Pass(InterlockType.TankLow);
    }
    
    // Additional interlock checks use EnsureFreshTelemetryAsync with check-specific thresholds

    private async Task<TelemetryReading?> EnsureFreshTelemetryAsync(Guid streamId, TimeSpan maxStaleness, CancellationToken cancellationToken)
    {
        var reading = await _telemetryService.GetLatestReadingAsync(streamId, cancellationToken);
        if (reading == null)
        {
            return null;
        }

        if (reading.Timestamp < DateTime.UtcNow.Subtract(maxStaleness))
        {
            return null;
        }

        return reading;
    }
}

public class InterlockResult
{
    public bool Allowed { get; private set; }
    public InterlockType? InterlockType { get; private set; }
    public string DenyReason { get; private set; }
    
    public static InterlockResult Allow() => new InterlockResult { Allowed = true };
    
    public static InterlockResult Deny(InterlockType interlockType, string reason)
        => new InterlockResult
        {
            Allowed = false,
            InterlockType = interlockType,
            DenyReason = reason
        };
}

public enum InterlockType
{
    EStop,
    Door,
    TankLow,
    EcPh,
    CO2,
    MaxRuntime,
    Concurrency
}
```

---

## API Contracts

### Create Irrigation Group

```http
POST /api/v1/irrigation/groups
Content-Type: application/json
Authorization: Bearer {token}

{
  "siteId": "uuid",
  "code": "GRP-001",
  "name": "Veg Room Group 1",
  "maxConcurrentValves": 6,
  "pumpEquipmentId": "uuid", // optional
  "zoneIds": ["uuid", "uuid", "uuid"]
}

Response 201 Created:
{
  "id": "uuid",
  "siteId": "uuid",
  "code": "GRP-001",
  "name": "Veg Room Group 1",
  "maxConcurrentValves": 6,
  "pumpEquipmentId": "uuid",
  "zones": [
    { "zoneId": "uuid", "zoneName": "VEG-A", "priority": 1 },
    { "zoneId": "uuid", "zoneName": "VEG-B", "priority": 1 },
    { "zoneId": "uuid", "zoneName": "VEG-C", "priority": 1 }
  ],
  "createdAt": "2025-10-07T10:00:00Z"
}
```

### Create Irrigation Program

```http
POST /api/v1/irrigation/programs
Content-Type: application/json
Authorization: Bearer {token}

{
  "siteId": "uuid",
  "groupId": "uuid",
  "code": "PROG-001",
  "name": "Veg Daily Watering",
  "description": "Standard veg room irrigation: 3-zone sequence, 90s per zone",
  "maxRuntimeSeconds": 600,
  "steps": [
    {
      "stepOrder": 1,
      "stepType": "Shot",
      "durationSeconds": 90,
      "zoneIds": ["uuid"] // Zone VEG-A
    },
    {
      "stepOrder": 2,
      "stepType": "Shot",
      "durationSeconds": 90,
      "zoneIds": ["uuid"] // Zone VEG-B
    },
    {
      "stepOrder": 3,
      "stepType": "Shot",
      "durationSeconds": 90,
      "zoneIds": ["uuid"] // Zone VEG-C
    }
  ]
}

Response 201 Created:
{
  "id": "uuid",
  "siteId": "uuid",
  "groupId": "uuid",
  "code": "PROG-001",
  "name": "Veg Daily Watering",
  "steps": [
    { "stepOrder": 1, "stepType": "Shot", "durationSeconds": 90, "zoneIds": ["uuid"] },
    { "stepOrder": 2, "stepType": "Shot", "durationSeconds": 90, "zoneIds": ["uuid"] },
    { "stepOrder": 3, "stepType": "Shot", "durationSeconds": 90, "zoneIds": ["uuid"] }
  ],
  "createdAt": "2025-10-07T10:05:00Z"
}
```

### Start Irrigation Run

```http
POST /api/v1/irrigation/runs
Content-Type: application/json
Authorization: Bearer {token}

{
  "programId": "uuid",
  "scheduleId": null, // null = manual start
  "startedBy": "uuid"
}

Response 201 Created:
{
  "id": "uuid",
  "programId": "uuid",
  "status": "Running",
  "startedBy": "uuid",
  "startedAt": "2025-10-07T10:10:00Z",
  "estimatedCompletionAt": "2025-10-07T10:14:30Z"
}
```

### Abort Irrigation Run

```http
POST /api/v1/irrigation/runs/{runId}/abort
Content-Type: application/json
Authorization: Bearer {token}

{
  "reason": "Manual operator abort - checked wrong zone"
}

Response 200 OK:
{
  "id": "uuid",
  "status": "Aborted",
  "abortReason": "Manual operator abort - checked wrong zone",
  "completedAt": "2025-10-07T10:11:23Z"
}
```

---

## Acceptance Criteria

| Criteria | Evidence |
|----------|----------|
| ✅ Program executes with step monitoring | E2E test: 3-step program completes, logs show each step start/end |
| ✅ Safe aborts close valves | Integration test: abort command → valves close < 5s |
| ✅ HIL report green (zero unsafe actuations) | HIL chaos drill report with firmware sign-off |
| ✅ Audit trail complete | Database audit: all runs, steps, interlocks, commands logged |
| ✅ Enqueue→ack p95 < 800ms | k6 load test: command queue → device ack within 800ms (p95) |

---

## Testing Strategy

### Unit Tests (Est: 6 hours)
- Domain entity creation/validation
- Interlock specifications (20+ scenarios)
- Step type validation (Shot/CycleSoak/Flush)
- Run state machine (Queued → Running → Completed/Aborted/Faulted)

### Integration Tests (Est: 8 hours)
- Orchestrator saga (run → abort → compensate)
- Device command queue (enqueue → process → ack)
- Interlock evaluation (telemetry → deny/allow)
- RLS policies (cross-site access blocked)

### HIL Tests (Est: 80 hours - hardware build + execution)
- Full chaos matrix on golden harness (12 tests)
- E-STOP/door hard-off verification
- Power failover (PoE/AC) spurious actuation check
- Broker loss offline-first behavior
- Concurrency cap thermal validation

### E2E Tests (Est: 4 hours)
- End-to-end irrigation flow (schedule → run → complete)
- Manual start with ABAC approval
- Interlock trip during run → safe abort

---

## Timeline & Estimates

| Phase | Tasks | Estimated Hours |
|-------|-------|-----------------|
| **Pre-Slice Setup** | Schema design, DI config, DTOs | 2-3 hours |
| **Slice 1: Core Orchestration** | Groups, Programs, basic runs | 8-10 hours |
| **Slice 2: Interlocks** | Evaluation service, telemetry integration | 6-8 hours |
| **Slice 3: Device Commands** | MQTT adapter, command queue, ack tracking | 4-6 hours |
| **Slice 4: Abort Saga** | Safe abort compensator, fault handling | 2-3 hours |
| **Testing & Polish** | Unit/integration tests, validators | 4-6 hours |
| **HIL Validation** | Golden harness build + chaos drills | 80 hours (Hardware) |
| **TOTAL** | | **24-28 hours dev + 80 hours hardware** |

---

## Dependencies & Blockers

### Prerequisites
- ✅ FRP-01 (Identity/RLS) - Complete
- ✅ FRP-02 (Spatial/Equipment) - Complete
- ⚠️ FRP-05 (Telemetry) - 93% complete, load testing outstanding

### Blockers
1. **FRP-05 Load Testing** - Must complete before FRP-06 start
   - Mitigation: Prioritize FRP-05 performance validation
2. **Golden Harness Build** - W0-W5 target
   - Mitigation: Start immediately, parallel track with dev work
3. **Device Firmware RC** - Required for HIL drills
   - Mitigation: Coordinate with Hardware/Firmware squad

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| **HIL drills fail** | Irrigation cannot enable, pilot blocked | Comprehensive interlock specs, firmware sign-off gate, re-test after fix |
| **FRP-05 telemetry lag** | Interlock decisions stale | Staleness check (< 5 min), fallback to safe deny |
| **Device command ack timeout** | Run stuck in "Running" state | Timeout + retry logic, manual intervention UI |
| **Concurrency cap thermal** | Transformer overheats | Hardware current limiting, firmware cap enforcement |

---

## Reuse from Track A

- ✅ MQTT ingestion adapters (FRP-05)
- ✅ Device command topics (existing)
- ✅ Interlock specification library (existing)
- ✅ Command queue infrastructure (existing)
- ✅ Observability dashboards baseline (existing)

---

## SLO Targets

| Metric | SLO | Alert Threshold |
|--------|-----|-----------------|
| Command enqueue→ack p95 | < 800ms | > 1.0s for 5m |
| Interlock evaluation p95 | < 200ms | > 300ms for 5m |
| Safe abort latency | < 5s | > 10s (critical alert) |
| HIL pass rate | 100% | Any failure = firmware sign-off denied |

---

## Related Documents

- [HIL Chaos Drill Playbook](./hardware/HIL_CHAOS_DRILL_PLAYBOOK.md)
- [FRP-05 Implementation Plan](./FRP05_IMPLEMENTATION_PLAN.md)
- [Track B Implementation Plan](./TRACK_B_IMPLEMENTATION_PLAN.md)
- [CanopyLogic PRD - Irrigation Section](./prd/CanopyLogic_PRD_Enterprise_Edition_v1_0_Sep_17_2025.md)

---

**Last Updated:** October 7, 2025  
**Status:** ✅ Planning Complete, Ready for Development  
**Next Review:** After FRP-05 load testing complete
