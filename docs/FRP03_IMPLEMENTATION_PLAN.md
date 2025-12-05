# FRP-03: Genetics, Strains & Batches - Implementation Plan

**Status:** ✅ Completed (Historical reference)  
**Actual Effort:** Within planned 18-22 hours window  
**Prerequisites:** ✅ FRP-01 Complete (Identity, RLS, ABAC), ✅ FRP-02 Complete (Spatial, Equipment)  
**Blocks Cleared:** Dependencies for FRP-07/08/09 are now unblocked by FRP-03 delivery

> **Note:** This document preserves the original task breakdown. All checklist items have been delivered; refer to `FRP03_FINAL_STATUS_UPDATE.md` for the final outcome.

---

## 📋 OVERVIEW

### Purpose
Establish a comprehensive genetics and strain management system with batch lifecycle tracking, mother plant health monitoring, and lineage traceability. This foundation enables seed-to-sale tracking, compliance reporting, and quality control throughout the cultivation process.

### Key Features
1. **Genetics Management** - Strain definitions, phenotypes, genetic profiles
2. **Batch Lifecycle** - From seed/clone to harvest with state tracking
3. **Mother Plant Registry** - Health logs, propagation tracking, genetic source
4. **Lineage Tracking** - Complete parent-child relationships for compliance
5. **Batch Relationships** - Splits, merges, and transformations
6. **Event Logging** - All batch state changes with audit trail

### Acceptance Criteria (from PRD)
- ✅ Batch lineage tracked correctly (parent-child relationships)
- ✅ Mother plant health logs retrievable and reportable
- ✅ Strain-specific blueprints associable to batches
- ✅ Configurable stage templates + transitions drive lifecycle
- ✅ Batch state machine enforces valid transitions
- ✅ RLS blocks cross-site access to genetics data

---

## 📊 IMPLEMENTATION BREAKDOWN

### Phase 1: Database Schema (3-4 hours)

#### Migration 1: Genetics & Strains
**File:** `src/database/migrations/frp03/20251002_01_CreateGeneticsTables.sql`

**Tables:**
```sql
-- Genetics (base genetic profiles)
genetics (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    name varchar(200) NOT NULL,
    description text,
    genetic_type varchar(50) CHECK (genetic_type IN ('indica', 'sativa', 'hybrid', 'autoflower', 'hemp')),
    thc_percentage_range decimal(5,2)[],
    cbd_percentage_range decimal(5,2)[],
    flowering_time_days int,
    yield_potential varchar(20) CHECK (yield_potential IN ('low', 'medium', 'high', 'very_high')),
    growth_characteristics jsonb,
    terpene_profile jsonb,
    breeding_notes text,
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id, name)
)

-- Phenotypes (specific expressions of genetics)
phenotypes (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    genetics_id uuid NOT NULL REFERENCES genetics(id) ON DELETE CASCADE,
    name varchar(200) NOT NULL,
    description text,
    expression_notes text,
    visual_characteristics jsonb,
    aroma_profile jsonb,
    growth_pattern jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id, genetics_id, name)
)

-- Strains (named combinations of genetics + phenotype)
strains (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    genetics_id uuid NOT NULL REFERENCES genetics(id) ON DELETE RESTRICT,
    phenotype_id uuid REFERENCES phenotypes(id) ON DELETE SET NULL,
    name varchar(200) NOT NULL,
    breeder varchar(200),
    seed_bank varchar(200),
    description text,
    cultivation_notes text,
    expected_harvest_window_days int,
    target_environment jsonb,
    compliance_requirements jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id, name)
)
```

#### Migration 2: Batches & Mother Plants
**File:** `src/database/migrations/frp03/20251002_02_CreateBatchTables.sql`

