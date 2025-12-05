using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Harvestry.Telemetry.IntegrationTests;

public sealed class AlertEvaluationTests : IntegrationTestBase
{
    [Fact]
    public async Task ThresholdAbove_FiresAndAcknowledgesAlert()
    {
        var sp = Services;
        var rls = sp.GetRequiredService<ITelemetryRlsContextAccessor>();
        var streamRepo = sp.GetRequiredService<ISensorStreamRepository>();
        var ingest = sp.GetRequiredService<ITelemetryIngestService>();
        var rules = sp.GetRequiredService<IAlertRuleRepository>();
        var alerts = sp.GetRequiredService<IAlertEvaluationService>();

        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        rls.Set(new TelemetryRlsContext(siteId, "service_account", userId));

        // Stream
        var stream = SensorStream.Create(siteId, equipmentId, StreamType.Co2, Unit.PartsPerMillion, "CO2");
        await streamRepo.CreateAsync(stream);

        // Ingest a few readings above threshold
        var now = DateTimeOffset.UtcNow;
        var readings = new List<Harvestry.Telemetry.Application.DTOs.SensorReadingDto>
        {
            new(stream.Id, now.AddMinutes(-4), 1500, Unit.PartsPerMillion, now.AddMinutes(-4), "a1", null),
            new(stream.Id, now.AddMinutes(-3), 1550, Unit.PartsPerMillion, now.AddMinutes(-3), "a2", null),
            new(stream.Id, now.AddMinutes(-2), 1525, Unit.PartsPerMillion, now.AddMinutes(-2), "a3", null),
        };
        var req = new Harvestry.Telemetry.Application.DTOs.IngestTelemetryRequestDto(siteId, equipmentId, IngestionProtocol.Http, readings);
        var res = await ingest.IngestBatchAsync(siteId, req);
        res.Accepted.Should().BeGreaterThan(0);

        // Create rule: average above 1400 ppm triggers
        var th = new ThresholdConfig { RuleType = AlertRuleType.ThresholdAbove, ThresholdValue = 1400 };
        var rule = AlertRule.Create(siteId, "High CO2", AlertRuleType.ThresholdAbove, th, new List<Guid> { stream.Id }, userId, evaluationWindowMinutes: 5);
        await rules.CreateAsync(rule);

        // Evaluate
        var eval = await alerts.EvaluateRuleAsync(rule, DateTimeOffset.UtcNow);
        eval.ShouldFireAlert.Should().BeTrue();

        // Fetch active alerts via repository and acknowledge via service
        var repo = sp.GetRequiredService<IAlertInstanceRepository>();
        var active = await repo.GetActiveBySiteAsync(siteId);
        active.Should().NotBeEmpty();
        var acked = await alerts.AcknowledgeAlertAsync(siteId, active[0].Id, userId, "ack test");
        acked.Should().BeTrue();
    }
}
