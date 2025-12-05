using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Xunit;
using Xunit.Sdk;

namespace Harvestry.Genetics.Tests.Integration;

/// <summary>
/// End-to-End tests for complete Genetics workflows using real database
/// </summary>
public sealed class GeneticsE2ETests : IClassFixture<GeneticsE2EFactory>, IAsyncLifetime
{
    private readonly GeneticsE2EFactory _factory;
    private HttpClient _client = null!;
    private readonly Guid _testSiteId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
    private readonly Guid _testUserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

    public GeneticsE2ETests(GeneticsE2EFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();

        Console.WriteLine($"[E2E] Container connection string: {_factory.ConnectionString}");
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("GeneticsDb");
        Console.WriteLine($"[E2E] Config connection string: {connectionString}");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private HttpRequestMessage CreateAuthedRequest(HttpMethod method, string uri)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add("X-User-Id", _testUserId.ToString());
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", _testSiteId.ToString());
        return request;
    }

    private string GenerateUniqueBatchCode()
    {
        return $"TEST-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4]}";
    }

    [Fact]
    public async Task StrainToBatchWorkflow_EndToEnd_Succeeds()
    {
        // Phase 1: Get existing genetics
        var getGeneticsRequest = CreateAuthedRequest(HttpMethod.Get, $"/api/sites/{_testSiteId}/genetics");
        var geneticsResponse = await _client.SendAsync(getGeneticsRequest);
        await AssertStatusCodeAsync(geneticsResponse, HttpStatusCode.OK, "GET /api/sites/{siteId}/genetics");

        var geneticsList = await geneticsResponse.Content.ReadFromJsonAsync<GeneticsResponse[]>();
        geneticsList.Should().NotBeNull();
        geneticsList.Should().HaveCountGreaterThan(0);

        var genetics = geneticsList![0];

        // Phase 2: Create a new strain from the genetics
        var createStrainRequest = new CreateStrainRequest(
            GeneticsId: genetics.Id,
            PhenotypeId: null,
            Name: $"Test Strain {Guid.NewGuid():N}",
            Description: "Test strain for E2E workflow",
            Breeder: "Test Breeder",
            SeedBank: "Test Bank",
            CultivationNotes: "Grown in optimal conditions",
            ExpectedHarvestWindowDays: 60,
            TargetEnvironment: new TargetEnvironment(
                GrowMedium: "Soil",
                LightingType: "LED",
                PhotoperiodHours: 18,
                TargetTemperatureDayF: 75,
                TargetTemperatureNightF: 68,
                TargetHumidityVeg: 60,
                TargetHumidityFlower: 40,
                Co2Supplementation: "None",
                IrrigationMethod: "Drip",
                NutrientRegime: "Organic",
                AdditionalParameters: null),
            ComplianceRequirements: new ComplianceRequirements(
                RequiredLabTests: new[] { "Potency", "Contaminants" },
                ProhibitedStageMethods: null,
                MandatoryTrackingEvents: new[] { "Daily", "Stage Change" },
                MinimumTrackingDays: 30,
                MaximumPlantCount: null,
                LabelingRequirements: null,
                RequiresMetrcTagging: true,
                RequiresBatchPhotography: false,
                JurisdictionSpecificRules: null));

        var createStrainHttpRequest = CreateAuthedRequest(HttpMethod.Post, $"/api/sites/{_testSiteId}/strains");
        createStrainHttpRequest.Content = JsonContent.Create(createStrainRequest);

        var createStrainResponse = await _client.SendAsync(createStrainHttpRequest);
        await AssertStatusCodeAsync(createStrainResponse, HttpStatusCode.Created, "POST /api/sites/{siteId}/strains");

        var createdStrain = await createStrainResponse.Content.ReadFromJsonAsync<StrainResponse>();
        createdStrain.Should().NotBeNull();
        createdStrain!.Name.Should().Be(createStrainRequest.Name);
        createdStrain.GeneticsId.Should().Be(genetics.Id);

        // Phase 3: Create a batch from the strain (need to provide all required fields)
        var batchCode = GenerateUniqueBatchCode();
        var createBatchRequest = new CreateBatchRequest(
            StrainId: createdStrain.Id,
            BatchCode: batchCode,
            BatchName: $"Test Batch {Guid.NewGuid():N}",
            BatchType: BatchType.Seed,
            SourceType: BatchSourceType.Propagation,
            PlantCount: 100,
            CurrentStageId: Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // propagation stage
            TargetPlantCount: 100,
            Notes: "Test batch for E2E workflow");

        var createBatchHttpRequest = CreateAuthedRequest(HttpMethod.Post, "/api/genetics/batches");
        createBatchHttpRequest.Content = JsonContent.Create(createBatchRequest);

        var createBatchResponse = await _client.SendAsync(createBatchHttpRequest);
        await AssertStatusCodeAsync(createBatchResponse, HttpStatusCode.Created, "POST /api/genetics/batches");

        var createdBatch = await createBatchResponse.Content.ReadFromJsonAsync<BatchResponse>();
        createdBatch.Should().NotBeNull();
        createdBatch!.StrainId.Should().Be(createdStrain.Id);
        createdBatch.PlantCount.Should().Be(createBatchRequest.PlantCount);
        createdBatch.Status.Should().Be(BatchStatus.Active);
        createdBatch.BatchCode.Should().BeEquivalentTo(batchCode);

        // Phase 4: Get active batches and verify our batch is there
        var getActiveBatchesRequest = CreateAuthedRequest(HttpMethod.Get, "/api/genetics/batches/active");
        var getActiveBatchesResponse = await _client.SendAsync(getActiveBatchesRequest);
        await AssertStatusCodeAsync(getActiveBatchesResponse, HttpStatusCode.OK, "GET /api/genetics/batches/active");

        var activeBatches = await getActiveBatchesResponse.Content.ReadFromJsonAsync<BatchResponse[]>();
        activeBatches.Should().NotBeNull();
        activeBatches.Should().Contain(b => b.Id == createdBatch.Id);

        // Phase 5: Get batch by ID
        var getBatchRequest = CreateAuthedRequest(HttpMethod.Get, $"/api/genetics/batches/{createdBatch.Id}");
        var getBatchResponse = await _client.SendAsync(getBatchRequest);
        await AssertStatusCodeAsync(getBatchResponse, HttpStatusCode.OK, "GET /api/genetics/batches/{id}");

        var retrievedBatch = await getBatchResponse.Content.ReadFromJsonAsync<BatchResponse>();
        retrievedBatch.Should().BeEquivalentTo(createdBatch);

        // Phase 6: Transition batch to next stage (vegetative)
        var transitionRequest = new TransitionBatchStageRequest(
            NewStageId: Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), // vegetative stage
            TransitionNotes: "Moving to vegetative stage");