**Tables:**
```sql
-- Batch Stage Definitions (site-configurable lifecycle stages)
batch_stage_definitions (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    stage_key varchar(50) NOT NULL,
    display_name varchar(100) NOT NULL,
    description text,
    sequence_order int NOT NULL,
    is_terminal boolean DEFAULT FALSE,
    requires_harvest_metrics boolean DEFAULT FALSE,
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id, stage_key),
    UNIQUE(site_id, sequence_order)
)

-- Batch Stage Transitions (site-configurable stage flow)
batch_stage_transitions (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    from_stage_id uuid NOT NULL REFERENCES batch_stage_definitions(id) ON DELETE CASCADE,
    to_stage_id uuid NOT NULL REFERENCES batch_stage_definitions(id) ON DELETE CASCADE,
    auto_advance boolean DEFAULT FALSE,
    requires_approval boolean DEFAULT FALSE,
    approval_role varchar(100),
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id, from_stage_id, to_stage_id)
)
> **Commissioning Option:** Offer a setup/commissioning package to help customers configure stage templates and transitions during onboarding, ensuring local compliance without custom code.

-- Batches (groups of plants with shared genetics)
batches (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    strain_id uuid NOT NULL REFERENCES strains(id) ON DELETE RESTRICT,
    batch_code varchar(100) NOT NULL,
    batch_name varchar(200),
    batch_type varchar(50) CHECK (batch_type IN ('seed', 'clone', 'tissue_culture', 'mother_plant')),
    source_type varchar(50) CHECK (source_type IN ('purchase', 'propagation', 'breeding', 'tissue_culture')),
    parent_batch_id uuid REFERENCES batches(id),
    generation int DEFAULT 1,
    plant_count int NOT NULL,
    target_plant_count int,
    current_stage_id uuid NOT NULL REFERENCES batch_stage_definitions(id),
    stage_started_at timestamptz,
    expected_harvest_date date,
    actual_harvest_date date,
    location_id uuid REFERENCES inventory_locations(id),
    room_id uuid REFERENCES rooms(id),
    zone_id uuid REFERENCES zones(id),
    status varchar(20) CHECK (status IN ('active', 'quarantine', 'hold', 'destroyed', 'completed')),
    notes text,
    metadata jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id, batch_code)
)

-- Batch Stage History (audit trail for stage changes)
batch_stage_history (
    id uuid PRIMARY KEY,
    batch_id uuid NOT NULL REFERENCES batches(id) ON DELETE CASCADE,
    from_stage_id uuid REFERENCES batch_stage_definitions(id),
    to_stage_id uuid NOT NULL REFERENCES batch_stage_definitions(id),
    changed_by uuid NOT NULL REFERENCES users(id),
    changed_at timestamptz NOT NULL,
    notes text
)

-- Batch Code Rules (jurisdiction-compliant, user-defined)
batch_code_rules (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    name varchar(100) NOT NULL,
    rule_definition jsonb NOT NULL,
    reset_policy varchar(50) CHECK (reset_policy IN ('never', 'annual', 'monthly', 'per_harvest')),
    is_active boolean DEFAULT TRUE,
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id, name),
    UNIQUE(site_id) WHERE is_active = TRUE
)

-- Batch Events (state changes and activities)
batch_events (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    batch_id uuid NOT NULL REFERENCES batches(id) ON DELETE CASCADE,
    event_type varchar(50) CHECK (event_type IN (
        'created', 'stage_change', 'location_change', 'plant_count_change',
        'harvest', 'split', 'merge', 'quarantine', 'hold', 'destroy',
        'note_added', 'photo_added', 'measurement_recorded'
    )),
    event_data jsonb,
    performed_by uuid NOT NULL REFERENCES users(id),
    performed_at timestamptz NOT NULL,
    notes text,
    created_at timestamptz
)

-- Batch Relationships (splits, merges, transformations)
batch_relationships (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    parent_batch_id uuid NOT NULL REFERENCES batches(id) ON DELETE CASCADE,
    child_batch_id uuid NOT NULL REFERENCES batches(id) ON DELETE CASCADE,
    relationship_type varchar(50) CHECK (relationship_type IN ('split', 'merge', 'propagation', 'transformation')),
    plant_count_transferred int,
    transfer_date date,
    notes text,
    created_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(parent_batch_id, child_batch_id, relationship_type)
)

-- Mother Plants (source plants for cloning)
mother_plants (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    batch_id uuid NOT NULL REFERENCES batches(id) ON DELETE CASCADE,
    plant_id varchar(100) NOT NULL,
    strain_id uuid NOT NULL REFERENCES strains(id) ON DELETE RESTRICT,
    location_id uuid REFERENCES inventory_locations(id),
    room_id uuid REFERENCES rooms(id),
    status varchar(20) CHECK (status IN ('active', 'quarantine', 'retired', 'destroyed')),
    date_established date NOT NULL,
    last_propagation_date date,
    propagation_count int DEFAULT 0,
    max_propagation_count int,
    notes text,
    metadata jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id, plant_id)
)

-- Mother Plant Health Reminder Settings (user-configurable per site)
mother_health_reminder_settings (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    reminder_frequency_days int NOT NULL DEFAULT 7,
    reminder_enabled boolean DEFAULT TRUE,
    escalation_days int DEFAULT 3,
    notification_channels text[] DEFAULT ARRAY[]::text[],
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id)
)

-- Propagation Settings (site-wide propagation controls)
propagation_settings (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    daily_limit int,
    weekly_limit int,
    mother_propagation_limit int,
    requires_override_approval boolean DEFAULT TRUE,
    approver_role varchar(100),
    approver_policy jsonb,
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id)
)

-- Propagation Override Requests (approval workflow)
propagation_override_requests (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    requested_by uuid NOT NULL REFERENCES users(id),
    approver_id uuid REFERENCES users(id),
    mother_plant_id uuid REFERENCES mother_plants(id),
    batch_id uuid REFERENCES batches(id),
    requested_quantity int NOT NULL,
    reason text NOT NULL,
    status varchar(30) CHECK (status IN ('pending', 'approved', 'rejected', 'expired')),
    requested_on timestamptz NOT NULL,
    decision_notes text,
    resolved_on timestamptz
)

-- Mother Plant Health Logs
mother_health_logs (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    mother_plant_id uuid NOT NULL REFERENCES mother_plants(id) ON DELETE CASCADE,
    log_date date NOT NULL,
    health_status varchar(20) CHECK (health_status IN ('excellent', 'good', 'fair', 'poor', 'critical')),
    observations text,
    treatments_applied text,
    pest_pressure varchar(20) CHECK (pest_pressure IN ('none', 'low', 'medium', 'high')),
    disease_pressure varchar(20) CHECK (disease_pressure IN ('none', 'low', 'medium', 'high')),
    nutrient_deficiencies text[],
    environmental_notes text,
    photos_urls text[],
    logged_by uuid NOT NULL REFERENCES users(id),
    created_at timestamptz
)
```

> **Operational follow-up:** Notification channels remain stubbed until the communications platform lands. Track the reconnect work in the post-FRP backlog so email/Slack hooks can be enabled without schema changes.

**RLS Policies:**
```sql
-- Enable RLS on all tables
ALTER TABLE genetics ENABLE ROW LEVEL SECURITY;
ALTER TABLE phenotypes ENABLE ROW LEVEL SECURITY;
ALTER TABLE strains ENABLE ROW LEVEL SECURITY;
ALTER TABLE batch_stage_definitions ENABLE ROW LEVEL SECURITY;
ALTER TABLE batch_stage_transitions ENABLE ROW LEVEL SECURITY;
ALTER TABLE batches ENABLE ROW LEVEL SECURITY;
ALTER TABLE batch_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE batch_relationships ENABLE ROW LEVEL SECURITY;
ALTER TABLE batch_stage_history ENABLE ROW LEVEL SECURITY;
ALTER TABLE mother_plants ENABLE ROW LEVEL SECURITY;
ALTER TABLE mother_health_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE batch_code_rules ENABLE ROW LEVEL SECURITY;
ALTER TABLE propagation_settings ENABLE ROW LEVEL SECURITY;
ALTER TABLE propagation_override_requests ENABLE ROW LEVEL SECURITY;

-- Policy: Users can only access their site's genetics data
CREATE POLICY genetics_site_access ON genetics
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
CREATE INDEX idx_genetics_site_name ON genetics(site_id, name);
CREATE INDEX idx_phenotypes_genetics ON phenotypes(genetics_id);
CREATE INDEX idx_strains_genetics ON strains(genetics_id);
CREATE INDEX idx_batches_site_strain ON batches(site_id, strain_id);
CREATE INDEX idx_batches_current_stage ON batches(current_stage) WHERE status = 'active';
CREATE INDEX idx_batch_events_batch_type ON batch_events(batch_id, event_type);
CREATE INDEX idx_batch_relationships_parent ON batch_relationships(parent_batch_id);
CREATE INDEX idx_batch_relationships_child ON batch_relationships(child_batch_id);
CREATE INDEX idx_batch_stage_definitions_site_key ON batch_stage_definitions(site_id, stage_key);
CREATE INDEX idx_batch_stage_definitions_order ON batch_stage_definitions(site_id, sequence_order);
CREATE INDEX idx_batch_stage_transitions_site_from ON batch_stage_transitions(site_id, from_stage_id);
CREATE INDEX idx_batch_stage_history_batch ON batch_stage_history(batch_id);
CREATE INDEX idx_mother_plants_site_status ON mother_plants(site_id, status);
CREATE INDEX idx_mother_health_logs_plant_date ON mother_health_logs(mother_plant_id, log_date);
CREATE INDEX idx_batch_code_rules_site ON batch_code_rules(site_id) WHERE is_active = TRUE;
CREATE INDEX idx_propagation_settings_site ON propagation_settings(site_id);
CREATE INDEX idx_propagation_override_requests_status ON propagation_override_requests(site_id, status);
```

---

### Phase 2: Domain Layer (4-5 hours)

#### Domain Entities

