using FluentAssertions;
using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Domain.Enums;
using Xunit;

namespace Harvestry.Compliance.Metrc.Tests.Unit;

public sealed class MetrcSyncJobTests
{
    private readonly Guid _siteId = Guid.NewGuid();
    private readonly string _licenseNumber = "CO-12345-MED";
    private readonly string _stateCode = "CO";

    [Fact]
    public void Create_WithValidParameters_CreatesJob()
    {
        // Act
        var job = MetrcSyncJob.Create(_siteId, _licenseNumber, _stateCode, SyncDirection.Outbound);

        // Assert
        job.Should().NotBeNull();
        job.Id.Should().NotBe(Guid.Empty);
        job.SiteId.Should().Be(_siteId);
        job.LicenseNumber.Should().Be(_licenseNumber.ToUpperInvariant());
        job.StateCode.Should().Be(_stateCode.ToUpperInvariant());
        job.Direction.Should().Be(SyncDirection.Outbound);
        job.Status.Should().Be(SyncStatus.Pending);
        job.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithEmptySiteId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => MetrcSyncJob.Create(Guid.Empty, _licenseNumber, _stateCode, SyncDirection.Outbound);
        act.Should().Throw<ArgumentException>().WithMessage("*Site ID*");
    }

    [Fact]
    public void Create_WithEmptyLicenseNumber_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => MetrcSyncJob.Create(_siteId, "", _stateCode, SyncDirection.Outbound);
        act.Should().Throw<ArgumentException>().WithMessage("*License number*");
    }

    [Fact]
    public void Start_FromPending_UpdatesStatusAndTimestamp()
    {
        // Arrange
        var job = MetrcSyncJob.Create(_siteId, _licenseNumber, _stateCode, SyncDirection.Outbound);

        // Act
        job.Start(100);

        // Assert
        job.Status.Should().Be(SyncStatus.Processing);
        job.StartedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        job.TotalItems.Should().Be(100);
    }

    [Fact]
    public void Start_WhenAlreadyProcessing_ThrowsInvalidOperationException()
    {
        // Arrange
        var job = MetrcSyncJob.Create(_siteId, _licenseNumber, _stateCode, SyncDirection.Outbound);
        job.Start(100);

        // Act & Assert
        var act = () => job.Start(50);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_SetsStatusAndClearsErrors()
    {
        // Arrange
        var job = MetrcSyncJob.Create(_siteId, _licenseNumber, _stateCode, SyncDirection.Outbound);
        job.Start(100);
        job.RecordProgress(50, 45, 5);

        // Act
        job.Complete();

        // Assert
        job.Status.Should().Be(SyncStatus.Completed);
        job.CompletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        job.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Fail_IncrementsRetryCount_AndSetsError()
    {
        // Arrange
        var job = MetrcSyncJob.Create(_siteId, _licenseNumber, _stateCode, SyncDirection.Outbound);
        job.Start(100);

        // Act
        job.Fail("Connection timeout", "Details here");

        // Assert
        job.Status.Should().Be(SyncStatus.Failed);
        job.RetryCount.Should().Be(1);
        job.ErrorMessage.Should().Be("Connection timeout");
        job.ErrorDetails.Should().Be("Details here");
        job.CanRetry.Should().BeTrue();
    }

    [Fact]
    public void Fail_AfterMaxRetries_SetsPermanentFailure()
    {
        // Arrange
        var job = MetrcSyncJob.Create(_siteId, _licenseNumber, _stateCode, SyncDirection.Outbound, maxRetries: 2);
        job.Start(100);

        // Act
        job.Fail("Error 1");
        job.Fail("Error 2");

        // Assert
        job.Status.Should().Be(SyncStatus.FailedPermanent);
        job.RetryCount.Should().Be(2);
        job.CanRetry.Should().BeFalse();
        job.IsTerminal.Should().BeTrue();
    }

    [Fact]
    public void RecordProgress_UpdatesCounters()
    {
        // Arrange
        var job = MetrcSyncJob.Create(_siteId, _licenseNumber, _stateCode, SyncDirection.Outbound);
        job.Start(100);

        // Act
        job.RecordProgress(75, 70, 5);

        // Assert
        job.ProcessedItems.Should().Be(75);
        job.SuccessfulItems.Should().Be(70);
        job.FailedItems.Should().Be(5);
        job.LastHeartbeatAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Cancel_SetsStatusAndReason()
    {
        // Arrange
        var job = MetrcSyncJob.Create(_siteId, _licenseNumber, _stateCode, SyncDirection.Outbound);
        job.Start(100);

        // Act
        job.Cancel("User requested cancellation");

        // Assert
        job.Status.Should().Be(SyncStatus.Cancelled);
        job.ErrorMessage.Should().Be("User requested cancellation");
        job.IsTerminal.Should().BeTrue();
    }

    [Fact]
    public void Duration_ReturnsCorrectTimespan()
    {
        // Arrange
        var job = MetrcSyncJob.Create(_siteId, _licenseNumber, _stateCode, SyncDirection.Outbound);

        // Act & Assert - Before start
        job.Duration.Should().BeNull();

        // Act - After start
        job.Start(100);
        var duration = job.Duration;

        // Assert
        duration.Should().NotBeNull();
        duration!.Value.Should().BeGreaterOrEqualTo(TimeSpan.Zero);
    }
}