        var transitionHttpRequest = CreateAuthedRequest(HttpMethod.Post, $"/api/genetics/batches/{createdBatch.Id}/transition");
        transitionHttpRequest.Content = JsonContent.Create(transitionRequest);

        var transitionResponse = await _client.SendAsync(transitionHttpRequest);
        await AssertStatusCodeAsync(transitionResponse, HttpStatusCode.OK, "POST /api/genetics/batches/{id}/transition");

        var transitionedBatch = await transitionResponse.Content.ReadFromJsonAsync<BatchResponse>();
        transitionedBatch.Should().NotBeNull();
        transitionedBatch!.Id.Should().Be(createdBatch.Id);
        transitionedBatch.Status.Should().Be(BatchStatus.Active);
        transitionedBatch.CurrentStageId.Should().Be(transitionRequest.NewStageId);

        // Phase 7: Terminate batch
        var terminateRequest = new TerminateBatchRequest(Reason: "Test termination");

        var terminateHttpRequest = CreateAuthedRequest(HttpMethod.Post, $"/api/genetics/batches/{createdBatch.Id}/terminate");
        terminateHttpRequest.Content = JsonContent.Create(terminateRequest);

        var terminateResponse = await _client.SendAsync(terminateHttpRequest);
        await AssertStatusCodeAsync(terminateResponse, HttpStatusCode.OK, "POST /api/genetics/batches/{id}/terminate");