**File Structure:**
```
src/backend/services/core-platform/genetics/Domain/
├── Entities/
│   ├── Genetics.cs
│   ├── Phenotype.cs
│   ├── Strain.cs
│   ├── Batch.cs
│   ├── BatchEvent.cs
│   ├── BatchRelationship.cs
│   ├── BatchStageDefinition.cs
│   ├── BatchStageTransition.cs
│   ├── BatchStageHistory.cs
│   ├── MotherPlant.cs
│   ├── MotherHealthLog.cs
│   ├── BatchCodeRule.cs
│   ├── PropagationSettings.cs
│   └── PropagationOverrideRequest.cs
├── ValueObjects/
│   ├── BatchCode.cs
│   ├── PlantId.cs
│   ├── GeneticProfile.cs
│   ├── TerpeneProfile.cs
│   ├── HealthAssessment.cs
│   ├── StageKey.cs
│   ├── TargetEnvironment.cs
│   └── ComplianceRequirements.cs
└── Enums/
    ├── GeneticType.cs (Indica, Sativa, Hybrid, Autoflower, Hemp)
    ├── YieldPotential.cs (Low, Medium, High, VeryHigh)
    ├── BatchType.cs (Seed, Clone, TissueCulture, MotherPlant)
    ├── BatchSourceType.cs (Purchase, Propagation, Breeding, TissueCulture)
    ├── BatchStatus.cs (Active, Quarantine, Hold, Destroyed, Completed, Transferred)
    ├── MotherPlantStatus.cs (Active, Quarantine, Retired, Destroyed)
    ├── HealthStatus.cs (Excellent, Good, Fair, Poor, Critical)
    ├── EventType.cs (Created, StageChange, LocationChange, etc.)
    ├── RelationshipType.cs (Split, Merge, Propagation, Transformation)
    ├── PressureLevel.cs (None, Low, Medium, High)
    ├── PropagationOverrideStatus.cs (Pending, Approved, Rejected, Expired)
    └── StandardBatchStage.cs (reference defaults provided during setup)
```

**Key Domain Methods:**

**Genetics.cs:**
```csharp
public class Genetics : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public GeneticType GeneticType { get; private set; }
    public (decimal Min, decimal Max) ThcPercentageRange { get; private set; }
    public (decimal Min, decimal Max) CbdPercentageRange { get; private set; }
    public int? FloweringTimeDays { get; private set; }
    public YieldPotential YieldPotential { get; private set; }
    public GeneticProfile GrowthCharacteristics { get; private set; }
    public TerpeneProfile TerpeneProfile { get; private set; }
    
    // Methods
    public void UpdateProfile(string description, GeneticProfile characteristics, TerpeneProfile terpenes);
    public void UpdateCannabinoidRanges(decimal thcMin, decimal thcMax, decimal cbdMin, decimal cbdMax);
    public void UpdateFloweringTime(int days);
    public bool CanDelete(); // Check for strains using this genetics
}
```

**Batch.cs:**
```csharp
public class Batch : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid StrainId { get; private set; }
    public BatchCode BatchCode { get; private set; }
    public string BatchName { get; private set; }
    public BatchType BatchType { get; private set; }
    public BatchSourceType SourceType { get; private set; }
    public Guid? ParentBatchId { get; private set; }
    public int Generation { get; private set; }
    public int PlantCount { get; private set; }
    public int? TargetPlantCount { get; private set; }
    public BatchStageDefinition CurrentStage { get; private set; }
    public DateTimeOffset StageStartedAt { get; private set; }
    public DateOnly? ExpectedHarvestDate { get; private set; }
    public DateOnly? ActualHarvestDate { get; private set; }
    public Guid? LocationId { get; private set; }
    public Guid? RoomId { get; private set; }
    public Guid? ZoneId { get; private set; }
    public BatchStatus Status { get; private set; }

    private readonly List<BatchEvent> _events = new();
    public IReadOnlyCollection<BatchEvent> Events => _events.AsReadOnly();

    // Methods
    public void ChangeStage(BatchStageDefinition targetStage, Guid userId, string? notes = null);
    public void UpdateLocation(Guid? locationId, Guid? roomId, Guid? zoneId, Guid userId);
    public void UpdatePlantCount(int newCount, Guid userId, string reason);
    public void Split(int plantCount, BatchCode newBatchCode, string newBatchName, Guid userId, bool isPartialSplit = true);
    public void Merge(Batch otherBatch, Guid userId);
    public void Quarantine(string reason, Guid userId);
    public void ReleaseFromQuarantine(Guid userId);
    public void Harvest(DateOnly harvestDate, Guid userId);
    public void Destroy(string reason, Guid userId);
    public void AddEvent(EventType eventType, object eventData, Guid userId, string? notes = null);
    public bool CanTransitionTo(BatchStageDefinition targetStage, IReadOnlyCollection<BatchStageTransition> allowedTransitions);
    public bool CanSplit(int plantCountToSplit);
    public bool CanMerge(Batch otherBatch);
    public bool RequiresPropagationOverride(int requestedCloneCount, PropagationSettings settings);
    public TimeSpan GetStageDuration();
    public IReadOnlyCollection<Batch> GetLineage();
    public static BatchCode GenerateBatchCode(BatchCodeRule rule, IReadOnlyDictionary<string, object> context);
}
```

**BatchStageDefinition.cs:**
```csharp
public class BatchStageDefinition : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public StageKey StageKey { get; private set; }
    public string DisplayName { get; private set; }
    public string? Description { get; private set; }
    public int SequenceOrder { get; private set; }
    public bool IsTerminal { get; private set; }
    public bool RequiresHarvestMetrics { get; private set; }

    public void Update(string displayName, string? description, int order, bool isTerminal, bool requiresHarvestMetrics, Guid userId);
}
```

**BatchStageTransition.cs:**
```csharp
public class BatchStageTransition : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid FromStageId { get; private set; }
    public Guid ToStageId { get; private set; }
    public bool AutoAdvance { get; private set; }
    public bool RequiresApproval { get; private set; }
    public string? ApprovalRole { get; private set; }

    public void Update(bool autoAdvance, bool requiresApproval, string? approvalRole, Guid userId);
}
```

**MotherPlant.cs:**
```csharp
public class MotherPlant : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid BatchId { get; private set; }
    public PlantId PlantId { get; private set; }
    public Guid StrainId { get; private set; }
    public Guid? LocationId { get; private set; }
    public Guid? RoomId { get; private set; }
    public MotherPlantStatus Status { get; private set; }
    public DateOnly DateEstablished { get; private set; }
    public DateOnly? LastPropagationDate { get; private set; }
    public int PropagationCount { get; private set; }

    private readonly List<MotherHealthLog> _healthLogs = new();
    public IReadOnlyCollection<MotherHealthLog> HealthLogs => _healthLogs.AsReadOnly();

    // Methods
    public void RecordHealthLog(HealthAssessment assessment, Guid userId);
    public void RegisterPropagation(int propagatedCount, Guid userId);
    public void Retire(string reason, Guid userId);
    public void Reactivate(Guid userId);
    public void UpdateLocation(Guid? locationId, Guid? roomId, Guid userId);
    public bool CanPropagate(PropagationSettings settings, int requestedCloneCount);
    public PropagationOverrideRequest RequestPropagationOverride(PropagationSettings settings, int requestedCloneCount, string reason, Guid requestedBy);
    public bool IsOverdueForHealthCheck(TimeSpan reminderFrequency);
    public HealthAssessment GetLatestAssessment();
    public TimeSpan GetAge();
    public DateOnly? GetNextHealthCheckDue(TimeSpan reminderFrequency);
}
```

