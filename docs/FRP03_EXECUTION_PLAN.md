# FRP-03 Execution Plan - Genetics, Strains & Batches

**Date:** October 2, 2025  
**Status:** 🎯 Ready to Start  
**Approach:** Build vertically in 3 feature slices  
**Estimated Time:** 18-22 hours (accounts for shared prep, configuration, and validation work)

---

## 📊 Current State Summary

### ✅ PREREQUISITES COMPLETE

- ✅ **FRP-01 Complete** - Identity, RLS, ABAC foundation
- ✅ **FRP-02 Complete** - Spatial hierarchy and equipment registry
- ✅ **Database Infrastructure** - Supabase with RLS policies
- ✅ **API Infrastructure** - ASP.NET Core with established patterns
- ✅ **Test Infrastructure** - Integration test automation

### 🎯 TARGET DELIVERABLES

- 🎯 **Genetics Management** - Strain definitions, phenotypes, genetic profiles
- 🎯 **Batch Lifecycle** - From seed/clone to harvest with state tracking
- 🎯 **Mother Plant Registry** - Health logs, propagation tracking, genetic source
- 🎯 **Lineage Tracking** - Complete parent-child relationships for compliance

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

**Note:** Total estimate is 20 hours over 4 days (accounts for infrastructure setup, implementation, testing, and deployment wiring). This is conservative and includes breaks, context switching, and configuration tasks.

---

## 📋 THE 3 SLICES

```
SLICE 1: Genetics & Strains Management
├── Service: GeneticsManagementService
├── Repos: GeneticsRepository, PhenotypeRepository, StrainRepository
├── Controller: GeneticsController, StrainsController
├── Validators: GeneticsValidators, StrainValidators
└── Tests: Unit + Integration

SLICE 2: Batch Lifecycle Management
├── Service: BatchLifecycleService
├── Repos: BatchRepository, BatchEventRepository, BatchRelationshipRepository
├── Controller: BatchesController
├── Validators: BatchValidators
└── Tests: Unit + Integration

SLICE 3: Mother Plant Health Tracking
├── Service: MotherHealthService
├── Repos: MotherPlantRepository, MotherHealthLogRepository
├── Controller: MotherPlantsController
├── Validators: MotherPlantValidators
└── Tests: Unit + Integration
```

---

## 🧰 Pre-Slice Setup (90 min)

Complete these shared tasks before starting the feature slices:

1. **Domain Rehydration Helpers (45 min)**
   - Add static `FromPersistence(...)` factories (or internal constructors) to `Genetics`, `Phenotype`, `Strain`, `Batch`, `BatchEvent`, `BatchRelationship`, `MotherPlant`, and `MotherHealthLog` so repositories can materialize aggregates without reflection.
   - Keep persistence-specific guardrails inside the factory to centralize validation and audit stamping.

2. **DTO Mapping Profiles (20 min)**
   - Create an AutoMapper profile (or dedicated mapper class) under `Application/Mappers` to convert between domain entities and API DTOs.
   - Ensures controllers return DTOs rather than exposing domain types directly.

3. **Configuration & DI Checklist (25 min)**
   - Register `GeneticsDataSourceFactory`, services, repositories, validators, and mappers in the API `Program.cs`.
   - Wire up a dedicated `GENETICS_DB_CONNECTION` secret across `appsettings.*`, Kubernetes/Helm manifests, and CI secret templates.
   - Document the environment variable in `docs/infra/environment-variables.md` for operations.

---

## 🔧 SLICE 1: GENETICS & STRAINS MANAGEMENT

**Goal:** Complete CRUD for genetics, phenotypes, and strains  
**Time:** 6-7 hours (after shared pre-work)  
**Owner:** Core Platform/Genetics Squad

### Task 1.1: Create Folder Structure (5 min)

```bash
# Create directories
mkdir -p src/backend/services/core-platform/genetics/Application/Interfaces
mkdir -p src/backend/services/core-platform/genetics/Application/DTOs
mkdir -p src/backend/services/core-platform/genetics/Application/Services
mkdir -p src/backend/services/core-platform/genetics/Infrastructure/Persistence
mkdir -p src/backend/services/core-platform/genetics/API/Controllers
mkdir -p src/backend/services/core-platform/genetics/API/Validators
```

### Task 1.2: Create Service Interface (15 min)

