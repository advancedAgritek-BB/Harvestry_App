# FRP-03: Genetics, Strains & Batches - Implementation Plan

**Status:** 🎯 READY TO START  
**Estimated Effort:** 18-22 hours  
**Prerequisites:** ✅ FRP-01 Complete (Identity, RLS, ABAC), ✅ FRP-02 Complete (Spatial, Equipment)  
**Blocks:** FRP-07 (Inventory), FRP-08 (Processing), FRP-09 (Compliance)

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
-- Batches (groups of plants with shared genetics)
batches (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    strain_id uuid NOT NULL REFERENCES strains(id) ON DELETE RESTRICT,
    batch_code varchar(100) NOT NULL,
    batch_name varchar(200),
    batch_type varchar(50) CHECK (batch_type IN ('seed', 'clone', 'tissue_culture', 'mother_plant')),
    source_type varchar(50) CHECK (source_type IN ('purchase', 'propagation', 'breeding', 'tissue_culture')),
    source_batch_id uuid REFERENCES batches(id),
    parent_batch_id uuid REFERENCES batches(id),
    generation int DEFAULT 1,
    plant_count int NOT NULL,
    target_plant_count int,
    current_stage varchar(50) CHECK (current_stage IN (
        'germination', 'seedling', 'veg', 'pre_flower', 'flower', 
        'harvest', 'cure', 'packaged', 'shipped', 'destroyed'
    )),
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

-- Batch Code Generation Settings (user-configurable per site)
batch_code_settings (
    id uuid PRIMARY KEY,
    site_id uuid NOT NULL REFERENCES sites(id),
    prefix varchar(20) NOT NULL,
    format varchar(50) NOT NULL DEFAULT '{prefix}-{year}-{sequence}',
    sequence_start int DEFAULT 1,
    sequence_padding int DEFAULT 4,
    is_active boolean DEFAULT TRUE,
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id, is_active) WHERE is_active = TRUE
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
    notification_channels text[] DEFAULT ARRAY['email', 'slack'],
    created_at timestamptz,
    updated_at timestamptz,
    created_by uuid NOT NULL REFERENCES users(id),
    updated_by uuid NOT NULL REFERENCES users(id),
    UNIQUE(site_id)
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

**RLS Policies:**
```sql
-- Enable RLS on all tables
ALTER TABLE genetics ENABLE ROW LEVEL SECURITY;
ALTER TABLE phenotypes ENABLE ROW LEVEL SECURITY;
ALTER TABLE strains ENABLE ROW LEVEL SECURITY;
ALTER TABLE batches ENABLE ROW LEVEL SECURITY;
ALTER TABLE batch_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE batch_relationships ENABLE ROW LEVEL SECURITY;
ALTER TABLE mother_plants ENABLE ROW LEVEL SECURITY;
ALTER TABLE mother_health_logs ENABLE ROW LEVEL SECURITY;

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
CREATE INDEX idx_mother_plants_site_status ON mother_plants(site_id, status);
CREATE INDEX idx_mother_health_logs_plant_date ON mother_health_logs(mother_plant_id, log_date);
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
│   ├── MotherPlant.cs
│   └── MotherHealthLog.cs
├── ValueObjects/
│   ├── BatchCode.cs
│   ├── PlantId.cs
│   ├── GeneticProfile.cs
│   ├── TerpeneProfile.cs
│   └── HealthStatus.cs
└── Enums/
    ├── GeneticType.cs (Indica, Sativa, Hybrid, Autoflower, Hemp)
    ├── BatchType.cs (Seed, Clone, TissueCulture, MotherPlant)
    ├── BatchStage.cs (Germination, Seedling, Veg, PreFlower, Flower, Harvest, Cure, Packaged, Shipped, Destroyed)
    ├── BatchStatus.cs (Active, Quarantine, Hold, Destroyed, Completed)
    ├── EventType.cs (Created, StageChange, LocationChange, etc.)
    ├── RelationshipType.cs (Split, Merge, Propagation, Transformation)
    └── HealthStatus.cs (Excellent, Good, Fair, Poor, Critical)
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
    public SourceType SourceType { get; private set; }
    public Guid? SourceBatchId { get; private set; }
    public Guid? ParentBatchId { get; private set; }
    public int Generation { get; private set; }
    public int PlantCount { get; private set; }
    public int? TargetPlantCount { get; private set; }
    public BatchStage CurrentStage { get; private set; }
    public DateTime? StageStartedAt { get; private set; }
    public DateOnly? ExpectedHarvestDate { get; private set; }
    public DateOnly? ActualHarvestDate { get; private set; }
    public Guid? LocationId { get; private set; }
    public Guid? RoomId { get; private set; }
    public Guid? ZoneId { get; private set; }
    public BatchStatus Status { get; private set; }
    
    private readonly List<BatchEvent> _events = new();
    public IReadOnlyCollection<BatchEvent> Events => _events.AsReadOnly();
    
    // Methods
    public void ChangeStage(BatchStage newStage, Guid userId, string notes = null);
    public void UpdateLocation(Guid? locationId, Guid? roomId, Guid? zoneId, Guid userId);
    public void UpdatePlantCount(int newCount, Guid userId, string reason);
    public void Split(int plantCount, BatchCode newBatchCode, string newBatchName, Guid userId, bool isPartialSplit = true);
    public void Merge(Batch otherBatch, Guid userId);
    public void Quarantine(string reason, Guid userId);
    public void ReleaseFromQuarantine(Guid userId);
    public void Harvest(DateOnly harvestDate, Guid userId);
    public void Destroy(string reason, Guid userId);
    public void AddEvent(EventType eventType, object eventData, Guid userId, string notes = null);
    public bool CanTransitionTo(BatchStage newStage);
    public bool CanSplit(int plantCountToSplit);
    public bool CanMerge(Batch otherBatch);
    public TimeSpan GetStageDuration();
    public List<Batch> GetLineage();
    public static BatchCode GenerateBatchCode(string prefix, string format, int sequence);
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
    public int? MaxPropagationCount { get; private set; }
    
    private readonly List<MotherHealthLog> _healthLogs = new();
    public IReadOnlyCollection<MotherHealthLog> HealthLogs => _healthLogs.AsReadOnly();
    
    // Methods
    public void RecordHealthLog(HealthStatus status, string observations, string treatments, 
        PestPressure pestPressure, DiseasePressure diseasePressure, string[] nutrientDeficiencies, 
        string environmentalNotes, string[] photoUrls, Guid userId);
    public void Propagate(Guid userId);
    public void Retire(string reason, Guid userId);
    public void Reactivate(Guid userId);
    public void UpdateLocation(Guid? locationId, Guid? roomId, Guid userId);
    public bool CanPropagate();
    public bool IsOverdueForHealthCheck(int reminderFrequencyDays);
    public HealthStatus GetCurrentHealthStatus();
    public TimeSpan GetAge();
    public DateOnly? GetNextHealthCheckDue(int reminderFrequencyDays);
}
```

---

### Phase 3: Application Layer (3-4 hours)

#### Application Services

**Files:**
```
src/backend/services/core-platform/genetics/Application/
├── Services/
│   ├── GeneticsManagementService.cs
│   ├── BatchLifecycleService.cs
│   └── MotherHealthService.cs
├── DTOs/
│   ├── CreateGeneticsRequest.cs
│   ├── CreateStrainRequest.cs
│   ├── CreateBatchRequest.cs
│   ├── BatchStageChangeRequest.cs
│   ├── BatchSplitRequest.cs
│   ├── BatchMergeRequest.cs
│   ├── MotherPlantHealthLogRequest.cs
│   ├── GeneticsResponse.cs
│   ├── StrainResponse.cs
│   ├── BatchResponse.cs
│   ├── BatchLineageResponse.cs
│   └── MotherPlantResponse.cs
└── Interfaces/
    ├── IGeneticsManagementService.cs
    ├── IBatchLifecycleService.cs
    └── IMotherHealthService.cs
```

**Key Service Methods:**

**IGeneticsManagementService:**
```csharp
public interface IGeneticsManagementService
{
    Task<GeneticsResponse> CreateGeneticsAsync(CreateGeneticsRequest request, CancellationToken ct);
    Task<PhenotypeResponse> CreatePhenotypeAsync(CreatePhenotypeRequest request, CancellationToken ct);
    Task<StrainResponse> CreateStrainAsync(CreateStrainRequest request, CancellationToken ct);
    Task<IReadOnlyList<GeneticsResponse>> GetGeneticsBySiteAsync(Guid siteId, CancellationToken ct);
    Task<IReadOnlyList<StrainResponse>> GetStrainsBySiteAsync(Guid siteId, CancellationToken ct);
    Task<StrainResponse> GetStrainByIdAsync(Guid strainId, CancellationToken ct);
    Task<bool> CanDeleteGeneticsAsync(Guid geneticsId, CancellationToken ct);
}
```

**IBatchLifecycleService:**
```csharp
public interface IBatchLifecycleService
{
    Task<BatchResponse> CreateBatchAsync(CreateBatchRequest request, CancellationToken ct);
    Task<BatchResponse> ChangeBatchStageAsync(Guid batchId, BatchStageChangeRequest request, CancellationToken ct);
    Task<BatchResponse> UpdateBatchLocationAsync(Guid batchId, UpdateBatchLocationRequest request, CancellationToken ct);
    Task<BatchResponse> UpdateBatchPlantCountAsync(Guid batchId, UpdatePlantCountRequest request, CancellationToken ct);
    Task<BatchResponse> SplitBatchAsync(Guid batchId, BatchSplitRequest request, CancellationToken ct);
    Task<BatchResponse> MergeBatchesAsync(BatchMergeRequest request, CancellationToken ct);
    Task<BatchResponse> QuarantineBatchAsync(Guid batchId, string reason, CancellationToken ct);
    Task<BatchResponse> HarvestBatchAsync(Guid batchId, DateOnly harvestDate, CancellationToken ct);
    Task<BatchLineageResponse> GetBatchLineageAsync(Guid batchId, CancellationToken ct);
    Task<IReadOnlyList<BatchResponse>> GetBatchesBySiteAsync(Guid siteId, BatchStatus? status, CancellationToken ct);
    Task<IReadOnlyList<BatchEventResponse>> GetBatchEventsAsync(Guid batchId, CancellationToken ct);
    Task<BatchCodeResponse> GenerateBatchCodeAsync(Guid siteId, CancellationToken ct);
    Task<BatchCodeSettingsResponse> GetBatchCodeSettingsAsync(Guid siteId, CancellationToken ct);
    Task<BatchCodeSettingsResponse> UpdateBatchCodeSettingsAsync(Guid siteId, UpdateBatchCodeSettingsRequest request, CancellationToken ct);
}
```

**IMotherHealthService:**
```csharp
public interface IMotherHealthService
{
    Task<MotherPlantResponse> CreateMotherPlantAsync(CreateMotherPlantRequest request, CancellationToken ct);
    Task<MotherPlantResponse> RecordHealthLogAsync(Guid motherPlantId, MotherPlantHealthLogRequest request, CancellationToken ct);
    Task<MotherPlantResponse> PropagateMotherPlantAsync(Guid motherPlantId, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<MotherPlantResponse>> GetMotherPlantsBySiteAsync(Guid siteId, MotherPlantStatus? status, CancellationToken ct);
    Task<IReadOnlyList<MotherHealthLogResponse>> GetHealthLogsAsync(Guid motherPlantId, CancellationToken ct);
    Task<MotherPlantHealthSummaryResponse> GetHealthSummaryAsync(Guid motherPlantId, CancellationToken ct);
    Task<IReadOnlyList<MotherPlantResponse>> GetOverdueForHealthCheckAsync(Guid siteId, CancellationToken ct);
    Task<MotherHealthReminderSettingsResponse> GetHealthReminderSettingsAsync(Guid siteId, CancellationToken ct);
    Task<MotherHealthReminderSettingsResponse> UpdateHealthReminderSettingsAsync(Guid siteId, UpdateHealthReminderSettingsRequest request, CancellationToken ct);
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
├── MotherPlantRepository.cs
└── MotherHealthLogRepository.cs
```

**Key Repository Methods:**

**IBatchRepository:**
```csharp
public interface IBatchRepository : IRepository<Batch, Guid>
{
    Task<Batch?> GetByBatchCodeAsync(string batchCode, Guid siteId, CancellationToken ct);
    Task<IReadOnlyList<Batch>> GetBySiteAndStatusAsync(Guid siteId, BatchStatus? status, CancellationToken ct);
    Task<IReadOnlyList<Batch>> GetByStrainAsync(Guid strainId, CancellationToken ct);
    Task<IReadOnlyList<Batch>> GetByCurrentStageAsync(BatchStage stage, CancellationToken ct);
    Task<IReadOnlyList<Batch>> GetByLocationAsync(Guid locationId, CancellationToken ct);
    Task<IReadOnlyList<Batch>> GetChildrenAsync(Guid parentBatchId, CancellationToken ct);
    Task<IReadOnlyList<Batch>> GetLineageAsync(Guid batchId, CancellationToken ct);
    Task UpdateStageAsync(Guid batchId, BatchStage newStage, DateTime stageStartedAt, CancellationToken ct);
    Task UpdateLocationAsync(Guid batchId, Guid? locationId, Guid? roomId, Guid? zoneId, CancellationToken ct);
    Task UpdatePlantCountAsync(Guid batchId, int newCount, CancellationToken ct);
}
```

**IMotherPlantRepository:**
```csharp
public interface IMotherPlantRepository : IRepository<MotherPlant, Guid>
{
    Task<MotherPlant?> GetByPlantIdAsync(string plantId, Guid siteId, CancellationToken ct);
    Task<IReadOnlyList<MotherPlant>> GetBySiteAndStatusAsync(Guid siteId, MotherPlantStatus? status, CancellationToken ct);
    Task<IReadOnlyList<MotherPlant>> GetByStrainAsync(Guid strainId, CancellationToken ct);
    Task<IReadOnlyList<MotherPlant>> GetByLocationAsync(Guid locationId, CancellationToken ct);
    Task UpdatePropagationCountAsync(Guid motherPlantId, int newCount, DateOnly lastPropagationDate, CancellationToken ct);
    Task<IReadOnlyList<MotherPlant>> GetOverdueForHealthCheckAsync(TimeSpan threshold, CancellationToken ct);
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
└── MotherPlantsController.cs
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
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BatchResponse>>> GetBatches(Guid siteId, [FromQuery] BatchStatus? status);
    
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
    
    [HttpPost("{batchId}/harvest")]
    public async Task<ActionResult<BatchResponse>> HarvestBatch(Guid siteId, Guid batchId, HarvestRequest request);
    
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
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MotherPlantResponse>>> GetMotherPlants(Guid siteId, [FromQuery] MotherPlantStatus? status);
    
    [HttpGet("{motherPlantId}")]
    public async Task<ActionResult<MotherPlantResponse>> GetMotherPlant(Guid siteId, Guid motherPlantId);
    
    [HttpPost("{motherPlantId}/health-log")]
    public async Task<ActionResult<MotherPlantResponse>> RecordHealthLog(Guid siteId, Guid motherPlantId, MotherPlantHealthLogRequest request);
    
    [HttpPost("{motherPlantId}/propagate")]
    public async Task<ActionResult<MotherPlantResponse>> Propagate(Guid siteId, Guid motherPlantId);
    
    [HttpGet("{motherPlantId}/health-logs")]
    public async Task<ActionResult<IReadOnlyList<MotherHealthLogResponse>>> GetHealthLogs(Guid siteId, Guid motherPlantId);
    
    [HttpGet("{motherPlantId}/health-summary")]
    public async Task<ActionResult<MotherPlantHealthSummaryResponse>> GetHealthSummary(Guid siteId, Guid motherPlantId);
}
```

---

### Phase 6: Validators (1 hour)

**Files:**
```
src/backend/services/core-platform/genetics/API/Validators/
├── CreateGeneticsRequestValidator.cs
├── CreateStrainRequestValidator.cs
├── CreateBatchRequestValidator.cs
├── BatchStageChangeRequestValidator.cs
├── BatchSplitRequestValidator.cs
├── BatchMergeRequestValidator.cs
└── MotherPlantHealthLogRequestValidator.cs
```

---

### Phase 7: Unit Tests (2-3 hours)

**Files:**
```
src/backend/services/core-platform/genetics/Tests/Unit/
├── Domain/
│   ├── GeneticsTests.cs
│   ├── StrainTests.cs
│   ├── BatchTests.cs
│   └── MotherPlantTests.cs
└── Services/
    ├── GeneticsManagementServiceTests.cs
    ├── BatchLifecycleServiceTests.cs
    └── MotherHealthServiceTests.cs
```

**Test Scenarios:**
- Genetics creation and validation
- Strain creation with genetics and phenotype
- Batch lifecycle state machine transitions
- Batch splitting and merging logic
- Mother plant health logging
- Lineage tracking and relationships
- RLS policy enforcement

---

### Phase 8: Integration Tests (2-3 hours)

**Files:**
```
src/backend/services/core-platform/genetics/Tests/Integration/
├── GeneticsManagementTests.cs
├── BatchLifecycleTests.cs
├── MotherPlantTests.cs
└── RlsGeneticsTests.cs
```

**Test Scenarios:**
- Create genetics → strain → batch (E2E)
- Batch stage transitions with validation
- Batch splitting creates proper relationships
- Mother plant health tracking
- RLS: Cross-site genetics access blocked
- Lineage queries return correct relationships
- Batch events are properly logged

---

## 📊 TASK BREAKDOWN WITH ESTIMATES

| Phase | Task | Est. Hours | Owner |
|-------|------|------------|-------|
| **1. Database** | Migration 1: Genetics tables | 1.5-2 | Backend |
| | Migration 2: Batch tables | 1.5-2 | Backend |
| **2. Domain** | 8 entity files | 2-2.5 | Backend |
| | 5 value object files | 0.5-1 | Backend |
| | 7 enum files | 0.5-1 | Backend |
| | Domain logic methods | 1-1.5 | Backend |
| **3. Application** | 3 service implementations | 1.5-2 | Backend |
| | 12 DTO files | 1-1.5 | Backend |
| | 3 interface files | 0.5 | Backend |
| **4. Infrastructure** | DbContext + 8 repositories | 2-2.5 | Backend |
| | RLS context integration | 0.5-1 | Backend |
| | Connection/retry logic | 0.5 | Backend |
| **5. API** | 4 controllers (~600 lines) | 2-2.5 | Backend |
| | Program.cs DI registration | 0.5 | Backend |
| **6. Validators** | 7 validator files | 1 | Backend |
| **7. Unit Tests** | 7 test files | 2-2.5 | Backend |
| **8. Integration Tests** | 4 test files | 2-2.5 | Backend |
| **TOTAL** | | **18-22** | |

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

1. **Batch Code Generation:** ✅ **Auto-generate with user-defined site/brand prefix** - Configurable per jurisdiction requirements
2. **Mother Plant Limits:** ✅ **User-configurable max propagation count** - Enforced when configured
3. **Health Log Frequency:** ✅ **Event-driven with user-configurable reminder frequency** - Flexible scheduling
4. **Lineage Depth:** ✅ **Unlimited with performance monitoring** - Track all generations with query optimization
5. **Batch Splitting:** ✅ **Both partial and complete splits with validation** - Flexible splitting with business rules

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