**PropagationSettings.cs:**
```csharp
public class PropagationSettings : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public int? DailyLimit { get; private set; }
    public int? WeeklyLimit { get; private set; }
    public int? MotherPropagationLimit { get; private set; }
    public bool RequiresOverrideApproval { get; private set; }
    public string? ApproverRole { get; private set; }
    public IReadOnlyDictionary<string, string>? ApproverPolicy { get; private set; } // site-specific ABAC mapping

    public void UpdateLimits(int? dailyLimit, int? weeklyLimit, int? motherLimit, bool requiresApproval, string? approverRole, IReadOnlyDictionary<string, string>? approverPolicy, Guid updatedBy);
    public bool IsWithinLimits(int requestedCount, DateOnly date, int motherPropagationCount);
    public PropagationOverrideRequest CreateOverrideRequest(Guid requestedBy, Guid? motherPlantId, Guid? batchId, int requestedCount, string reason);
}
```

**PropagationOverrideRequest.cs:**
```csharp
public class PropagationOverrideRequest : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid RequestedBy { get; private set; }
    public Guid? MotherPlantId { get; private set; }
    public Guid? BatchId { get; private set; }
    public int RequestedQuantity { get; private set; }
    public string Reason { get; private set; }
    public PropagationOverrideStatus Status { get; private set; }
    public DateTimeOffset RequestedOn { get; private set; }
    public Guid? ApproverId { get; private set; }
    public DateTimeOffset? ResolvedOn { get; private set; }
    public string? DecisionNotes { get; private set; }

    public void Approve(Guid approverId, string? notes);
    public void Reject(Guid approverId, string? notes);
    public void Expire();
}
```

**HealthAssessment.cs (Value Object):**
```csharp
public readonly record struct HealthAssessment(
    HealthStatus Status,
    PressureLevel PestPressure,
    PressureLevel DiseasePressure,
    IReadOnlyCollection<string> NutrientDeficiencies,
    string? Observations,
    string? TreatmentsApplied,
    string? EnvironmentalNotes,
    IReadOnlyCollection<Uri> PhotoUrls
);
```

> **Note:** Approver policies are stored as JSON so each site can align overrides with its custom role taxonomy. Apply existing ABAC roles when present; otherwise allow admins to seed arbitrary claims and map them to propagation approvals.


---

### Phase 3: Application Layer (3-4 hours)

#### Application Services

**Files:**
```
src/backend/services/core-platform/genetics/Application/
├── Services/
│   ├── GeneticsManagementService.cs
│   ├── BatchLifecycleService.cs
│   ├── BatchStageConfigurationService.cs
│   └── MotherHealthService.cs (health + propagation controls)
├── DTOs/
│   ├── CreateGeneticsRequest.cs
│   ├── UpdateGeneticsRequest.cs
│   ├── CreatePhenotypeRequest.cs
│   ├── UpdatePhenotypeRequest.cs
│   ├── CreateStrainRequest.cs
│   ├── UpdateStrainRequest.cs
│   ├── CreateBatchRequest.cs
│   ├── UpdateBatchRequest.cs
│   ├── BatchStageChangeRequest.cs
│   ├── BatchSplitRequest.cs
│   ├── BatchMergeRequest.cs
│   ├── BatchStageRequest.cs
│   ├── BatchStageResponse.cs
│   ├── BatchStageOrderUpdateRequest.cs
│   ├── BatchStageTransitionRequest.cs
│   ├── BatchStageTransitionResponse.cs
│   ├── BatchCodeGenerationContext.cs
│   ├── BatchCodeRuleRequest.cs
│   ├── BatchCodeRuleResponse.cs
│   ├── UpdatePropagationSettingsRequest.cs
│   ├── PropagationSettingsResponse.cs
│   ├── CreatePropagationOverrideRequest.cs
│   ├── PropagationOverrideDecisionRequest.cs
│   ├── PropagationOverrideResponse.cs
│   ├── UpdateMotherPlantRequest.cs
│   ├── RegisterPropagationRequest.cs
│   ├── MotherPlantHealthLogRequest.cs
│   ├── HealthAssessmentDto.cs
│   ├── GeneticsResponse.cs
│   ├── PhenotypeResponse.cs
│   ├── StrainResponse.cs
│   ├── BatchResponse.cs
│   ├── BatchLineageResponse.cs
│   ├── BatchEventResponse.cs
│   ├── MotherPlantResponse.cs
│   └── MotherPlantHealthSummaryResponse.cs
└── Interfaces/
    ├── IGeneticsManagementService.cs
    ├── IBatchLifecycleService.cs
    ├── IBatchStageConfigurationService.cs
    └── IMotherHealthService.cs
```

**Key Service Methods:**

**IGeneticsManagementService:**
```csharp
public interface IGeneticsManagementService
{
    Task<GeneticsResponse> CreateGeneticsAsync(Guid siteId, CreateGeneticsRequest request, Guid userId, CancellationToken ct);
    Task<GeneticsResponse?> GetGeneticsByIdAsync(Guid siteId, Guid geneticsId, CancellationToken ct);
    Task<IReadOnlyList<GeneticsResponse>> GetGeneticsBySiteAsync(Guid siteId, CancellationToken ct);
    Task<GeneticsResponse> UpdateGeneticsAsync(Guid siteId, Guid geneticsId, UpdateGeneticsRequest request, Guid userId, CancellationToken ct);
    Task DeleteGeneticsAsync(Guid siteId, Guid geneticsId, Guid userId, CancellationToken ct);

    Task<PhenotypeResponse> CreatePhenotypeAsync(Guid siteId, CreatePhenotypeRequest request, Guid userId, CancellationToken ct);
    Task<PhenotypeResponse> UpdatePhenotypeAsync(Guid siteId, Guid phenotypeId, UpdatePhenotypeRequest request, Guid userId, CancellationToken ct);
    Task DeletePhenotypeAsync(Guid siteId, Guid phenotypeId, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<PhenotypeResponse>> GetPhenotypesByGeneticsAsync(Guid geneticsId, CancellationToken ct);

    Task<StrainResponse> CreateStrainAsync(Guid siteId, CreateStrainRequest request, Guid userId, CancellationToken ct);
    Task<StrainResponse> UpdateStrainAsync(Guid siteId, Guid strainId, UpdateStrainRequest request, Guid userId, CancellationToken ct);
    Task DeleteStrainAsync(Guid siteId, Guid strainId, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<StrainResponse>> GetStrainsBySiteAsync(Guid siteId, CancellationToken ct);

    Task<bool> CanDeleteGeneticsAsync(Guid geneticsId, CancellationToken ct);
    Task<bool> CanDeleteStrainAsync(Guid strainId, CancellationToken ct);
}
```