**File:** `Application/Interfaces/IGeneticsManagementService.cs`

```csharp
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Service for managing genetics, phenotypes, and strains
/// </summary>
public interface IGeneticsManagementService
{
    // Genetics operations
    Task<Genetics> CreateGeneticsAsync(Guid siteId, CreateGeneticsRequest request, Guid userId, CancellationToken ct = default);
    Task<Genetics?> GetGeneticsByIdAsync(Guid siteId, Guid geneticsId, CancellationToken ct = default);
    Task<List<Genetics>> GetGeneticsBySiteAsync(Guid siteId, CancellationToken ct = default);
    Task UpdateGeneticsAsync(Guid siteId, Guid geneticsId, UpdateGeneticsRequest request, Guid userId, CancellationToken ct = default);
    Task DeleteGeneticsAsync(Guid siteId, Guid geneticsId, Guid userId, CancellationToken ct = default);

    // Phenotype operations
    Task<Phenotype> CreatePhenotypeAsync(Guid siteId, CreatePhenotypeRequest request, Guid userId, CancellationToken ct = default);
    Task<Phenotype?> GetPhenotypeByIdAsync(Guid siteId, Guid phenotypeId, CancellationToken ct = default);
    Task<List<Phenotype>> GetPhenotypesByGeneticsAsync(Guid geneticsId, CancellationToken ct = default);
    Task UpdatePhenotypeAsync(Guid siteId, Guid phenotypeId, UpdatePhenotypeRequest request, Guid userId, CancellationToken ct = default);
    Task DeletePhenotypeAsync(Guid siteId, Guid phenotypeId, Guid userId, CancellationToken ct = default);

    // Strain operations
    Task<Strain> CreateStrainAsync(Guid siteId, CreateStrainRequest request, Guid userId, CancellationToken ct = default);
    Task<Strain?> GetStrainByIdAsync(Guid siteId, Guid strainId, CancellationToken ct = default);
    Task<List<Strain>> GetStrainsBySiteAsync(Guid siteId, CancellationToken ct = default);
    Task<List<Strain>> GetStrainsByGeneticsAsync(Guid geneticsId, CancellationToken ct = default);
    Task UpdateStrainAsync(Guid siteId, Guid strainId, UpdateStrainRequest request, Guid userId, CancellationToken ct = default);
    Task DeleteStrainAsync(Guid siteId, Guid strainId, Guid userId, CancellationToken ct = default);
    
    // Validation helpers
    Task<bool> CanDeleteGeneticsAsync(Guid geneticsId, CancellationToken ct = default);
    Task<bool> CanDeletePhenotypeAsync(Guid phenotypeId, CancellationToken ct = default);
    Task<bool> CanDeleteStrainAsync(Guid strainId, CancellationToken ct = default);
}
```

### Task 1.3: Create DTOs (30 min)

**File:** `Application/DTOs/GeneticsDtos.cs`

```csharp
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Application.DTOs;

public record CreateGeneticsRequest(
    string Name,
    string Description,
    GeneticType GeneticType,
    decimal ThcMin,
    decimal ThcMax,
    decimal CbdMin,
    decimal CbdMax,
    int? FloweringTimeDays,
    YieldPotential YieldPotential,
    GeneticProfile GrowthCharacteristics,
    TerpeneProfile TerpeneProfile,
    string? BreedingNotes
);

public record UpdateGeneticsRequest(
    string Description,
    decimal ThcMin,
    decimal ThcMax,
    decimal CbdMin,
    decimal CbdMax,
    int? FloweringTimeDays,
    YieldPotential YieldPotential,
    GeneticProfile GrowthCharacteristics,
    TerpeneProfile TerpeneProfile,
    string? BreedingNotes
);

public record GeneticsResponse(
    Guid Id,
    Guid SiteId,
    string Name,
    string Description,
    GeneticType GeneticType,
    decimal ThcMin,
    decimal ThcMax,
    decimal CbdMin,
    decimal CbdMax,
    int? FloweringTimeDays,
    YieldPotential YieldPotential,
    GeneticProfile GrowthCharacteristics,
    TerpeneProfile TerpeneProfile,
    string? BreedingNotes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreatePhenotypeRequest(
    Guid GeneticsId,
    string Name,
    string Description,
    string? ExpressionNotes,
    VisualCharacteristics VisualCharacteristics,
    AromaProfile AromaProfile,
    GrowthPattern GrowthPattern
);

public record PhenotypeResponse(
    Guid Id,
    Guid SiteId,
    Guid GeneticsId,
    string Name,
    string Description,
    string? ExpressionNotes,
    VisualCharacteristics VisualCharacteristics,
    AromaProfile AromaProfile,
    GrowthPattern GrowthPattern,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateStrainRequest(
    Guid GeneticsId,
    Guid? PhenotypeId,
    string Name,
    string? Breeder,
    string? SeedBank,
    string Description,
    string? CultivationNotes,
    int? ExpectedHarvestWindowDays,
    TargetEnvironment TargetEnvironment,
    ComplianceRequirements ComplianceRequirements
);

public record StrainResponse(
    Guid Id,
    Guid SiteId,
    Guid GeneticsId,
    Guid? PhenotypeId,
    string Name,
    string? Breeder,
    string? SeedBank,
    string Description,
    string? CultivationNotes,
    int? ExpectedHarvestWindowDays,
    TargetEnvironment TargetEnvironment,
    ComplianceRequirements ComplianceRequirements,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
```

