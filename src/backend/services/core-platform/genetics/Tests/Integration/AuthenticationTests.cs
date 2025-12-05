using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using FluentAssertions;
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Mappers;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Moq;
using Xunit;

namespace Harvestry.Genetics.Tests.Integration;

public sealed class AuthenticationTests
{
    [Fact]
    public async Task GetGenetics_WithoutAuthHeaders_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();
        var siteId = Guid.NewGuid();
        var requestUri = $"/api/sites/{siteId}/genetics";

        // Act
        var response = await client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.GeneticsServiceMock.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGenetics_WithHeaderAuth_ReturnsOk()
    {
        // Arrange
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();
        var siteId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/sites/{siteId}/genetics");
        request.Headers.Add("X-User-Id", adminId.ToString());
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyList<GeneticsResponse>>();
        payload.Should().NotBeNull();
        payload.Should().BeEmpty();

        factory.GeneticsServiceMock.Verify(
            service => service.GetGeneticsBySiteAsync(siteId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStrains_WithHeaderAuth_ReturnsOk()
    {
        // Arrange
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var strains = new List<StrainResponse>
        {
            new(
                Id: Guid.NewGuid(),
                SiteId: siteId,
                GeneticsId: Guid.NewGuid(),
                PhenotypeId: null,
                Name: "Test Strain",
                Description: "A strain",
                Breeder: null,
                SeedBank: null,
                CultivationNotes: null,
                ExpectedHarvestWindowDays: null,
                TargetEnvironment: TargetEnvironment.Empty,
                ComplianceRequirements: ComplianceRequirements.Empty,
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow,
                CreatedByUserId: adminId,
                UpdatedByUserId: adminId)
        };

        factory.GeneticsServiceMock
            .Setup(service => service.GetStrainsBySiteAsync(siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(strains);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/sites/{siteId}/strains");
        request.Headers.Add("X-User-Id", adminId.ToString());
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyList<StrainResponse>>();
        payload.Should().NotBeNull();
        payload.Should().BeEquivalentTo(strains);

        factory.GeneticsServiceMock.Verify(
            service => service.GetStrainsBySiteAsync(siteId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateGenetics_WithHeaderAuth_ReturnsCreated()
    {
        // Arrange
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var requestBody = new CreateGeneticsRequest(
            Name: "Glue",
            Description: "Heavy hitting",
            GeneticType: GeneticType.Hybrid,
            ThcMin: 20m,
            ThcMax: 28m,
            CbdMin: 0.1m,
            CbdMax: 0.5m,
            FloweringTimeDays: 60,
            YieldPotential: YieldPotential.High,
            GrowthCharacteristics: GeneticProfile.Empty,
            TerpeneProfile: TerpeneProfile.Empty,
            BreedingNotes: "Testing");

        var createdResponse = new GeneticsResponse(
            Id: Guid.NewGuid(),
            SiteId: siteId,
            Name: requestBody.Name,
            Description: requestBody.Description,
            GeneticType: requestBody.GeneticType,
            ThcMin: requestBody.ThcMin,
            ThcMax: requestBody.ThcMax,
            CbdMin: requestBody.CbdMin,
            CbdMax: requestBody.CbdMax,
            FloweringTimeDays: requestBody.FloweringTimeDays,
            YieldPotential: requestBody.YieldPotential,
            GrowthCharacteristics: requestBody.GrowthCharacteristics,
            TerpeneProfile: requestBody.TerpeneProfile,
            BreedingNotes: requestBody.BreedingNotes,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            CreatedByUserId: userId,
            UpdatedByUserId: userId);

        factory.GeneticsServiceMock
            .Setup(service => service.CreateGeneticsAsync(
                siteId,
                It.Is<CreateGeneticsRequest>(r => r.Name == requestBody.Name),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdResponse);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/sites/{siteId}/genetics")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.AbsolutePath.Should().Be($"/api/sites/{siteId}/genetics/{createdResponse.Id}");

        var payload = await response.Content.ReadFromJsonAsync<GeneticsResponse>();
        payload.Should().BeEquivalentTo(createdResponse);

        factory.GeneticsServiceMock.Verify(
            service => service.CreateGeneticsAsync(
                siteId,
                It.Is<CreateGeneticsRequest>(r => r.Name == requestBody.Name),
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateGenetics_MissingRoleHeader_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var requestBody = new CreateGeneticsRequest(
            Name: "No Role",
            Description: "Missing role header",
            GeneticType: GeneticType.Sativa,
            ThcMin: 15m,
            ThcMax: 25m,
            CbdMin: 0.1m,
            CbdMax: 0.3m,
            FloweringTimeDays: 70,
            YieldPotential: YieldPotential.Medium,
            GrowthCharacteristics: GeneticProfile.Empty,
            TerpeneProfile: TerpeneProfile.Empty,
            BreedingNotes: null);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/sites/{siteId}/genetics")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-Site-Id", siteId.ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.GeneticsServiceMock.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBatches_WithHeaderAuth_ReturnsOk()
    {
        // Arrange
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var batches = new List<BatchResponse>
        {
            new(
                Id: Guid.NewGuid(),
                SiteId: siteId,
                StrainId: Guid.NewGuid(),
                BatchCode: "BATCH-01",
                BatchName: "Active Batch",
                BatchType: BatchType.Seed,
                SourceType: BatchSourceType.Propagation,
                ParentBatchId: null,
                Generation: 1,
                PlantCount: 100,
                TargetPlantCount: 100,
                CurrentStageId: Guid.NewGuid(),
                StageStartedAt: DateTime.UtcNow,
                ExpectedHarvestDate: null,
                ActualHarvestDate: null,
                LocationId: null,
                RoomId: null,
                ZoneId: null,
                Status: BatchStatus.Active,
                Notes: null,
                Metadata: new Dictionary<string, object>(),
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow,
                CreatedByUserId: userId,
                UpdatedByUserId: userId)
        };

        factory.BatchLifecycleServiceMock
            .Setup(service => service.GetActiveBatchesAsync(siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(batches);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/genetics/batches/active");
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<IReadOnlyList<BatchResponse>>();
        payload.Should().NotBeNull();
        payload.Should().BeEquivalentTo(batches);

        factory.BatchLifecycleServiceMock.Verify(
            service => service.GetActiveBatchesAsync(siteId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveBatches_MissingRoleHeader_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/genetics/batches/active");
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-Site-Id", siteId.ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.BatchLifecycleServiceMock.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task TransitionBatchStage_WithHeaderAuth_ReturnsOk()
    {
        // Arrange
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var requestBody = new TransitionBatchStageRequest(
            NewStageId: Guid.NewGuid(),
            TransitionNotes: "Stage up"
        );

        var responseDto = new BatchResponse(
            Id: batchId,
            SiteId: siteId,
            StrainId: Guid.NewGuid(),
            BatchCode: "BATCH-02",
            BatchName: "Transitioned Batch",
            BatchType: BatchType.Seed,
            SourceType: BatchSourceType.Propagation,
            ParentBatchId: null,
            Generation: 1,
            PlantCount: 120,
            TargetPlantCount: 120,
            CurrentStageId: requestBody.NewStageId,
            StageStartedAt: DateTime.UtcNow,
            ExpectedHarvestDate: null,
            ActualHarvestDate: null,
            LocationId: null,
            RoomId: null,
            ZoneId: null,
            Status: BatchStatus.Active,
            Notes: null,
            Metadata: new Dictionary<string, object>(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            CreatedByUserId: userId,
            UpdatedByUserId: userId);

        factory.BatchLifecycleServiceMock
            .Setup(service => service.TransitionBatchStageAsync(batchId, requestBody, siteId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseDto);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/genetics/batches/{batchId}/transition")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<BatchResponse>();
        payload.Should().BeEquivalentTo(responseDto);

        factory.BatchLifecycleServiceMock.Verify(
            service => service.TransitionBatchStageAsync(batchId, requestBody, siteId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TransitionBatchStage_MissingRoleHeader_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var requestBody = new TransitionBatchStageRequest(NewStageId: Guid.NewGuid());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/genetics/batches/{batchId}/transition")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-Site-Id", siteId.ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.BatchLifecycleServiceMock.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveBatchStages_WithHeaderAuth_ReturnsOk()
    {
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        factory.BatchStageConfigurationServiceMock
            .Setup(service => service.GetActiveStagesAsync(siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<BatchStageResponse>());

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/genetics/batch-stages/active");
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        factory.BatchStageConfigurationServiceMock.Verify(
            service => service.GetActiveStagesAsync(siteId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateBatchStageTransition_WithHeaderAuth_ReturnsCreated()
    {
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var requestBody = new CreateStageTransitionRequest(
            FromStageId: Guid.NewGuid(),
            ToStageId: Guid.NewGuid(),
            AutoAdvance: true,
            RequiresApproval: false,
            ApprovalRole: null);

        var responseDto = BatchStageMapper.ToTransitionResponse(
            BatchStageTransition.Create(siteId, requestBody.FromStageId, requestBody.ToStageId, userId,
                requestBody.AutoAdvance, requestBody.RequiresApproval, requestBody.ApprovalRole));

        factory.BatchStageConfigurationServiceMock
            .Setup(service => service.CreateTransitionAsync(requestBody, siteId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseDto);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/genetics/batch-stages/transitions")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<StageTransitionResponse>();
        payload.Should().BeEquivalentTo(responseDto);

        factory.BatchStageConfigurationServiceMock.Verify(
            service => service.CreateTransitionAsync(requestBody, siteId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateBatchStageTransition_MissingRoleHeader_ReturnsUnauthorized()
    {
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var requestBody = new CreateStageTransitionRequest(
            FromStageId: Guid.NewGuid(),
            ToStageId: Guid.NewGuid());

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/genetics/batch-stages/transitions")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-Site-Id", siteId.ToString());

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.BatchStageConfigurationServiceMock.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllBatchStageTransitions_WithHeaderAuth_ReturnsOk()
    {
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        factory.BatchStageConfigurationServiceMock
            .Setup(service => service.GetAllTransitionsAsync(siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<StageTransitionResponse>());

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/genetics/batch-stages/transitions");
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.BatchStageConfigurationServiceMock.Verify(
            service => service.GetAllTransitionsAsync(siteId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllBatchStageTransitions_MissingRoleHeader_ReturnsUnauthorized()
    {
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/genetics/batch-stages/transitions");
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-Site-Id", siteId.ToString());

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.BatchStageConfigurationServiceMock.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateBatchStage_WithHeaderAuth_ReturnsOk()
    {
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var stageId = Guid.NewGuid();

        var requestBody = new UpdateBatchStageRequest("Updated Stage", "Updated", 5, false, true);

        var responseStage = BatchStageDefinition.Create(siteId, StageKey.Create("flower"), "Updated Stage", 5, userId,
            description: "Updated", isTerminal: false, requiresHarvestMetrics: true);
        var responseDto = BatchStageMapper.ToResponse(responseStage);

        factory.BatchStageConfigurationServiceMock
            .Setup(service => service.UpdateStageAsync(stageId, requestBody, siteId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseDto);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/genetics/batch-stages/{stageId}")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.BatchStageConfigurationServiceMock.Verify(
            service => service.UpdateStageAsync(stageId, requestBody, siteId, userId, It.IsAny<CancellationToken>()),
            Times.Once);

        (await response.Content.ReadAsStringAsync()).Should().Contain("Updated Stage");
    }

    [Fact]
    public async Task UpdateBatchStage_MissingRoleHeader_ReturnsUnauthorized()
    {
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var stageId = Guid.NewGuid();
        var requestBody = new UpdateBatchStageRequest("Name", null, 1, false, false);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/genetics/batch-stages/{stageId}")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-Site-Id", siteId.ToString());

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.BatchStageConfigurationServiceMock.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteBatchStage_WithHeaderAuth_ReturnsNoContent()
    {
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var stageId = Guid.NewGuid();

        factory.BatchStageConfigurationServiceMock
            .Setup(service => service.DeleteStageAsync(stageId, siteId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/genetics/batch-stages/{stageId}");
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        factory.BatchStageConfigurationServiceMock.Verify(
            service => service.DeleteStageAsync(stageId, siteId, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteBatchStage_MissingRoleHeader_ReturnsUnauthorized()
    {
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var stageId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/genetics/batch-stages/{stageId}");
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-Site-Id", siteId.ToString());

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.BatchStageConfigurationServiceMock.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateStrain_WithInvalidRole_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var requestBody = new CreateStrainRequest(
            GeneticsId: Guid.NewGuid(),
            PhenotypeId: null,
            Name: "Invalid Role",
            Description: "Attempt with invalid role",
            Breeder: null,
            SeedBank: null,
            CultivationNotes: null,
            ExpectedHarvestWindowDays: null,
            TargetEnvironment: TargetEnvironment.Empty,
            ComplianceRequirements: ComplianceRequirements.Empty);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/sites/{siteId}/strains")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("X-User-Id", userId.ToString());
        request.Headers.Add("X-User-Role", "invalid_role");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.GeneticsServiceMock.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateStrain_MissingUserHeader_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new GeneticsApiFactory();
        using var client = factory.CreateClient();

        var siteId = Guid.NewGuid();
        var requestBody = new CreateStrainRequest(
            GeneticsId: Guid.NewGuid(),
            PhenotypeId: null,
            Name: "No User",
            Description: "Missing user header",
            Breeder: null,
            SeedBank: null,
            CultivationNotes: null,
            ExpectedHarvestWindowDays: null,
            TargetEnvironment: TargetEnvironment.Empty,
            ComplianceRequirements: ComplianceRequirements.Empty);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/sites/{siteId}/strains")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Add("X-User-Role", "admin");
        request.Headers.Add("X-Site-Id", siteId.ToString());

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.GeneticsServiceMock.Invocations.Should().BeEmpty();
    }
}