**IBatchLifecycleService:**
```csharp
public interface IBatchLifecycleService
{
    Task<BatchResponse> CreateBatchAsync(Guid siteId, CreateBatchRequest request, Guid userId, CancellationToken ct);
    Task<BatchResponse> UpdateBatchAsync(Guid siteId, Guid batchId, UpdateBatchRequest request, Guid userId, CancellationToken ct);
    Task<BatchResponse> ChangeBatchStageAsync(Guid siteId, Guid batchId, BatchStageChangeRequest request, Guid userId, CancellationToken ct);
    Task<BatchResponse> UpdateBatchLocationAsync(Guid siteId, Guid batchId, UpdateBatchLocationRequest request, Guid userId, CancellationToken ct);
    Task<BatchResponse> UpdateBatchPlantCountAsync(Guid siteId, Guid batchId, UpdatePlantCountRequest request, Guid userId, CancellationToken ct);
    Task<BatchResponse> SplitBatchAsync(Guid siteId, Guid batchId, BatchSplitRequest request, Guid userId, CancellationToken ct);
    Task<BatchResponse> MergeBatchesAsync(Guid siteId, BatchMergeRequest request, Guid userId, CancellationToken ct);
    Task<BatchResponse> QuarantineBatchAsync(Guid siteId, Guid batchId, string reason, Guid userId, CancellationToken ct);
    Task<BatchResponse> ReleaseBatchFromQuarantineAsync(Guid siteId, Guid batchId, Guid userId, CancellationToken ct);
    Task<BatchResponse> HarvestBatchAsync(Guid siteId, Guid batchId, DateOnly harvestDate, Guid userId, CancellationToken ct);
    Task<BatchResponse> DestroyBatchAsync(Guid siteId, Guid batchId, string reason, Guid userId, CancellationToken ct);
    Task<BatchLineageResponse> GetBatchLineageAsync(Guid siteId, Guid batchId, CancellationToken ct);
    Task<IReadOnlyList<BatchResponse>> GetBatchesBySiteAsync(Guid siteId, BatchStatus? status, Guid? stageId, CancellationToken ct);
    Task<IReadOnlyList<BatchEventResponse>> GetBatchEventsAsync(Guid siteId, Guid batchId, CancellationToken ct);

    Task<BatchCodeResponse> GenerateBatchCodeAsync(Guid siteId, BatchCodeGenerationContext context, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<BatchCodeRuleResponse>> GetBatchCodeRulesAsync(Guid siteId, CancellationToken ct);
    Task<BatchCodeRuleResponse> UpsertBatchCodeRuleAsync(Guid siteId, Guid? ruleId, BatchCodeRuleRequest request, Guid userId, CancellationToken ct);
    Task ArchiveBatchCodeRuleAsync(Guid siteId, Guid ruleId, Guid userId, CancellationToken ct);
}
```

**IBatchStageConfigurationService:**
```csharp
public interface IBatchStageConfigurationService
{
    Task<IReadOnlyList<BatchStageResponse>> GetStagesAsync(Guid siteId, CancellationToken ct);
    Task<BatchStageResponse> CreateStageAsync(Guid siteId, BatchStageRequest request, Guid userId, CancellationToken ct);
    Task<BatchStageResponse> UpdateStageAsync(Guid siteId, Guid stageId, BatchStageRequest request, Guid userId, CancellationToken ct);
    Task ReorderStagesAsync(Guid siteId, BatchStageOrderUpdateRequest request, Guid userId, CancellationToken ct);
    Task DeleteStageAsync(Guid siteId, Guid stageId, Guid userId, CancellationToken ct);

    Task<IReadOnlyList<BatchStageTransitionResponse>> GetTransitionsAsync(Guid siteId, CancellationToken ct);
    Task<BatchStageTransitionResponse> UpsertTransitionAsync(Guid siteId, Guid? transitionId, BatchStageTransitionRequest request, Guid userId, CancellationToken ct);
    Task DeleteTransitionAsync(Guid siteId, Guid transitionId, Guid userId, CancellationToken ct);

    Task SeedDefaultStagesAsync(Guid siteId, IEnumerable<StandardBatchStage> defaults, Guid userId, CancellationToken ct);
}
```

**IMotherHealthService:**
```csharp
public interface IMotherHealthService
{
    Task<MotherPlantResponse> CreateMotherPlantAsync(Guid siteId, CreateMotherPlantRequest request, Guid userId, CancellationToken ct);
    Task<MotherPlantResponse?> GetMotherPlantByIdAsync(Guid siteId, Guid motherPlantId, CancellationToken ct);
    Task<IReadOnlyList<MotherPlantResponse>> GetMotherPlantsBySiteAsync(Guid siteId, MotherPlantStatus? status, CancellationToken ct);
    Task<MotherPlantResponse> UpdateMotherPlantAsync(Guid siteId, Guid motherPlantId, UpdateMotherPlantRequest request, Guid userId, CancellationToken ct);
    Task<MotherPlantResponse> RecordHealthLogAsync(Guid siteId, Guid motherPlantId, MotherPlantHealthLogRequest request, Guid userId, CancellationToken ct);
    Task<MotherPlantResponse> RegisterPropagationAsync(Guid siteId, Guid motherPlantId, int propagatedCount, Guid userId, CancellationToken ct);
    Task<MotherPlantHealthSummaryResponse> GetHealthSummaryAsync(Guid siteId, Guid motherPlantId, CancellationToken ct);
    Task<IReadOnlyList<MotherHealthLogResponse>> GetHealthLogsAsync(Guid siteId, Guid motherPlantId, CancellationToken ct);
    Task<IReadOnlyList<MotherPlantResponse>> GetOverdueForHealthCheckAsync(Guid siteId, CancellationToken ct);

    Task<PropagationSettingsResponse> GetPropagationSettingsAsync(Guid siteId, CancellationToken ct);
    Task<PropagationSettingsResponse> UpdatePropagationSettingsAsync(Guid siteId, UpdatePropagationSettingsRequest request, Guid userId, CancellationToken ct);
    Task<PropagationOverrideResponse> RequestPropagationOverrideAsync(Guid siteId, CreatePropagationOverrideRequest request, Guid userId, CancellationToken ct);
    Task<PropagationOverrideResponse> DecidePropagationOverrideAsync(Guid siteId, Guid overrideId, PropagationOverrideDecisionRequest request, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<PropagationOverrideResponse>> GetPropagationOverridesAsync(Guid siteId, PropagationOverrideStatus? status, CancellationToken ct);

    Task<MotherHealthReminderSettingsResponse> GetHealthReminderSettingsAsync(Guid siteId, CancellationToken ct);
    Task<MotherHealthReminderSettingsResponse> UpdateHealthReminderSettingsAsync(Guid siteId, UpdateHealthReminderSettingsRequest request, Guid userId, CancellationToken ct);
}
```

---

### Phase 4: Infrastructure Layer (3-4 hours)

#### Repositories

**Files:**
```
src/backend/services/core-platform/genetics/Infrastructure/Persistence/
├── GeneticsDbContext.cs
├── GeneticsRepository.cs
├── PhenotypeRepository.cs
├── StrainRepository.cs
├── BatchRepository.cs
├── BatchEventRepository.cs
├── BatchRelationshipRepository.cs
├── BatchStageDefinitionRepository.cs
├── BatchStageTransitionRepository.cs
├── BatchStageHistoryRepository.cs
├── BatchCodeRuleRepository.cs
├── MotherPlantRepository.cs
├── MotherHealthLogRepository.cs
├── PropagationSettingsRepository.cs
└── PropagationOverrideRequestRepository.cs
```