### Task 1.4: Implement Service (90 min)

**File:** `Application/Services/GeneticsManagementService.cs`

**Pattern to follow:** Look at `identity/Application/Services/BadgeAuthService.cs` for:

- Constructor DI pattern
- Validation approach
- Exception handling
- Async/await usage

**Key implementation notes:**

- Validate genetics name uniqueness (per site)
- Validate phenotype name uniqueness (per genetics)
- Validate strain name uniqueness (per site)
- Check for dependent strains before deleting genetics
- Check for dependent batches before deleting strains
- Use repository methods for data access
- Return domain entities via `FromPersistence` factories

**Estimated lines:** ~400

### Task 1.5: Create Repositories (120 min)

**File:** `Infrastructure/Persistence/GeneticsDataSourceFactory.cs`

**Copy from:** `identity/Infrastructure/Persistence/IdentityDataSourceFactory.cs`  
**Changes:**

- Rename class to `GeneticsDataSourceFactory`
- Read connection string from `GENETICS_DB_CONNECTION` (new secret managed alongside identity connection)

**Estimated lines:** ~150

---

**File:** `Infrastructure/Persistence/GeneticsRepository.cs`

```csharp
using Npgsql;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Infrastructure.Persistence;

public interface IGeneticsRepository
{
    Task<Genetics?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Genetics?> GetByNameAsync(Guid siteId, string name, CancellationToken ct = default);
    Task<List<Genetics>> GetBySiteAsync(Guid siteId, CancellationToken ct = default);
    Task<Guid> InsertAsync(Genetics genetics, CancellationToken ct = default);
    Task UpdateAsync(Genetics genetics, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasDependentStrainsAsync(Guid geneticsId, CancellationToken ct = default);
}

public class GeneticsRepository : IGeneticsRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<GeneticsRepository> _logger;
    
    public GeneticsRepository(NpgsqlDataSource dataSource, ILogger<GeneticsRepository> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<Genetics?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT id, site_id, name, description, genetic_type,
                   thc_percentage_range, cbd_percentage_range, flowering_time_days,
                   yield_potential, growth_characteristics, terpene_profile,
                   breeding_notes, created_at, updated_at, created_by, updated_by
            FROM genetics
            WHERE id = @id";
        
        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("id", id);
        
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;
        
        return MapGenetics(reader);
    }
    
    // ... implement other methods
    
    private Genetics MapGenetics(NpgsqlDataReader reader)
    {
        return Genetics.FromPersistence(
            id: reader.GetGuid(reader.GetOrdinal("id")),
            siteId: reader.GetGuid(reader.GetOrdinal("site_id")),
            name: reader.GetString(reader.GetOrdinal("name")),
            description: reader.GetString(reader.GetOrdinal("description")),
            geneticType: (GeneticType)reader.GetInt32(reader.GetOrdinal("genetic_type")),
            thcRange: ParseDecimalRange(reader.GetFieldValue<decimal[]>(reader.GetOrdinal("thc_percentage_range"))),
            cbdRange: ParseDecimalRange(reader.GetFieldValue<decimal[]>(reader.GetOrdinal("cbd_percentage_range"))),
            floweringTimeDays: reader.IsDBNull(reader.GetOrdinal("flowering_time_days")) ? null : reader.GetInt32(reader.GetOrdinal("flowering_time_days")),
            yieldPotential: (YieldPotential)reader.GetInt32(reader.GetOrdinal("yield_potential")),
            growthCharacteristics: JsonSerializer.Deserialize<GeneticProfile>(reader.GetString(reader.GetOrdinal("growth_characteristics")))!,
            terpeneProfile: JsonSerializer.Deserialize<TerpeneProfile>(reader.GetString(reader.GetOrdinal("terpene_profile")))!,
            breedingNotes: reader.IsDBNull(reader.GetOrdinal("breeding_notes")) ? null : reader.GetString(reader.GetOrdinal("breeding_notes")),
            createdAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
            createdBy: reader.GetGuid(reader.GetOrdinal("created_by")),
            updatedAt: reader.GetDateTime(reader.GetOrdinal("updated_at")),
            updatedBy: reader.GetGuid(reader.GetOrdinal("updated_by"))
        );
    }
    
    private (decimal Min, decimal Max) ParseDecimalRange(decimal[] range)
    {
        return range.Length == 2 ? (range[0], range[1]) : (0, 0);
    }
}
```

