using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.Application.Services;

/// <summary>
/// Service for detecting anomalies in telemetry data using statistical methods.
/// Implements Z-score and moving average deviation for pilot-ready anomaly detection.
/// </summary>
public sealed class AnomalyDetectionService : IAnomalyDetectionService
{
    private readonly ITelemetryQueryRepository _telemetryQueryRepository;
    private readonly ISensorStreamRepository _streamRepository;
    private readonly ILogger<AnomalyDetectionService> _logger;

    // Configurable thresholds
    private const double DEFAULT_ZSCORE_THRESHOLD = 2.5;
    private const double DEFAULT_DEVIATION_PERCENT = 15.0;
    private const int DEFAULT_BASELINE_HOURS = 24;
    private const int MIN_SAMPLES_FOR_ANALYSIS = 10;

    public AnomalyDetectionService(
        ITelemetryQueryRepository telemetryQueryRepository,
        ISensorStreamRepository streamRepository,
        ILogger<AnomalyDetectionService> logger)
    {
        _telemetryQueryRepository = telemetryQueryRepository ?? throw new ArgumentNullException(nameof(telemetryQueryRepository));
        _streamRepository = streamRepository ?? throw new ArgumentNullException(nameof(streamRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Analyzes a stream for anomalies using statistical methods
    /// </summary>
    public async Task<AnomalyAnalysisResult> AnalyzeStreamAsync(
        Guid streamId,
        TimeSpan? analysisWindow = null,
        CancellationToken cancellationToken = default)
    {
        var window = analysisWindow ?? TimeSpan.FromHours(1);
        var baselineWindow = TimeSpan.FromHours(DEFAULT_BASELINE_HOURS);

        var now = DateTimeOffset.UtcNow;
        var start = now - baselineWindow;

        // Get recent readings for baseline
        var readings = await _telemetryQueryRepository.GetReadingsAsync(
            streamId, start, now, limit: 5000, cancellationToken);

        if (readings.Count < MIN_SAMPLES_FOR_ANALYSIS)
        {
            return AnomalyAnalysisResult.InsufficientData(streamId, readings.Count);
        }

        // Calculate baseline statistics
        var values = readings.Select(r => r.Value).ToList();
        var (mean, stdDev) = CalculateStatistics(values);

        // Analyze recent window for anomalies
        var recentStart = now - window;
        var recentReadings = readings
            .Where(r => r.RecordedAt >= recentStart)
            .ToList();

        var anomalies = new List<DetectedAnomaly>();

        foreach (var reading in recentReadings)
        {
            // Z-score analysis
            var zscore = stdDev > 0 ? (reading.Value - mean) / stdDev : 0;
            if (Math.Abs(zscore) > DEFAULT_ZSCORE_THRESHOLD)
            {
                anomalies.Add(new DetectedAnomaly
                {
                    Timestamp = reading.RecordedAt,
                    Value = reading.Value,
                    ExpectedValue = mean,
                    ZScore = zscore,
                    AnomalyType = zscore > 0 ? AnomalyType.HighSpike : AnomalyType.LowSpike,
                    Severity = CalculateSeverity(zscore)
                });
            }
        }

        // Check for drift (moving average deviation)
        var recentMean = recentReadings.Count > 0 
            ? recentReadings.Average(r => r.Value) 
            : mean;
        var driftPercent = mean != 0 
            ? ((recentMean - mean) / Math.Abs(mean)) * 100 
            : 0;

        if (Math.Abs(driftPercent) > DEFAULT_DEVIATION_PERCENT)
        {
            anomalies.Add(new DetectedAnomaly
            {
                Timestamp = now,
                Value = recentMean,
                ExpectedValue = mean,
                ZScore = driftPercent / DEFAULT_DEVIATION_PERCENT,
                AnomalyType = driftPercent > 0 ? AnomalyType.UpwardDrift : AnomalyType.DownwardDrift,
                Severity = Math.Abs(driftPercent) > 25 ? AnomalySeverity.High : AnomalySeverity.Medium
            });
        }

        // Get stream info for recommendations
        var stream = await _streamRepository.GetByIdAsync(streamId, cancellationToken);
        var streamType = stream?.StreamType ?? StreamType.Generic;

        // Generate recommendations
        var recommendations = GenerateRecommendations(anomalies, streamType, mean, stdDev);

        return new AnomalyAnalysisResult
        {
            StreamId = streamId,
            AnalysisTimestamp = now,
            BaselineMean = mean,
            BaselineStdDev = stdDev,
            SampleCount = readings.Count,
            AnomaliesDetected = anomalies.Count,
            Anomalies = anomalies,
            Recommendations = recommendations,
            StreamType = streamType
        };
    }

    /// <summary>
    /// Analyzes all active streams for a site
    /// </summary>
    public async Task<SiteAnomalyReport> AnalyzeSiteAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var streams = await _streamRepository.GetBySiteIdAsync(siteId, cancellationToken);
        var results = new List<AnomalyAnalysisResult>();

        foreach (var stream in streams.Where(s => s.IsActive))
        {
            try
            {
                var result = await AnalyzeStreamAsync(stream.Id, cancellationToken: cancellationToken);
                if (result.AnomaliesDetected > 0)
                {
                    results.Add(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze stream {StreamId} for anomalies", stream.Id);
            }
        }

        // Aggregate recommendations
        var allRecommendations = results
            .SelectMany(r => r.Recommendations)
            .GroupBy(r => r.Category)
            .Select(g => g.OrderByDescending(r => r.Priority).First())
            .ToList();

        return new SiteAnomalyReport
        {
            SiteId = siteId,
            ReportTimestamp = DateTimeOffset.UtcNow,
            StreamsAnalyzed = streams.Count(s => s.IsActive),
            StreamsWithAnomalies = results.Count,
            TotalAnomalies = results.Sum(r => r.AnomaliesDetected),
            StreamResults = results,
            TopRecommendations = allRecommendations.Take(5).ToList()
        };
    }

    private static (double Mean, double StdDev) CalculateStatistics(List<double> values)
    {
        if (values.Count == 0)
            return (0, 0);

        var mean = values.Average();
        var sumSquaredDiff = values.Sum(v => Math.Pow(v - mean, 2));
        var variance = sumSquaredDiff / values.Count;
        var stdDev = Math.Sqrt(variance);

        return (mean, stdDev);
    }

    private static AnomalySeverity CalculateSeverity(double zscore)
    {
        var absZ = Math.Abs(zscore);
        return absZ switch
        {
            > 4.0 => AnomalySeverity.Critical,
            > 3.0 => AnomalySeverity.High,
            > 2.5 => AnomalySeverity.Medium,
            _ => AnomalySeverity.Low
        };
    }

    private static List<AnomalyRecommendation> GenerateRecommendations(
        List<DetectedAnomaly> anomalies,
        StreamType streamType,
        double mean,
        double stdDev)
    {
        var recommendations = new List<AnomalyRecommendation>();

        if (anomalies.Count == 0)
            return recommendations;

        // Generate recommendations based on stream type and anomaly patterns
        var spikes = anomalies.Where(a => a.AnomalyType is AnomalyType.HighSpike or AnomalyType.LowSpike).ToList();
        var drifts = anomalies.Where(a => a.AnomalyType is AnomalyType.UpwardDrift or AnomalyType.DownwardDrift).ToList();

        if (spikes.Any())
        {
            recommendations.Add(GenerateSpikeRecommendation(streamType, spikes));
        }

        if (drifts.Any())
        {
            recommendations.Add(GenerateDriftRecommendation(streamType, drifts, mean));
        }

        // Stream-specific recommendations
        recommendations.AddRange(GenerateStreamSpecificRecommendations(streamType, anomalies, mean, stdDev));

        return recommendations.Where(r => r != null).ToList()!;
    }

    private static AnomalyRecommendation GenerateSpikeRecommendation(
        StreamType streamType,
        List<DetectedAnomaly> spikes)
    {
        var highSpikes = spikes.Count(s => s.AnomalyType == AnomalyType.HighSpike);
        var lowSpikes = spikes.Count(s => s.AnomalyType == AnomalyType.LowSpike);

        var action = streamType switch
        {
            StreamType.Temperature when highSpikes > 0 => 
                "Check HVAC system and verify ventilation. High temperature spikes may indicate cooling failure or blocked airflow.",
            StreamType.Temperature when lowSpikes > 0 =>
                "Check heating system and insulation. Low temperature spikes may indicate heating failure or draft infiltration.",
            StreamType.Humidity when highSpikes > 0 =>
                "Check dehumidification system. High humidity spikes increase disease risk.",
            StreamType.Humidity when lowSpikes > 0 =>
                "Check humidification system and irrigation schedule. Low humidity may stress plants.",
            StreamType.EC when highSpikes > 0 =>
                "Review nutrient dosing. High EC spikes may indicate over-feeding or concentration issues.",
            StreamType.PH when highSpikes > 0 || lowSpikes > 0 =>
                "Check pH adjustment system and source water. pH instability affects nutrient uptake.",
            StreamType.Co2 when highSpikes > 0 =>
                "Check CO₂ enrichment system controls. Excessive CO₂ can be harmful.",
            _ => "Investigate sensor readings and environmental controls for the detected anomalies."
        };

        return new AnomalyRecommendation
        {
            Category = "Spike Detection",
            Priority = spikes.Max(s => (int)s.Severity),
            Action = action,
            Impact = $"Detected {spikes.Count} spike(s) that deviate significantly from baseline."
        };
    }

    private static AnomalyRecommendation GenerateDriftRecommendation(
        StreamType streamType,
        List<DetectedAnomaly> drifts,
        double mean)
    {
        var upwardDrift = drifts.Any(d => d.AnomalyType == AnomalyType.UpwardDrift);

        var action = streamType switch
        {
            StreamType.Temperature =>
                upwardDrift 
                    ? "Gradual temperature increase detected. Check cooling capacity and lighting heat output."
                    : "Gradual temperature decrease detected. Check heating system efficiency.",
            StreamType.Humidity =>
                upwardDrift
                    ? "Humidity trending upward. May indicate dehumidifier degradation or increased plant transpiration."
                    : "Humidity trending downward. Check humidification system and irrigation coverage.",
            StreamType.EC =>
                upwardDrift
                    ? "EC trending upward. May indicate salt buildup - consider flush cycle."
                    : "EC trending downward. Check nutrient dosing system calibration.",
            StreamType.VPD =>
                "VPD drift detected. Review temperature and humidity setpoints for optimal plant stress levels.",
            _ => "Environmental parameter showing gradual drift. Review system calibration and setpoints."
        };

        return new AnomalyRecommendation
        {
            Category = "Drift Detection",
            Priority = (int)AnomalySeverity.Medium,
            Action = action,
            Impact = $"Parameter drifting from baseline of {mean:F2}. Early intervention prevents larger issues."
        };
    }

    private static IEnumerable<AnomalyRecommendation> GenerateStreamSpecificRecommendations(
        StreamType streamType,
        List<DetectedAnomaly> anomalies,
        double mean,
        double stdDev)
    {
        var recommendations = new List<AnomalyRecommendation>();

        // High variability recommendation
        if (stdDev / Math.Abs(mean) > 0.2) // CV > 20%
        {
            recommendations.Add(new AnomalyRecommendation
            {
                Category = "Stability",
                Priority = (int)AnomalySeverity.Medium,
                Action = streamType switch
                {
                    StreamType.Temperature => "High temperature variability. Consider adding thermal mass or improving HVAC response time.",
                    StreamType.Humidity => "High humidity variability. Check dehumidifier cycling and room air circulation.",
                    StreamType.EC => "High EC variability. Review mixing tank and injection system consistency.",
                    StreamType.PH => "High pH variability. Check buffer capacity and adjustment system calibration.",
                    _ => "High measurement variability detected. Review control system tuning."
                },
                Impact = "High variability creates plant stress. Tighter control improves consistency and yields."
            });
        }

        return recommendations;
    }
}

/// <summary>
/// Interface for anomaly detection service
/// </summary>
public interface IAnomalyDetectionService
{
    Task<AnomalyAnalysisResult> AnalyzeStreamAsync(
        Guid streamId,
        TimeSpan? analysisWindow = null,
        CancellationToken cancellationToken = default);

    Task<SiteAnomalyReport> AnalyzeSiteAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of anomaly analysis for a single stream
/// </summary>
public sealed record AnomalyAnalysisResult
{
    public Guid StreamId { get; init; }
    public DateTimeOffset AnalysisTimestamp { get; init; }
    public StreamType StreamType { get; init; }
    public double BaselineMean { get; init; }
    public double BaselineStdDev { get; init; }
    public int SampleCount { get; init; }
    public int AnomaliesDetected { get; init; }
    public IReadOnlyList<DetectedAnomaly> Anomalies { get; init; } = Array.Empty<DetectedAnomaly>();
    public IReadOnlyList<AnomalyRecommendation> Recommendations { get; init; } = Array.Empty<AnomalyRecommendation>();

    public static AnomalyAnalysisResult InsufficientData(Guid streamId, int sampleCount) => new()
    {
        StreamId = streamId,
        AnalysisTimestamp = DateTimeOffset.UtcNow,
        SampleCount = sampleCount,
        AnomaliesDetected = 0
    };
}

/// <summary>
/// Site-wide anomaly report
/// </summary>
public sealed record SiteAnomalyReport
{
    public Guid SiteId { get; init; }
    public DateTimeOffset ReportTimestamp { get; init; }
    public int StreamsAnalyzed { get; init; }
    public int StreamsWithAnomalies { get; init; }
    public int TotalAnomalies { get; init; }
    public IReadOnlyList<AnomalyAnalysisResult> StreamResults { get; init; } = Array.Empty<AnomalyAnalysisResult>();
    public IReadOnlyList<AnomalyRecommendation> TopRecommendations { get; init; } = Array.Empty<AnomalyRecommendation>();
}

/// <summary>
/// A detected anomaly
/// </summary>
public sealed record DetectedAnomaly
{
    public DateTimeOffset Timestamp { get; init; }
    public double Value { get; init; }
    public double ExpectedValue { get; init; }
    public double ZScore { get; init; }
    public AnomalyType AnomalyType { get; init; }
    public AnomalySeverity Severity { get; init; }
}

/// <summary>
/// Recommended action for addressing anomalies
/// </summary>
public sealed record AnomalyRecommendation
{
    public string Category { get; init; } = string.Empty;
    public int Priority { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Impact { get; init; } = string.Empty;
}

/// <summary>
/// Types of anomalies that can be detected
/// </summary>
public enum AnomalyType
{
    HighSpike,
    LowSpike,
    UpwardDrift,
    DownwardDrift,
    HighVariability,
    PatternBreak
}

/// <summary>
/// Severity levels for anomalies
/// </summary>
public enum AnomalySeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