**Key Repository Methods:**

**IBatchRepository:**
```csharp
public interface IBatchRepository : IRepository<Batch, Guid>
{
    Task<Batch?> GetByBatchCodeAsync(Guid siteId, string batchCode, CancellationToken ct);
    Task<IReadOnlyList<Batch>> GetBySiteAndStatusAsync(Guid siteId, BatchStatus? status, CancellationToken ct);
    Task<IReadOnlyList<Batch>> GetByStrainAsync(Guid siteId, Guid strainId, CancellationToken ct);
    Task<IReadOnlyList<Batch>> GetByStageAsync(Guid siteId, Guid stageId, CancellationToken ct);
    Task<IReadOnlyList<Batch>> GetByLocationAsync(Guid siteId, Guid locationId, CancellationToken ct);
    Task<IReadOnlyList<Batch>> GetChildrenAsync(Guid siteId, Guid parentBatchId, CancellationToken ct);
    Task<IReadOnlyCollection<Batch>> GetLineageAsync(Guid siteId, Guid batchId, CancellationToken ct);
    Task UpdateStageAsync(Guid siteId, Guid batchId, Guid newStageId, DateTimeOffset stageStartedAt, CancellationToken ct);
    Task UpdateLocationAsync(Guid siteId, Guid batchId, Guid? locationId, Guid? roomId, Guid? zoneId, CancellationToken ct);
    Task UpdatePlantCountAsync(Guid siteId, Guid batchId, int newCount, CancellationToken ct);
}
```

**IBatchStageDefinitionRepository:**
```csharp
public interface IBatchStageDefinitionRepository : IRepository<BatchStageDefinition, Guid>
{
    Task<IReadOnlyList<BatchStageDefinition>> GetBySiteAsync(Guid siteId, CancellationToken ct);
    Task<bool> ExistsWithKeyAsync(Guid siteId, StageKey key, CancellationToken ct);
    Task ReorderAsync(Guid siteId, IReadOnlyCollection<(Guid StageId, int Order)> reorderedStages, Guid userId, CancellationToken ct);
}
```

**IBatchStageTransitionRepository:**
```csharp
public interface IBatchStageTransitionRepository : IRepository<BatchStageTransition, Guid>
{
    Task<IReadOnlyList<BatchStageTransition>> GetBySiteAsync(Guid siteId, CancellationToken ct);
    Task<bool> ExistsAsync(Guid siteId, Guid fromStageId, Guid toStageId, CancellationToken ct);
}
```

**IBatchStageHistoryRepository:**
```csharp
public interface IBatchStageHistoryRepository : IRepository<BatchStageHistory, Guid>
{
    Task<IReadOnlyList<BatchStageHistory>> GetByBatchAsync(Guid siteId, Guid batchId, CancellationToken ct);
}
```

**IBatchCodeRuleRepository:**
```csharp
public interface IBatchCodeRuleRepository
{
    Task<IReadOnlyList<BatchCodeRule>> GetBySiteAsync(Guid siteId, CancellationToken ct);
    Task<BatchCodeRule?> GetByIdAsync(Guid siteId, Guid ruleId, CancellationToken ct);
    Task<BatchCodeRule> UpsertAsync(BatchCodeRule rule, CancellationToken ct);
    Task ArchiveAsync(Guid siteId, Guid ruleId, Guid userId, CancellationToken ct);
}
```

**IPropagationSettingsRepository:**
```csharp
public interface IPropagationSettingsRepository
{
    Task<PropagationSettings?> GetBySiteAsync(Guid siteId, CancellationToken ct);
    Task<PropagationSettings> UpsertAsync(PropagationSettings settings, CancellationToken ct);
}
```

**IPropagationOverrideRequestRepository:**
```csharp
public interface IPropagationOverrideRequestRepository
{
    Task<IReadOnlyList<PropagationOverrideRequest>> GetBySiteAsync(Guid siteId, PropagationOverrideStatus? status, CancellationToken ct);
    Task<PropagationOverrideRequest?> GetByIdAsync(Guid siteId, Guid overrideId, CancellationToken ct);
    Task<PropagationOverrideRequest> AddAsync(PropagationOverrideRequest request, CancellationToken ct);
    Task UpdateAsync(PropagationOverrideRequest request, CancellationToken ct);
}
```

**IMotherPlantRepository:**
```csharp
public interface IMotherPlantRepository : IRepository<MotherPlant, Guid>
{
    Task<MotherPlant?> GetByPlantIdAsync(Guid siteId, string plantId, CancellationToken ct);
    Task<IReadOnlyList<MotherPlant>> GetBySiteAndStatusAsync(Guid siteId, MotherPlantStatus? status, CancellationToken ct);
    Task<IReadOnlyList<MotherPlant>> GetByStrainAsync(Guid siteId, Guid strainId, CancellationToken ct);
    Task<IReadOnlyList<MotherPlant>> GetByLocationAsync(Guid siteId, Guid locationId, CancellationToken ct);
    Task UpdatePropagationAsync(Guid siteId, Guid motherPlantId, int newCount, DateOnly lastPropagationDate, CancellationToken ct);
    Task<IReadOnlyList<MotherPlant>> GetOverdueForHealthCheckAsync(Guid siteId, TimeSpan threshold, CancellationToken ct);
    Task<int> GetPropagationCountForWindowAsync(Guid siteId, DateOnly windowStart, DateOnly windowEnd, CancellationToken ct);
}
```

---

### Phase 5: API Layer (2-3 hours)

#### Controllers

**Files:**
```
src/backend/services/core-platform/genetics/API/Controllers/
├── GeneticsController.cs
├── StrainsController.cs
├── BatchesController.cs
├── BatchStagesController.cs
├── BatchCodeRulesController.cs
├── MotherPlantsController.cs
└── PropagationController.cs
```

**Key Endpoints:**

