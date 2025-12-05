using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Application.Services;
using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

using GeneticsEntity = Harvestry.Genetics.Domain.Entities.Genetics;
using PhenotypeEntity = Harvestry.Genetics.Domain.Entities.Phenotype;
using StrainEntity = Harvestry.Genetics.Domain.Entities.Strain;
using BatchEntity = Harvestry.Genetics.Domain.Entities.Batch;

namespace Harvestry.Genetics.Tests.Unit.Services;

public sealed class GeneticsManagementServiceTests
{
    private readonly Mock<IGeneticsRepository> _geneticsRepository = new(MockBehavior.Strict);
    private readonly Mock<IPhenotypeRepository> _phenotypeRepository = new(MockBehavior.Strict);
    private readonly Mock<IStrainRepository> _strainRepository = new(MockBehavior.Strict);
    private readonly GeneticsManagementService _sut;

    public GeneticsManagementServiceTests()
    {
        _sut = new GeneticsManagementService(
            _geneticsRepository.Object,
            _phenotypeRepository.Object,
            _strainRepository.Object,
            Mock.Of<ILogger<GeneticsManagementService>>());
    }

    [Fact]
    public async Task CreateGeneticsAsync_PersistsEntityAndReturnsMappedResponse()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateGeneticsRequest(
            Name: "Test Genetics",
            Description: "Test Description",
            GeneticType: GeneticType.Hybrid,
            ThcMin: 18m,
            ThcMax: 24m,
            CbdMin: 0.1m,
            CbdMax: 1.2m,
            FloweringTimeDays: 65,
            YieldPotential: YieldPotential.Medium,
            GrowthCharacteristics: GeneticProfile.Empty,
            TerpeneProfile: TerpeneProfile.Empty,
            BreedingNotes: "Notes");

        GeneticsEntity? persistedEntity = null;

        _geneticsRepository
            .Setup(repo => repo.AddAsync(It.IsAny<GeneticsEntity>(), It.IsAny<CancellationToken>()))
            .Callback<GeneticsEntity, CancellationToken>((entity, _) => persistedEntity = entity)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _sut.CreateGeneticsAsync(siteId, request, userId, CancellationToken.None);