**Pattern to follow:** `identity/Infrastructure/Persistence/UserRepository.cs`

**Key implementation notes:**

- Set RLS context: `SET LOCAL app.current_user_id = '{userId}'`
- Use parameterized queries (SQL injection protection)
- Implement retry logic for transient errors
- Proper async/await with cancellation tokens
- Handle JSONB serialization for complex objects

**Estimated lines:** ~300

---

**File:** `Infrastructure/Persistence/PhenotypeRepository.cs` (~250 lines)
**File:** `Infrastructure/Persistence/StrainRepository.cs` (~300 lines)

### Task 1.6: Create Controllers (90 min)

**File:** `API/Controllers/GeneticsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.API.Controllers;

[ApiController]
[Route("api/sites/{siteId:guid}/genetics")]
[Produces("application/json")]
public class GeneticsController : ControllerBase
{
    private readonly IGeneticsManagementService _geneticsService;
    private readonly ILogger<GeneticsController> _logger;
    
    public GeneticsController(
        IGeneticsManagementService geneticsService,
        ILogger<GeneticsController> logger)
    {
        _geneticsService = geneticsService;
        _logger = logger;
    }
    
    /// <summary>
    /// Creates new genetics for a site
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GeneticsResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGenetics(
        Guid siteId,
        [FromBody] CreateGeneticsRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId(); // From claims
        var genetics = await _geneticsService.CreateGeneticsAsync(siteId, request, userId, ct);
        var response = GeneticsMapper.ToResponse(genetics);
        return CreatedAtAction(nameof(GetGenetics), new { siteId, geneticsId = response.Id }, response);
    }
    
    /// <summary>
    /// Gets genetics by ID
    /// </summary>
    [HttpGet("{geneticsId:guid}")]
    [ProducesResponseType(typeof(GeneticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGenetics(Guid siteId, Guid geneticsId, CancellationToken ct)
    {
        var genetics = await _geneticsService.GetGeneticsByIdAsync(siteId, geneticsId, ct);
        if (genetics == null)
            return NotFound();
        return Ok(GeneticsMapper.ToResponse(genetics));
    }
    
    // ... implement remaining endpoints
    
    private Guid GetCurrentUserId()
    {
        // Extract from JWT claims or context
        var userIdClaim = User.FindFirst("sub")?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}
```

**Endpoints to implement:**

- POST /api/sites/{siteId}/genetics
- GET /api/sites/{siteId}/genetics
- GET /api/sites/{siteId}/genetics/{geneticsId}
- PUT /api/sites/{siteId}/genetics/{geneticsId}
- DELETE /api/sites/{siteId}/genetics/{geneticsId}
- POST /api/sites/{siteId}/genetics/{geneticsId}/phenotypes
- GET /api/sites/{siteId}/genetics/{geneticsId}/phenotypes
- POST /api/sites/{siteId}/strains
- GET /api/sites/{siteId}/strains
- GET /api/sites/{siteId}/strains/{strainId}
- PUT /api/sites/{siteId}/strains/{strainId}
- DELETE /api/sites/{siteId}/strains/{strainId}

**Implementation notes:**