**GeneticsController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/genetics")]
public class GeneticsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<GeneticsResponse>> CreateGenetics(Guid siteId, CreateGeneticsRequest request);
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GeneticsResponse>>> GetGenetics(Guid siteId);
    
    [HttpGet("{geneticsId}")]
    public async Task<ActionResult<GeneticsResponse>> GetGeneticsById(Guid siteId, Guid geneticsId);
    
    [HttpPut("{geneticsId}")]
    public async Task<ActionResult<GeneticsResponse>> UpdateGenetics(Guid siteId, Guid geneticsId, UpdateGeneticsRequest request);
    
    [HttpDelete("{geneticsId}")]
    public async Task<IActionResult> DeleteGenetics(Guid siteId, Guid geneticsId);
}
```

**BatchesController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/batches")]
public class BatchesController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BatchResponse>> CreateBatch(Guid siteId, CreateBatchRequest request);

    [HttpPut("{batchId}")]
    public async Task<ActionResult<BatchResponse>> UpdateBatch(Guid siteId, Guid batchId, UpdateBatchRequest request);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BatchResponse>>> GetBatches(Guid siteId, [FromQuery] BatchStatus? status, [FromQuery] Guid? stageId);

    [HttpGet("{batchId}")]
    public async Task<ActionResult<BatchResponse>> GetBatch(Guid siteId, Guid batchId);

    [HttpPost("{batchId}/stage-change")]
    public async Task<ActionResult<BatchResponse>> ChangeStage(Guid siteId, Guid batchId, BatchStageChangeRequest request);

    [HttpPost("{batchId}/split")]
    public async Task<ActionResult<BatchResponse>> SplitBatch(Guid siteId, Guid batchId, BatchSplitRequest request);

    [HttpPost("merge")]
    public async Task<ActionResult<BatchResponse>> MergeBatches(Guid siteId, BatchMergeRequest request);

    [HttpPost("{batchId}/quarantine")]
    public async Task<ActionResult<BatchResponse>> QuarantineBatch(Guid siteId, Guid batchId, QuarantineRequest request);

    [HttpPost("{batchId}/release-quarantine")]
    public async Task<ActionResult<BatchResponse>> ReleaseFromQuarantine(Guid siteId, Guid batchId, ReleaseQuarantineRequest request);

    [HttpPost("{batchId}/harvest")]
    public async Task<ActionResult<BatchResponse>> HarvestBatch(Guid siteId, Guid batchId, HarvestRequest request);

    [HttpPost("{batchId}/destroy")]
    public async Task<ActionResult<BatchResponse>> DestroyBatch(Guid siteId, Guid batchId, DestroyRequest request);

    [HttpGet("{batchId}/lineage")]
    public async Task<ActionResult<BatchLineageResponse>> GetLineage(Guid siteId, Guid batchId);

    [HttpGet("{batchId}/events")]
    public async Task<ActionResult<IReadOnlyList<BatchEventResponse>>> GetEvents(Guid siteId, Guid batchId);
}
```

**MotherPlantsController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/mother-plants")]
public class MotherPlantsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MotherPlantResponse>> CreateMotherPlant(Guid siteId, CreateMotherPlantRequest request);

    [HttpPut("{motherPlantId}")]
    public async Task<ActionResult<MotherPlantResponse>> UpdateMotherPlant(Guid siteId, Guid motherPlantId, UpdateMotherPlantRequest request);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MotherPlantResponse>>> GetMotherPlants(Guid siteId, [FromQuery] MotherPlantStatus? status);

    [HttpGet("{motherPlantId}")]
    public async Task<ActionResult<MotherPlantResponse>> GetMotherPlant(Guid siteId, Guid motherPlantId);

    [HttpPost("{motherPlantId}/health-log")]
    public async Task<ActionResult<MotherPlantResponse>> RecordHealthLog(Guid siteId, Guid motherPlantId, MotherPlantHealthLogRequest request);

    [HttpPost("{motherPlantId}/propagation/register")]
    public async Task<ActionResult<MotherPlantResponse>> RegisterPropagation(Guid siteId, Guid motherPlantId, RegisterPropagationRequest request);

    [HttpPost("{motherPlantId}/propagation/override")]
    public async Task<ActionResult<PropagationOverrideResponse>> RequestOverride(Guid siteId, Guid motherPlantId, CreatePropagationOverrideRequest request);

    [HttpGet("{motherPlantId}/health-logs")]
    public async Task<ActionResult<IReadOnlyList<MotherHealthLogResponse>>> GetHealthLogs(Guid siteId, Guid motherPlantId);

    [HttpGet("{motherPlantId}/health-summary")]
    public async Task<ActionResult<MotherPlantHealthSummaryResponse>> GetHealthSummary(Guid siteId, Guid motherPlantId);
}
```

**BatchStagesController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/batch-stages")]
public class BatchStagesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BatchStageResponse>>> GetStages(Guid siteId);

    [HttpPost]
    public async Task<ActionResult<BatchStageResponse>> CreateStage(Guid siteId, BatchStageRequest request);

    [HttpPut("{stageId}")]
    public async Task<ActionResult<BatchStageResponse>> UpdateStage(Guid siteId, Guid stageId, BatchStageRequest request);

    [HttpPost("reorder")]
    public async Task<IActionResult> ReorderStages(Guid siteId, BatchStageOrderUpdateRequest request);

    [HttpDelete("{stageId}")]
    public async Task<IActionResult> DeleteStage(Guid siteId, Guid stageId);

    [HttpGet("transitions")]
    public async Task<ActionResult<IReadOnlyList<BatchStageTransitionResponse>>> GetTransitions(Guid siteId);

    [HttpPost("transitions")]
    public async Task<ActionResult<BatchStageTransitionResponse>> UpsertTransition(Guid siteId, BatchStageTransitionRequest request);

    [HttpDelete("transitions/{transitionId}")]
    public async Task<IActionResult> DeleteTransition(Guid siteId, Guid transitionId);
}
```

**BatchCodeRulesController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/batch-code-rules")]
public class BatchCodeRulesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BatchCodeRuleResponse>>> GetRules(Guid siteId);

    [HttpPost]
    public async Task<ActionResult<BatchCodeRuleResponse>> CreateRule(Guid siteId, BatchCodeRuleRequest request);

    [HttpPut("{ruleId}")]
    public async Task<ActionResult<BatchCodeRuleResponse>> UpdateRule(Guid siteId, Guid ruleId, BatchCodeRuleRequest request);

    [HttpDelete("{ruleId}")]
    public async Task<IActionResult> ArchiveRule(Guid siteId, Guid ruleId);

    [HttpPost("preview")]
    public async Task<ActionResult<BatchCodeResponse>> Preview(Guid siteId, BatchCodeGenerationContext context);
}
```

**PropagationController.cs:**
```csharp
[ApiController]
[Route("api/sites/{siteId}/propagation")]
public class PropagationController : ControllerBase
{
    [HttpGet("settings")]
    public async Task<ActionResult<PropagationSettingsResponse>> GetSettings(Guid siteId);

    [HttpPut("settings")]
    public async Task<ActionResult<PropagationSettingsResponse>> UpdateSettings(Guid siteId, UpdatePropagationSettingsRequest request);

    [HttpGet("overrides")]
    public async Task<ActionResult<IReadOnlyList<PropagationOverrideResponse>>> GetOverrides(Guid siteId, [FromQuery] PropagationOverrideStatus? status);