        // Assert
        _geneticsRepository.Verify(repo => repo.AddAsync(It.IsAny<GeneticsEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        persistedEntity.Should().NotBeNull();
        persistedEntity!.SiteId.Should().Be(siteId);
        persistedEntity.Name.Should().Be(request.Name);

        response.SiteId.Should().Be(siteId);
        response.Name.Should().Be(request.Name);
        response.GeneticType.Should().Be(request.GeneticType);
    }

    [Fact]
    public async Task UpdateGeneticsAsync_GeneticsFromDifferentSite_ThrowsInvalidOperationException()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var geneticsId = Guid.NewGuid();
        var storedGenetics = CreateGenetics(Guid.NewGuid()); // Different site

        _geneticsRepository
            .Setup(repo => repo.GetByIdAsync(geneticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedGenetics);

        var request = new UpdateGeneticsRequest(
            Description: "Updated",
            ThcMin: 15m,
            ThcMax: 22m,
            CbdMin: 0.2m,
            CbdMax: 0.8m,
            FloweringTimeDays: 62,
            YieldPotential: YieldPotential.High,
            GrowthCharacteristics: GeneticProfile.Empty,
            TerpeneProfile: TerpeneProfile.Empty,
            BreedingNotes: "Updated notes");

        // Act
        Func<Task> action = () => _sut.UpdateGeneticsAsync(siteId, geneticsId, request, userId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>
            ().WithMessage("Cannot update genetics from a different site");

        _geneticsRepository.Verify(repo => repo.UpdateAsync(It.IsAny<GeneticsEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteGeneticsAsync_WithDependentStrains_ThrowsInvalidOperationException()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var geneticsId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var storedGenetics = CreateGenetics(siteId);

        _geneticsRepository
            .Setup(repo => repo.GetByIdAsync(geneticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedGenetics);

        _geneticsRepository
            .Setup(repo => repo.HasDependentStrainsAsync(geneticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> action = () => _sut.DeleteGeneticsAsync(siteId, geneticsId, userId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>
            ().WithMessage("Cannot delete genetics that has dependent strains");

        _geneticsRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePhenotypeAsync_GeneticsFromDifferentSite_ThrowsInvalidOperationException()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var geneticsId = Guid.NewGuid();
        var request = new CreatePhenotypeRequest(
            GeneticsId: geneticsId,
            Name: "Purple",
            Description: "Site mismatch",
            ExpressionNotes: null,
            VisualCharacteristics: null,
            AromaProfile: null,
            GrowthPattern: null);

        _geneticsRepository
            .Setup(repo => repo.GetByIdAsync(geneticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateGenetics(Guid.NewGuid())); // different site

        // Act
        Func<Task> action = () => _sut.CreatePhenotypeAsync(siteId, request, userId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Genetics not found or does not belong to this site");

        _phenotypeRepository.Verify(repo => repo.AddAsync(It.IsAny<PhenotypeEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePhenotypeAsync_PhenotypeFromDifferentSite_ThrowsInvalidOperationException()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var phenotypeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var storedPhenotype = CreatePhenotype(Guid.NewGuid(), Guid.NewGuid()); // different site

        _phenotypeRepository
            .Setup(repo => repo.GetByIdAsync(phenotypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedPhenotype);

        var request = new UpdatePhenotypeRequest(
            Description: "Updated",
            ExpressionNotes: null,
            VisualCharacteristics: null,
            AromaProfile: null,
            GrowthPattern: null);

        // Act
        Func<Task> action = () => _sut.UpdatePhenotypeAsync(siteId, phenotypeId, request, userId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot update phenotype from a different site");

        _phenotypeRepository.Verify(repo => repo.UpdateAsync(It.IsAny<PhenotypeEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeletePhenotypeAsync_PhenotypeFromDifferentSite_ThrowsInvalidOperationException()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var phenotypeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var storedPhenotype = CreatePhenotype(Guid.NewGuid(), Guid.NewGuid()); // different site

        _phenotypeRepository
            .Setup(repo => repo.GetByIdAsync(phenotypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedPhenotype);

        // Act
        Func<Task> action = () => _sut.DeletePhenotypeAsync(siteId, phenotypeId, userId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete phenotype from a different site");

        _phenotypeRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateStrainAsync_GeneticsFromDifferentSite_ThrowsInvalidOperationException()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var geneticsId = Guid.NewGuid();
        var request = new CreateStrainRequest(
            GeneticsId: geneticsId,
            PhenotypeId: null,
            Name: "Site mismatch",
            Description: "Invalid",
            Breeder: null,
            SeedBank: null,
            CultivationNotes: null,
            ExpectedHarvestWindowDays: null,
            TargetEnvironment: TargetEnvironment.Empty,
            ComplianceRequirements: ComplianceRequirements.Empty);

        _geneticsRepository
            .Setup(repo => repo.GetByIdAsync(geneticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateGenetics(Guid.NewGuid()));

        // Act
        Func<Task> action = () => _sut.CreateStrainAsync(siteId, request, userId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Genetics not found or does not belong to this site");

        _strainRepository.Verify(repo => repo.AddAsync(It.IsAny<StrainEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateStrainAsync_PhenotypeFromDifferentSite_ThrowsInvalidOperationException()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var geneticsId = Guid.NewGuid();
        var phenotypeId = Guid.NewGuid();
        var request = new CreateStrainRequest(
            GeneticsId: geneticsId,
            PhenotypeId: phenotypeId,
            Name: "Phenotype mismatch",
            Description: "Invalid",
            Breeder: null,
            SeedBank: null,
            CultivationNotes: null,
            ExpectedHarvestWindowDays: null,
            TargetEnvironment: TargetEnvironment.Empty,
            ComplianceRequirements: ComplianceRequirements.Empty);

        _geneticsRepository
            .Setup(repo => repo.GetByIdAsync(geneticsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateGenetics(siteId));

        _phenotypeRepository
            .Setup(repo => repo.GetByIdAsync(phenotypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatePhenotype(Guid.NewGuid(), geneticsId));

        // Act
        Func<Task> action = () => _sut.CreateStrainAsync(siteId, request, userId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Phenotype not found or does not belong to this genetics");

        _strainRepository.Verify(repo => repo.AddAsync(It.IsAny<StrainEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStrainAsync_StrainsFromDifferentSite_ThrowsInvalidOperationException()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var strainId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var storedStrain = CreateStrain(Guid.NewGuid(), Guid.NewGuid());

        _strainRepository
            .Setup(repo => repo.GetByIdAsync(strainId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedStrain);

        var request = new UpdateStrainRequest(
            Description: "Updated",
            Breeder: null,
            SeedBank: null,
            CultivationNotes: null,
            ExpectedHarvestWindowDays: null,
            TargetEnvironment: TargetEnvironment.Empty,
            ComplianceRequirements: ComplianceRequirements.Empty);

        // Act
        Func<Task> action = () => _sut.UpdateStrainAsync(siteId, strainId, request, userId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot update strain from a different site");

        _strainRepository.Verify(repo => repo.UpdateAsync(It.IsAny<StrainEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteStrainAsync_StrainFromDifferentSite_ThrowsInvalidOperationException()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var strainId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var storedStrain = CreateStrain(Guid.NewGuid(), Guid.NewGuid());

        _strainRepository
            .Setup(repo => repo.GetByIdAsync(strainId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedStrain);

        // Act
        Func<Task> action = () => _sut.DeleteStrainAsync(siteId, strainId, userId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete strain from a different site");

        _strainRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteStrainAsync_WithDependentBatches_ThrowsInvalidOperationException()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var strainId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var storedStrain = CreateStrain(siteId, Guid.NewGuid());

        _strainRepository
            .Setup(repo => repo.GetByIdAsync(strainId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedStrain);

        _strainRepository
            .Setup(repo => repo.HasDependentBatchesAsync(strainId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> action = () => _sut.DeleteStrainAsync(siteId, strainId, userId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete strain that has dependent batches");

        _strainRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static GeneticsEntity CreateGenetics(Guid siteId)
    {
        return GeneticsEntity.Create(
            siteId,
            name: "Stored",
            description: "Stored description",
            geneticType: GeneticType.Indica,
            thcRange: (15m, 20m),
            cbdRange: (0.1m, 0.5m),
            floweringTimeDays: 60,
            yieldPotential: YieldPotential.High,
            growthCharacteristics: GeneticProfile.Empty,
            terpeneProfile: TerpeneProfile.Empty,
            createdByUserId: Guid.NewGuid(),
            breedingNotes: "notes");
    }

    private static PhenotypeEntity CreatePhenotype(Guid siteId, Guid geneticsId)
    {
        return PhenotypeEntity.Create(
            siteId,
            geneticsId,
            name: "Stored Phenotype",
            description: "Stored description",
            createdByUserId: Guid.NewGuid());
    }

    private static StrainEntity CreateStrain(Guid siteId, Guid geneticsId)
    {
        return StrainEntity.Create(
            siteId,
            geneticsId,
            phenotypeId: null,
            name: "Stored Strain",
            description: "Stored description",
            createdByUserId: Guid.NewGuid(),
            breeder: null,
            seedBank: null,
            cultivationNotes: null,
            expectedHarvestWindowDays: null,
            targetEnvironment: TargetEnvironment.Empty,
            complianceRequirements: ComplianceRequirements.Empty);
    }
}