- Maintain site-scoped routes for multi-tenant clarity and guardrails
- Return DTOs using the mapper profile
- Register controller routes and Swagger annotations after wiring DI registrations

**Estimated lines:** ~300

### Task 1.7: Create Validators (45 min)

**File:** `API/Validators/GeneticsValidators.cs`

```csharp
using FluentValidation;
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.API.Validators;

public class CreateGeneticsRequestValidator : AbstractValidator<CreateGeneticsRequest>
{
    public CreateGeneticsRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[A-Za-z0-9\s\-_]+$")
            .WithMessage("Name must contain only letters, numbers, spaces, hyphens, and underscores");
        
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);
        
        RuleFor(x => x.ThcMin)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100);
        
        RuleFor(x => x.ThcMax)
            .GreaterThanOrEqualTo(x => x.ThcMin)
            .LessThanOrEqualTo(100);
        
        RuleFor(x => x.CbdMin)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(100);
        
        RuleFor(x => x.CbdMax)
            .GreaterThanOrEqualTo(x => x.CbdMin)
            .LessThanOrEqualTo(100);
        
        RuleFor(x => x.FloweringTimeDays)
            .GreaterThan(0)
            .LessThanOrEqualTo(365)
            .When(x => x.FloweringTimeDays.HasValue);
        
        RuleFor(x => x.GrowthCharacteristics)
            .NotNull()
            .WithMessage("Growth characteristics are required");
        
        RuleFor(x => x.TerpeneProfile)
            .NotNull()
            .WithMessage("Terpene profile is required");
    }
}

public class CreateStrainRequestValidator : AbstractValidator<CreateStrainRequest>
{
    public CreateStrainRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[A-Za-z0-9\s\-_]+$")
            .WithMessage("Name must contain only letters, numbers, spaces, hyphens, and underscores");
        
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);
        
        RuleFor(x => x.ExpectedHarvestWindowDays)
            .GreaterThan(0)
            .LessThanOrEqualTo(365)
            .When(x => x.ExpectedHarvestWindowDays.HasValue);
        
        RuleFor(x => x.TargetEnvironment)
            .NotNull()
            .WithMessage("Target environment is required");
        
        RuleFor(x => x.ComplianceRequirements)
            .NotNull()
            .WithMessage("Compliance requirements are required");
    }
}
```

**Pattern to follow:** `identity/API/Validators/UserRequestValidators.cs`

**Estimated lines:** ~200 total

### Task 1.8: Unit Tests (90 min)

**File:** `Tests/Unit/Domain/GeneticsTests.cs`

```csharp
using Xunit;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Tests.Unit.Domain;

public class GeneticsTests
{
    private readonly Guid _siteId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    
    [Fact]
    public void Constructor_ValidInputs_CreatesGenetics()
    {
        // Arrange & Act
        var genetics = new Genetics(
            _siteId, 
            "Blue Dream", 
            "Popular hybrid strain", 
            GeneticType.Hybrid, 
            (5.0m, 25.0m), 
            (0.1m, 2.0m), 
            65, 
            YieldPotential.High,
            new GeneticProfile(),
            new TerpeneProfile(),
            _userId
        );
        
        // Assert
        Assert.NotEqual(Guid.Empty, genetics.Id);
        Assert.Equal(_siteId, genetics.SiteId);
        Assert.Equal("Blue Dream", genetics.Name);
        Assert.Equal(GeneticType.Hybrid, genetics.GeneticType);
        Assert.Equal((5.0m, 25.0m), genetics.ThcPercentageRange);
    }
    
    [Fact]
    public void UpdateProfile_ValidInputs_UpdatesProperties()
    {
        // Arrange
        var genetics = CreateTestGenetics();
        var newCharacteristics = new GeneticProfile();
        var newTerpenes = new TerpeneProfile();
        
        // Act
        genetics.UpdateProfile("Updated description", newCharacteristics, newTerpenes);
        
        // Assert
        Assert.Equal("Updated description", genetics.Description);
        Assert.Equal(newCharacteristics, genetics.GrowthCharacteristics);
        Assert.Equal(newTerpenes, genetics.TerpeneProfile);
    }
    
    [Fact]
    public void UpdateCannabinoidRanges_InvalidRange_ThrowsException()
    {
        // Arrange
        var genetics = CreateTestGenetics();
        
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => 
            genetics.UpdateCannabinoidRanges(25.0m, 5.0m, 0.1m, 2.0m));
        Assert.Contains("THC max must be greater than min", ex.Message);
    }
    
    // ... more tests
    
    private Genetics CreateTestGenetics()
    {
        return new Genetics(
            _siteId, 
            "Test Genetics", 
            "Test description", 
            GeneticType.Hybrid, 
            (5.0m, 25.0m), 
            (0.1m, 2.0m), 
            65, 
            YieldPotential.High,
            new GeneticProfile(),
            new TerpeneProfile(),
            _userId
        );
    }
}
```