    [HttpPost("overrides/{overrideId}/decision")]
    public async Task<ActionResult<PropagationOverrideResponse>> Decide(Guid siteId, Guid overrideId, PropagationOverrideDecisionRequest request);
}
```

---

### Phase 6: Validators (1 hour)

**Files:**
```
src/backend/services/core-platform/genetics/API/Validators/
├── CreateGeneticsRequestValidator.cs
├── UpdateGeneticsRequestValidator.cs
├── CreateStrainRequestValidator.cs
├── UpdateStrainRequestValidator.cs
├── CreateBatchRequestValidator.cs
├── UpdateBatchRequestValidator.cs
├── BatchStageRequestValidator.cs
├── BatchStageOrderUpdateRequestValidator.cs
├── BatchStageTransitionRequestValidator.cs
├── BatchStageChangeRequestValidator.cs
├── BatchSplitRequestValidator.cs
├── BatchMergeRequestValidator.cs
├── BatchCodeRuleRequestValidator.cs
├── BatchCodeGenerationContextValidator.cs
├── UpdatePropagationSettingsRequestValidator.cs
├── RegisterPropagationRequestValidator.cs
├── CreatePropagationOverrideRequestValidator.cs
├── PropagationOverrideDecisionRequestValidator.cs
├── UpdateMotherPlantRequestValidator.cs
└── MotherPlantHealthLogRequestValidator.cs
```

---

### Phase 7: Unit Tests (2-3 hours)

**Files:**
```
src/backend/services/core-platform/genetics/Tests/Unit/
├── Domain/
│   ├── GeneticsTests.cs
│   ├── PhenotypeTests.cs
│   ├── StrainTests.cs
│   ├── BatchTests.cs
│   ├── BatchStageDefinitionTests.cs
│   ├── BatchStageTransitionTests.cs
│   ├── BatchStageHistoryTests.cs
│   ├── BatchCodeRuleTests.cs
│   ├── PropagationSettingsTests.cs
│   └── PropagationOverrideRequestTests.cs
└── Services/
    ├── GeneticsManagementServiceTests.cs
    ├── BatchLifecycleServiceTests.cs
    ├── BatchStageConfigurationServiceTests.cs
    ├── BatchCodeRuleServiceTests.cs
    └── MotherHealthServiceTests.cs
```

**Test Scenarios:**
- Genetics + phenotype CRUD validation
- Batch lifecycle transitions, regulatory stage coverage, and lineage
- Stage definition CRUD, ordering, and transition rules
- Batch code rule evaluation (rule parsing, reset policies, collision prevention)
- Propagation limit enforcement vs override workflow
- Mother plant health assessments and reminder scheduling
- Approval status transitions for propagation overrides
- RLS policy enforcement across all repositories

---

### Phase 8: Integration Tests (2-3 hours)

**Files:**
```
src/backend/services/core-platform/genetics/Tests/Integration/
├── GeneticsManagementTests.cs
├── BatchLifecycleTests.cs
├── BatchStageConfigurationTests.cs
├── BatchCodeRuleTests.cs
├── MotherPlantTests.cs
├── PropagationControlTests.cs
└── RlsGeneticsTests.cs (extended to cover propagation + batch-code RLS)
```

**Test Scenarios:**
- Create genetics → strain → batch (E2E) with regulatory stage transitions
- Stage definition CRUD + transition enforcement across lifecycle
- Batch code rule evaluation + preview (jurisdiction-specific formatting)
- Batch splitting/merging with lineage graph validation
- Propagation limit enforcement, override request, approval, and audit trail
- Mother plant health tracking plus reminder scheduling
- RLS: cross-site access blocked for genetics, batches, rules, propagation data
- Event log completeness for batch + propagation actions

---

## 📊 TASK BREAKDOWN WITH ESTIMATES

| Phase | Task | Est. Hours | Owner |
|-------|------|------------|-------|
| **1. Database** | Migration 1: Genetics tables | 1.5-2 | Backend |
| | Migration 2: Batch + stage tables | 2-2.5 | Backend |
| **2. Domain** | 14 entity files | 2.5-3 | Backend |
| | 8 value object files | 0.75-1.25 | Backend |
| | 12 enum/reference files | 0.5-1 | Backend |
| | Domain logic methods | 1.5-2 | Backend |
| **3. Application** | 4 service implementations | 2-2.5 | Backend |
| | 28 DTO files | 2-2.5 | Backend |
| | 4 interface files | 0.75 | Backend |
| **4. Infrastructure** | DbContext + 11 repositories | 2.5-3 | Backend |
| | RLS context integration | 0.75-1.25 | Backend |
| | Connection/retry logic | 0.5 | Backend |
| **5. API** | 7 controllers (~850 lines) | 2.5-3 | Backend |
| | Program.cs DI registration | 0.5 | Backend |
| **6. Validators** | 20 validator files | 1.5 | Backend |
| **7. Unit Tests** | 15 test files | 3-3.5 | Backend |
| **8. Integration Tests** | 6 test files | 2.5-3 | Backend |
| **TOTAL** | | **24-28** | |

---

## ✅ QUALITY GATES (Same as FRP-01/FRP-02)

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
- ✅ **Batch lineage tracked correctly** - Implemented via `batch_relationships` table + lineage queries
- ✅ **Mother plant health logs retrievable** - `mother_health_logs` table + health summary endpoints
- ✅ **Strain-specific blueprints associable** - Strain-to-batch relationship + blueprint metadata
- ✅ **Batch state machine enforces valid transitions** - Domain logic + validation
- ✅ **RLS blocks cross-site access** - Integration tests validate

---

## 🚀 DEPENDENCIES & BLOCKING

### Prerequisites (All Met ✅)
- ✅ FRP-01 Complete (Identity, RLS, ABAC)
- ✅ FRP-02 Complete (Spatial, Equipment)
- ✅ Database infrastructure (Supabase)
- ✅ API infrastructure (ASP.NET Core)

### Blocks (After FRP-03 Complete)
- **FRP-07: Inventory** - Needs batch tracking for lot creation
- **FRP-08: Processing** - Needs batch relationships for yield tracking
- **FRP-09: Compliance** - Needs lineage data for METRC reporting

---

## 📝 DESIGN DECISIONS CONFIRMED

1. **Batch Stage Templates:** ✅ **Site-configurable lifecycle stages & transitions** - Default templates provided; admins can tailor per jurisdiction or via commissioning services
2. **Batch Code Generation:** ✅ **Rule-driven with user-defined expressions** - Teams define compliant patterns per jurisdiction (prefixes, sequence seeds, resets)
3. **Propagation Limits:** ✅ **Site-wide limits with approval workflow** - Daily/weekly caps plus per-mother guardrails; overrides go through routed approvals
4. **Health Log Frequency:** ✅ **Event-driven with user-configurable reminder cadence** - Reminders stored now, delivery hooks wired once comms platform ships
5. **Lineage Depth:** ✅ **Unlimited with performance monitoring** - Track all generations with query optimization
6. **Batch Splitting:** ✅ **Both partial and complete splits with validation** - Flexible splitting with business rules

---

## 🎯 SUCCESS CRITERIA

**Definition of Done:**
- ✅ All 8 quality gates passed
- ✅ Batch lifecycle state machine operational
- ✅ Mother plant health tracking complete
- ✅ Lineage relationships properly maintained
- ✅ RLS validated (cross-site blocked)
- ✅ Integration tests passing
- ✅ Swagger docs published
- ✅ Ready for FRP-07 (Inventory) handoff

**Expected Outcome:**
- 45-55 C# files created
- ~5,000-6,500 lines of code
- Complete genetics and batch foundation
- Production-ready API
- FRP-07, FRP-08, FRP-09 unblocked

---

**Status:** 🎯 READY FOR REVIEW & APPROVAL  
**Next Step:** Review plan → Get approval → Begin implementation  
**Estimated Completion:** 18-22 hours from start
