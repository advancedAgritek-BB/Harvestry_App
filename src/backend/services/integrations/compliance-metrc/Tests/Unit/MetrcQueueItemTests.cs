using FluentAssertions;
using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Domain.Enums;
using Xunit;

namespace Harvestry.Compliance.Metrc.Tests.Unit;

public sealed class MetrcQueueItemTests
{
    private readonly Guid _syncJobId = Guid.NewGuid();
    private readonly Guid _siteId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();
    private readonly string _licenseNumber = "CO-12345-MED";

    [Fact]
    public void Create_WithValidParameters_CreatesItem()
    {
        // Arrange
        var payload = "{\"name\":\"Test Plant\"}";

        // Act
        var item = MetrcQueueItem.Create(
            _syncJobId,
            _siteId,
            _licenseNumber,
            MetrcEntityType.Plant,
            MetrcOperationType.Create,
            _entityId,
            payload);

        // Assert
        item.Should().NotBeNull();
        item.Id.Should().NotBe(Guid.Empty);
        item.SyncJobId.Should().Be(_syncJobId);
        item.SiteId.Should().Be(_siteId);
        item.LicenseNumber.Should().Be(_licenseNumber.ToUpperInvariant());
        item.EntityType.Should().Be(MetrcEntityType.Plant);
        item.OperationType.Should().Be(MetrcOperationType.Create);
        item.HarvestryEntityId.Should().Be(_entityId);
        item.PayloadJson.Should().Be(payload);
        item.Status.Should().Be(SyncStatus.Pending);
        item.IdempotencyKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Create_WithDependency_SetsDepends()
    {
        // Arrange
        var dependsOnId = Guid.NewGuid();
        var payload = "{\"name\":\"Test\"}";

        // Act
        var item = MetrcQueueItem.Create(
            _syncJobId,
            _siteId,
            _licenseNumber,
            MetrcEntityType.Package,
            MetrcOperationType.Create,
            _entityId,
            payload,
            dependsOnItemId: dependsOnId);

        // Assert
        item.DependsOnItemId.Should().Be(dependsOnId);
    }

    [Fact]
    public void MarkProcessing_UpdatesStatusAndTimestamp()
    {
        // Arrange
        var item = CreateTestItem();

        // Act
        item.MarkProcessing();

        // Assert
        item.Status.Should().Be(SyncStatus.Processing);
        item.ProcessedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Complete_SetsStatusAndMetrcId()
    {
        // Arrange
        var item = CreateTestItem();
        item.MarkProcessing();

        // Act
        item.Complete(metrcId: 12345, metrcLabel: "1A4060300001234000012345");

        // Assert
        item.Status.Should().Be(SyncStatus.Completed);
        item.MetrcId.Should().Be(12345);
        item.MetrcLabel.Should().Be("1A4060300001234000012345");
        item.CompletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        item.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Fail_IncrementsRetryAndSchedulesBackoff()
    {
        // Arrange
        var item = CreateTestItem();
        item.MarkProcessing();

        // Act
        item.Fail("METRC API error", "500");

        // Assert
        item.Status.Should().Be(SyncStatus.Failed);
        item.RetryCount.Should().Be(1);
        item.ErrorMessage.Should().Be("METRC API error");
        item.ErrorCode.Should().Be("500");
        item.ScheduledAt.Should().BeAfter(DateTimeOffset.UtcNow);
        item.CanRetry.Should().BeTrue();
    }

    [Fact]
    public void Fail_AfterMaxRetries_SetsPermanentFailure()
    {
        // Arrange
        var item = MetrcQueueItem.Create(
            _syncJobId,
            _siteId,
            _licenseNumber,
            MetrcEntityType.Plant,
            MetrcOperationType.Create,
            _entityId,
            "{}",
            maxRetries: 2);
        item.MarkProcessing();

        // Act
        item.Fail("Error 1");
        item.Fail("Error 2");

        // Assert
        item.Status.Should().Be(SyncStatus.FailedPermanent);
        item.RetryCount.Should().Be(2);
        item.CanRetry.Should().BeFalse();
        item.IsTerminal.Should().BeTrue();
    }

    [Fact]
    public void IsReadyForProcessing_WhenPending_ReturnsTrue()
    {
        // Arrange
        var item = CreateTestItem();

        // Assert
        item.IsReadyForProcessing.Should().BeTrue();
    }

    [Fact]
    public void IsReadyForProcessing_WhenScheduledInFuture_ReturnsFalse()
    {
        // Arrange
        var item = CreateTestItem();
        item.Schedule(DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        item.IsReadyForProcessing.Should().BeFalse();
    }

    [Fact]
    public void IsReadyForProcessing_WhenScheduledInPast_ReturnsTrue()
    {
        // Arrange
        var item = MetrcQueueItem.FromPersistence(
            Guid.NewGuid(),
            _syncJobId,
            _siteId,
            _licenseNumber,
            MetrcEntityType.Plant,
            MetrcOperationType.Create,
            _entityId,
            null,
            null,
            "{}",
            SyncStatus.Pending,
            100,
            0,
            3,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(-1), // Scheduled in past
            null,
            null,
            null,
            null,
            null,
            "key",
            null);

        // Assert
        item.IsReadyForProcessing.Should().BeTrue();
    }

    [Fact]
    public void Cancel_SetsStatusAndReason()
    {
        // Arrange
        var item = CreateTestItem();

        // Act
        item.Cancel("Cancelled by user");

        // Assert
        item.Status.Should().Be(SyncStatus.Cancelled);
        item.ErrorMessage.Should().Be("Cancelled by user");
        item.IsTerminal.Should().BeTrue();
    }

    [Fact]
    public void RequireManualReview_SetsStatus()
    {
        // Arrange
        var item = CreateTestItem();
        item.MarkProcessing();

        // Act
        item.RequireManualReview("Duplicate tag detected in METRC");

        // Assert
        item.Status.Should().Be(SyncStatus.ManualReviewRequired);
        item.ErrorMessage.Should().Be("Duplicate tag detected in METRC");
    }

    private MetrcQueueItem CreateTestItem()
    {
        return MetrcQueueItem.Create(
            _syncJobId,
            _siteId,
            _licenseNumber,
            MetrcEntityType.Plant,
            MetrcOperationType.Create,
            _entityId,
            "{}");
    }
}