**Tests needed:**

- Constructor validation tests (empty params, invalid ranges)
- UpdateProfile tests
- UpdateCannabinoidRanges tests (valid/invalid ranges)
- UpdateFloweringTime tests
- CanDelete tests (with/without dependent strains)

**Estimated lines:** ~250

---

**File:** `Tests/Unit/Domain/StrainTests.cs` (~200 lines)
**File:** `Tests/Unit/Services/GeneticsManagementServiceTests.cs` (~300 lines)

### Task 1.9: Integration Tests (90 min)

**File:** `Tests/Integration/GeneticsManagementIntegrationTests.cs`

```csharp
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Tests.Integration;

[Collection("Integration")]
public class GeneticsManagementIntegrationTests : IntegrationTestBase
{
    private readonly IGeneticsManagementService _geneticsService;
    
    public GeneticsManagementIntegrationTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _geneticsService = ServiceProvider.GetRequiredService<IGeneticsManagementService>();
    }
    
    [Fact]
    public async Task CreateGenetics_ValidRequest_ReturnsGeneticsWithId()
    {
        // Arrange
        var request = new CreateGeneticsRequest(
            Name: "Blue Dream",
            Description: "Popular hybrid strain with balanced effects",
            GeneticType: GeneticType.Hybrid,
            ThcMin: 15.0m,
            ThcMax: 25.0m,
            CbdMin: 0.1m,
            CbdMax: 2.0m,
            FloweringTimeDays: 65,
            YieldPotential: YieldPotential.High,
            GrowthCharacteristics: new GeneticProfile(),
            TerpeneProfile: new TerpeneProfile(),
            BreedingNotes: "Stable hybrid with consistent characteristics"
        );
        
        // Act
        var genetics = await _geneticsService.CreateGeneticsAsync(TestSiteId, request, TestUserId);
        
        // Assert
        Assert.NotEqual(Guid.Empty, genetics.Id);
        Assert.Equal("Blue Dream", genetics.Name);
        Assert.Equal(TestSiteId, genetics.SiteId);
        Assert.Equal(GeneticType.Hybrid, genetics.GeneticType);
    }
    
    [Fact]
    public async Task CreateStrain_WithGenetics_ReturnsStrainWithId()
    {
        // Arrange - Create genetics first
        var genetics = await CreateTestGeneticsAsync();
        
        var request = new CreateStrainRequest(
            GeneticsId: genetics.Id,
            PhenotypeId: null,
            Name: "Blue Dream #1",
            Breeder: "Humboldt Seed Company",
            SeedBank: "Local Dispensary",
            Description: "Premium Blue Dream phenotype",
            CultivationNotes: "Prefers moderate humidity",
            ExpectedHarvestWindowDays: 65,
            TargetEnvironment: new TargetEnvironment(),
            ComplianceRequirements: new ComplianceRequirements()
        );
        
        // Act
        var strain = await _geneticsService.CreateStrainAsync(TestSiteId, request, TestUserId);
        
        // Assert
        Assert.NotEqual(Guid.Empty, strain.Id);
        Assert.Equal("Blue Dream #1", strain.Name);
        Assert.Equal(genetics.Id, strain.GeneticsId);
        Assert.Equal(TestSiteId, strain.SiteId);
    }
    
    [Fact]
    public async Task DeleteGenetics_WithDependentStrains_ThrowsException()
    {
        // Arrange - Create genetics and dependent strain
        var genetics = await CreateTestGeneticsAsync();
        var strain = await CreateTestStrainAsync(genetics.Id);
        
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _geneticsService.DeleteGeneticsAsync(TestSiteId, genetics.Id, TestUserId));
        Assert.Contains("Cannot delete genetics with dependent strains", ex.Message);
    }
    
    // ... more tests
    
    private async Task<Genetics> CreateTestGeneticsAsync()
    {
        var request = new CreateGeneticsRequest(
            Name: "Test Genetics",
            Description: "Test description",
            GeneticType: GeneticType.Hybrid,
            ThcMin: 10.0m,
            ThcMax: 20.0m,
            CbdMin: 0.5m,
            CbdMax: 1.5m,
            FloweringTimeDays: 60,
            YieldPotential: YieldPotential.Medium,
            GrowthCharacteristics: new GeneticProfile(),
            TerpeneProfile: new TerpeneProfile(),
            BreedingNotes: null
        );
        
        return await _geneticsService.CreateGeneticsAsync(TestSiteId, request, TestUserId);
    }
    
    private async Task<Strain> CreateTestStrainAsync(Guid geneticsId)
    {
        var request = new CreateStrainRequest(
            GeneticsId: geneticsId,
            PhenotypeId: null,
            Name: "Test Strain",
            Breeder: "Test Breeder",
            SeedBank: "Test Bank",
            Description: "Test strain description",
            CultivationNotes: null,
            ExpectedHarvestWindowDays: 60,
            TargetEnvironment: new TargetEnvironment(),
            ComplianceRequirements: new ComplianceRequirements()
        );
        
        return await _geneticsService.CreateStrainAsync(TestSiteId, request, TestUserId);
    }
}
```

