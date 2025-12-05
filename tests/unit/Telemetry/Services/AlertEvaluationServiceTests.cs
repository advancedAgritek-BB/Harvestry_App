using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Application.Mappers;
using Harvestry.Telemetry.Application.Services;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harvestry.Telemetry.Tests.Services;

public class AlertEvaluationServiceTests
{
    private readonly MapperConfiguration _mapperConfig = new(cfg => cfg.AddProfile<TelemetryMappingProfile>());

    [Fact]
    public async Task EvaluateRulesAsync_FiresAlert_WhenThresholdExceeded()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var streamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var threshold = new ThresholdConfig
        {
            RuleType = AlertRuleType.ThresholdAbove,
            ThresholdValue = 10
        };

        var rule = AlertRule.Create(
            siteId,
            "High Temperature",
            AlertRuleType.ThresholdAbove,
            threshold,
            new List<Guid> { streamId },
            userId);

        var ruleRepo = new Mock<IAlertRuleRepository>();
        ruleRepo.Setup(r => r.GetActiveBySiteIdAsync(siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AlertRule> { rule });

        var instanceRepo = new Mock<IAlertInstanceRepository>();
        instanceRepo.Setup(r => r.GetActiveByRuleAndStreamAsync(rule.Id, streamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlertInstance?)null);
        instanceRepo.Setup(r => r.CreateAsync(It.IsAny<AlertInstance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var queryRepo = new Mock<ITelemetryQueryRepository>();
        queryRepo.Setup(q => q.GetReadingsAsync(streamId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SensorReading>
            {
                SensorReading.FromIngestion(streamId, 20, QualityCode.Good, DateTimeOffset.UtcNow)
            });

        var service = BuildService(ruleRepo.Object, instanceRepo.Object, queryRepo.Object);

        // Act
        await service.EvaluateRulesAsync(siteId);

        // Assert
        instanceRepo.Verify(r => r.CreateAsync(It.IsAny<AlertInstance>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateRuleAsync_ClearsAlert_WhenConditionResolves()
    {
        var siteId = Guid.NewGuid();
        var streamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var threshold = new ThresholdConfig
        {
            RuleType = AlertRuleType.ThresholdAbove,
            ThresholdValue = 10
        };

        var rule = AlertRule.Create(
            siteId,
            "High Temperature",
            AlertRuleType.ThresholdAbove,
            threshold,
            new List<Guid> { streamId },
            userId);

        var activeAlert = AlertInstance.Fire(siteId, rule.Id, streamId, AlertSeverity.Warning, "Above threshold", 15, 10);

        var ruleRepo = new Mock<IAlertRuleRepository>();
        var instanceRepo = new Mock<IAlertInstanceRepository>();
        instanceRepo.Setup(r => r.GetActiveByRuleAndStreamAsync(rule.Id, streamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeAlert);
        instanceRepo.Setup(r => r.UpdateAsync(activeAlert, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var queryRepo = new Mock<ITelemetryQueryRepository>();
        queryRepo.Setup(q => q.GetReadingsAsync(streamId, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SensorReading>
            {
                SensorReading.FromIngestion(streamId, 5, QualityCode.Good, DateTimeOffset.UtcNow)
            });

        var service = BuildService(ruleRepo.Object, instanceRepo.Object, queryRepo.Object);

        await service.EvaluateRuleAsync(rule, DateTimeOffset.UtcNow);

        instanceRepo.Verify(r => r.UpdateAsync(activeAlert, It.IsAny<CancellationToken>()), Times.Once);
        activeAlert.IsActive().Should().BeFalse();
    }

    private AlertEvaluationService BuildService(
        IAlertRuleRepository ruleRepository,
        IAlertInstanceRepository instanceRepository,
        ITelemetryQueryRepository telemetryQueryRepository)
    {
        var mapper = _mapperConfig.CreateMapper();
        var logger = Mock.Of<ILogger<AlertEvaluationService>>();
        var rlsAccessor = new TestRlsContextAccessor();

        return new AlertEvaluationService(
            ruleRepository,
            instanceRepository,
            telemetryQueryRepository,
            rlsAccessor,
            mapper,
            logger);
    }

    private sealed class TestRlsContextAccessor : ITelemetryRlsContextAccessor
    {
        public TelemetryRlsContext Current { get; private set; } = new(Guid.Empty, "service_account", null);

        public void Clear()
        {
            Current = new TelemetryRlsContext(Guid.Empty, "service_account", null);
        }

        public void Set(TelemetryRlsContext context)
        {
            Current = context;
        }
    }
}