        var terminatedBatch = await terminateResponse.Content.ReadFromJsonAsync<BatchResponse>();
        terminatedBatch.Should().NotBeNull();
        terminatedBatch!.Id.Should().Be(createdBatch.Id);
        terminatedBatch.Status.Should().Be(BatchStatus.Destroyed);
    }


    [Fact]
    public async Task BatchCodeGeneration_UniqueCodesGenerated_Manually()
    {
        // Get existing strain
        var getStrainsRequest = CreateAuthedRequest(HttpMethod.Get, $"/api/sites/{_testSiteId}/strains");
        var strainsResponse = await _client.SendAsync(getStrainsRequest);
        var strains = await strainsResponse.Content.ReadFromJsonAsync<StrainResponse[]>();
        var strain = strains![0];

        // Create multiple batches with manually generated unique codes
        var batchCodes = new System.Collections.Generic.List<string>();

        for (int i = 0; i < 3; i++)
        {
            var batchCode = GenerateUniqueBatchCode();
            var createBatchRequest = new CreateBatchRequest(
                StrainId: strain.Id,
                BatchCode: batchCode,
                BatchName: $"Code Test Batch {i}",
                BatchType: BatchType.Seed,
                SourceType: BatchSourceType.Propagation,
                PlantCount: 50,
                CurrentStageId: Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // propagation stage
                TargetPlantCount: 50,
                Notes: $"Batch {i} for code generation testing");

            var createBatchHttpRequest = CreateAuthedRequest(HttpMethod.Post, "/api/genetics/batches");
            createBatchHttpRequest.Content = JsonContent.Create(createBatchRequest);

            var createBatchResponse = await _client.SendAsync(createBatchHttpRequest);
            await AssertStatusCodeAsync(createBatchResponse, HttpStatusCode.Created, "POST /api/genetics/batches (code generation)");

            var createdBatch = await createBatchResponse.Content.ReadFromJsonAsync<BatchResponse>();
            createdBatch.Should().NotBeNull();

            // Verify code matches what we provided
            createdBatch!.BatchCode.Should().BeEquivalentTo(batchCode);
            batchCodes.Add(createdBatch.BatchCode);
        }

        // Verify all codes are unique
        batchCodes.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task BatchLifecycle_CompleteWorkflow_TracksHistory()
    {
        // Get existing strain
        var getStrainsRequest = CreateAuthedRequest(HttpMethod.Get, $"/api/sites/{_testSiteId}/strains");
        var strainsResponse = await _client.SendAsync(getStrainsRequest);
        var strains = await strainsResponse.Content.ReadFromJsonAsync<StrainResponse[]>();
        var strain = strains![0];

        // Create batch
        var batchCode = GenerateUniqueBatchCode();
        var createBatchRequest = new CreateBatchRequest(
            StrainId: strain.Id,
            BatchCode: batchCode,
            BatchName: "Lifecycle Test Batch",
            BatchType: BatchType.Seed,
            SourceType: BatchSourceType.Propagation,
            PlantCount: 25,
            CurrentStageId: Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), // propagation stage
            TargetPlantCount: 25,
            Notes: "Testing complete lifecycle");

        var createBatchHttpRequest = CreateAuthedRequest(HttpMethod.Post, "/api/genetics/batches");
        createBatchHttpRequest.Content = JsonContent.Create(createBatchRequest);

        var createBatchResponse = await _client.SendAsync(createBatchHttpRequest);
        await AssertStatusCodeAsync(createBatchResponse, HttpStatusCode.Created, "POST /api/genetics/batches");
        var createdBatch = await createBatchResponse.Content.ReadFromJsonAsync<BatchResponse>();

        // Get batch events (should have creation event)
        var getEventsRequest = CreateAuthedRequest(HttpMethod.Get, $"/api/genetics/batches/{createdBatch!.Id}/events");
        var getEventsResponse = await _client.SendAsync(getEventsRequest);
        await AssertStatusCodeAsync(getEventsResponse, HttpStatusCode.OK, "GET /api/genetics/batches/{id}/events");

        var events = await getEventsResponse.Content.ReadFromJsonAsync<BatchEventResponse[]>();
        events.Should().NotBeNull();
        events.Should().HaveCountGreaterThan(0);
        events.Should().Contain(e => e.EventType == EventType.Created);

        // Transition through stages
        var vegetativeStageId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002");
        var transitionRequest = new TransitionBatchStageRequest(
            NewStageId: vegetativeStageId,
            TransitionNotes: "Starting vegetative growth");

        var transitionHttpRequest = CreateAuthedRequest(HttpMethod.Post, $"/api/genetics/batches/{createdBatch.Id}/transition");
        transitionHttpRequest.Content = JsonContent.Create(transitionRequest);

        var transitionResponse = await _client.SendAsync(transitionHttpRequest);
        await AssertStatusCodeAsync(transitionResponse, HttpStatusCode.OK, "POST /api/genetics/batches/{id}/transition");

        // Check events again (should have transition event)
        var getUpdatedEventsRequest = CreateAuthedRequest(HttpMethod.Get, $"/api/genetics/batches/{createdBatch.Id}/events");
        var getUpdatedEventsResponse = await _client.SendAsync(getUpdatedEventsRequest);
        await AssertStatusCodeAsync(getUpdatedEventsResponse, HttpStatusCode.OK, "GET /api/genetics/batches/{id}/events (after transition)");
        var updatedEvents = await getUpdatedEventsResponse.Content.ReadFromJsonAsync<BatchEventResponse[]>();

        updatedEvents.Should().Contain(e => e.EventType == EventType.StageChange);
        updatedEvents.Should().Contain(e => e.EventType == EventType.Created);
    }

    [Fact]
    public async Task MotherPlantLifecycle_Workflow_Succeeds()
    {
        // Lookup base strain and create supporting batch
        var getStrainsRequest = CreateAuthedRequest(HttpMethod.Get, $"/api/sites/{_testSiteId}/strains");
        var strainsResponse = await _client.SendAsync(getStrainsRequest);
        await AssertStatusCodeAsync(strainsResponse, HttpStatusCode.OK, "GET /api/sites/{siteId}/strains (mother workflow)");
        var strains = await strainsResponse.Content.ReadFromJsonAsync<StrainResponse[]>() ?? Array.Empty<StrainResponse>();
        strains.Should().NotBeEmpty();
        var strain = strains[0];

        var batchCode = GenerateUniqueBatchCode();
        var createBatchRequest = new CreateBatchRequest(
            StrainId: strain.Id,
            BatchCode: batchCode,
            BatchName: "Mother Seed Batch",
            BatchType: BatchType.MotherPlant,
            SourceType: BatchSourceType.Propagation,
            PlantCount: 15,
            CurrentStageId: Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
            TargetPlantCount: 15,
            Notes: "Batch supporting mother plant workflow");

        var batchRequest = CreateAuthedRequest(HttpMethod.Post, "/api/genetics/batches");
        batchRequest.Content = JsonContent.Create(createBatchRequest);
        var batchResponse = await _client.SendAsync(batchRequest);
        await AssertStatusCodeAsync(batchResponse, HttpStatusCode.Created, "POST /api/genetics/batches (mother workflow)");
        var supportingBatch = await batchResponse.Content.ReadFromJsonAsync<BatchResponse>();
        supportingBatch.Should().NotBeNull();

        var createMotherRequest = new CreateMotherPlantRequest(
            BatchId: supportingBatch!.Id,
            StrainId: supportingBatch.StrainId,
            PlantTag: $"MP-{Guid.NewGuid():N}",
            DateEstablished: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-45)),
            LocationId: null,
            RoomId: null,
            MaxPropagationCount: 25,
            Notes: "Integration mother plant",
            Metadata: new Dictionary<string, object> { { "type", "integration" } });

        var createMotherHttp = CreateAuthedRequest(HttpMethod.Post, "/api/genetics/mother-plants");
        createMotherHttp.Content = JsonContent.Create(createMotherRequest);
        var createMotherResponse = await _client.SendAsync(createMotherHttp);
        await AssertStatusCodeAsync(createMotherResponse, HttpStatusCode.Created, "POST /api/genetics/mother-plants");
        var createdMother = await createMotherResponse.Content.ReadFromJsonAsync<MotherPlantResponse>();
        createdMother.Should().NotBeNull();
        createdMother!.PlantTag.Should().BeEquivalentTo(createMotherRequest.PlantTag);

        var healthLogRequest = new MotherPlantHealthLogRequest(
            LogDate: DateOnly.FromDateTime(DateTime.UtcNow),
            Status: HealthStatus.Excellent,
            PestPressure: PressureLevel.None,
            DiseasePressure: PressureLevel.None,
            NutrientDeficiencies: Array.Empty<string>(),
            Observations: "Strong growth",
            TreatmentsApplied: null,
            EnvironmentalNotes: null,
            PhotoUrls: Array.Empty<string>());

        var logHttp = CreateAuthedRequest(HttpMethod.Post, $"/api/genetics/mother-plants/{createdMother.Id}/health-logs");
        logHttp.Content = JsonContent.Create(healthLogRequest);
        var logResponse = await _client.SendAsync(logHttp);
        await AssertStatusCodeAsync(logResponse, HttpStatusCode.OK, "POST /api/genetics/mother-plants/{id}/health-logs");

        var propagationRequest = new RegisterPropagationRequest(PropagatedCount: 5);
        var propagationHttp = CreateAuthedRequest(HttpMethod.Post, $"/api/genetics/mother-plants/{createdMother.Id}/propagation");
        propagationHttp.Content = JsonContent.Create(propagationRequest);
        var propagationResponse = await _client.SendAsync(propagationHttp);
        await AssertStatusCodeAsync(propagationResponse, HttpStatusCode.OK, "POST /api/genetics/mother-plants/{id}/propagation");
        var propagatedMother = await propagationResponse.Content.ReadFromJsonAsync<MotherPlantResponse>();
        propagatedMother.Should().NotBeNull();
        propagatedMother!.PropagationCount.Should().BeGreaterThanOrEqualTo(5);

        var summaryHttp = CreateAuthedRequest(HttpMethod.Get, $"/api/genetics/mother-plants/{createdMother.Id}/health-summary");
        var summaryResponse = await _client.SendAsync(summaryHttp);
        await AssertStatusCodeAsync(summaryResponse, HttpStatusCode.OK, "GET /api/genetics/mother-plants/{id}/health-summary");
        var summary = await summaryResponse.Content.ReadFromJsonAsync<MotherPlantHealthSummaryResponse>();
        summary.Should().NotBeNull();
        summary!.MotherPlant.Id.Should().Be(createdMother.Id);
        summary.MotherPlant.PropagationCount.Should().Be(propagatedMother.PropagationCount);
    }

    private static async Task AssertStatusCodeAsync(HttpResponseMessage response, HttpStatusCode expectedStatus, string scenario)
    {
        if (response.StatusCode != expectedStatus)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new XunitException($"{scenario} expected {(int)expectedStatus} ({expectedStatus}) but received {(int)response.StatusCode} ({response.StatusCode}). Body: {body}");
        }
    }
}