**Tests needed:**

- CRUD operations for genetics
- CRUD operations for phenotypes
- CRUD operations for strains
- Dependency validation (cannot delete genetics with strains)
- RLS verification (cross-site access blocked)

**Estimated lines:** ~350

---

**File:** `Tests/Integration/RlsGeneticsTests.cs` (~150 lines)

### Slice 1 Summary

**Total Time:** 6-7 hours  
**Total Files:** 15 files  
**Total Lines:** ~2,800 lines

**Deliverables:**

- ✅ Service interface + implementation
- ✅ 3 repositories with RLS
- ✅ 2 controllers with 12 endpoints
- ✅ 3 validator files
- ✅ 5 test files (unit + integration)

---

## 🔧 SLICE 2: BATCH LIFECYCLE MANAGEMENT

**Goal:** Complete batch lifecycle with state machine and event tracking  
**Time:** 7-8 hours  
**Dependencies:** Slice 1 (Strains must exist)

### Quick Overview (Detailed steps similar to Slice 1)

**Service:** `BatchLifecycleService.cs` (~500 lines)

- CreateBatch, GetBatch, UpdateBatch
- ChangeStage, UpdateLocation, UpdatePlantCount
- SplitBatch, MergeBatches, QuarantineBatch
- HarvestBatch, DestroyBatch, GetLineage
- AddEvent, GetEvents

**Repositories:** `BatchRepository.cs` (~400 lines), `BatchEventRepository.cs` (~200 lines), `BatchRelationshipRepository.cs` (~250 lines)

- Standard CRUD with RLS
- State machine validation queries
- Lineage relationship queries
- Event history queries

**Controller:** `BatchesController.cs` (~400 lines)

- POST /api/sites/{siteId}/batches
- GET /api/sites/{siteId}/batches (with filters)
- GET /api/sites/{siteId}/batches/{batchId}
- POST /api/sites/{siteId}/batches/{batchId}/stage-change
- POST /api/sites/{siteId}/batches/{batchId}/split
- POST /api/sites/{siteId}/batches/merge
- POST /api/sites/{siteId}/batches/{batchId}/quarantine
- POST /api/sites/{siteId}/batches/{batchId}/harvest
- GET /api/sites/{siteId}/batches/{batchId}/lineage
- GET /api/sites/{siteId}/batches/{batchId}/events

**Validators:** `BatchValidators.cs` (~200 lines)

**Tests:** 4 files (~600 lines total)

- BatchTests.cs (unit - state machine)
- BatchLifecycleServiceTests.cs (unit)
- BatchLifecycleIntegrationTests.cs (integration)
- RlsBatchTests.cs (integration - RLS)

---

## 🔧 SLICE 3: MOTHER PLANT HEALTH TRACKING

**Goal:** Mother plant registry with health logging  
**Time:** 4-5 hours  
**Dependencies:** Slice 2 (Batches must exist)

### Quick Overview

**Service:** `MotherHealthService.cs` (~300 lines)

- CreateMotherPlant, GetMotherPlant, UpdateMotherPlant
- RecordHealthLog, GetHealthLogs, GetHealthSummary
- PropagateMotherPlant, RetireMotherPlant, ReactivateMotherPlant
- UpdateLocation, GetOverdueForHealthCheck

**Repositories:** `MotherPlantRepository.cs` (~300 lines), `MotherHealthLogRepository.cs` (~200 lines)

- Standard CRUD with RLS
- Health log queries (by date range, by status)
- Propagation tracking queries
- Overdue health check queries

**Controller:** `MotherPlantsController.cs` (~250 lines)

- POST /api/sites/{siteId}/mother-plants
- GET /api/sites/{siteId}/mother-plants (with filters)
- GET /api/sites/{siteId}/mother-plants/{motherPlantId}
- POST /api/sites/{siteId}/mother-plants/{motherPlantId}/health-log
- POST /api/sites/{siteId}/mother-plants/{motherPlantId}/propagate
- GET /api/sites/{siteId}/mother-plants/{motherPlantId}/health-logs
- GET /api/sites/{siteId}/mother-plants/{motherPlantId}/health-summary

**Validators:** `MotherPlantValidators.cs` (~150 lines)

**Tests:** 3 files (~400 lines total)

- MotherPlantTests.cs (unit - health tracking)
- MotherHealthServiceTests.cs (unit)
- MotherPlantIntegrationTests.cs (integration)

---

## 📅 RECOMMENDED TIMELINE

### Day 1 (6 hours) - Foundation + Slice 1 Part 1

- Morning (2h): Pre-slice setup (rehydration factories, mapper profile, configuration checklist)
- Midday (2h): Tasks 1.1-1.3 (folder structure, service interface, DTOs)
- Afternoon (2h): Begin Task 1.4 (service implementation)

### Day 2 (6 hours) - Slice 1 Part 2

- Morning (3h): Complete Task 1.4 and build `GeneticsDataSourceFactory`
- Afternoon (3h): GeneticsRepository + PhenotypeRepository + StrainRepository (Task 1.5)

### Day 3 (5 hours) - Slice 1 Part 3 + Slice 2 Start

- Morning (2h): Task 1.6 (controllers with explicit routing + DTO responses)
- Midday (1h): Task 1.7 (validators)
- Afternoon (2h): Tasks 1.8-1.9 (unit + integration tests)
- Evening (1h): Begin Slice 2 (BatchLifecycleService interface + DTOs)

### Day 4 (6 hours) - Slice 2 Complete

- Morning (3h): BatchLifecycleService + repositories (with mapper + rehydration helpers)
- Afternoon (3h): BatchesController + validators + tests

### Day 5 (4 hours) - Slice 3 Complete

- Morning (2.5h): MotherHealthService + repositories + controller
- Afternoon (1.5h): Validators + tests + final integration

**Total:** 27 hours over 5 days (includes pre-slice setup, implementation, testing, and configuration/DI wiring)

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
- ✅ Program.cs, appsettings, and deployment secrets updated/validated for new components

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

### Check Coverage

```bash
# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageReporter=html
```

### Start API locally

```bash
cd src/backend/services/core-platform/genetics/API
dotnet run
```

---

## 📋 Progress Tracking

After each task, update:

1. `docs/TRACK_B_COMPLETION_CHECKLIST.md` - Mark checkboxes
2. `docs/FRP03_CURRENT_STATUS.md` - Update % complete
3. Git commit with meaningful message

Example commit messages:

```
feat(frp03): implement genetics management service
feat(frp03): add genetics and strain repositories with RLS
feat(frp03): add genetics controller with 12 endpoints
test(frp03): add integration tests for genetics management
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
7. ✅ Follows FRP-01/FRP-02 patterns

---

## 🎯 Success Metrics

FRP-03 is successfully complete when:

- ✅ All 3 slices delivered and tested
- ✅ 21/21 checklist items marked complete
- ✅ All acceptance criteria met
- ✅ Performance targets met (p95 < 200ms)
- ✅ Documentation updated
- ✅ Ready for FRP-07

**Note on estimates:** The 20-hour total (spread over 4 days) accounts for all work including setup, coding, testing, breaks, and deployment configuration. Pure execution time (focused coding without setup/breaks) is estimated at 14-16 hours.

---

**Ready to start?** Begin with **Slice 1, Task 1.1** (Folder Structure) 🚀

