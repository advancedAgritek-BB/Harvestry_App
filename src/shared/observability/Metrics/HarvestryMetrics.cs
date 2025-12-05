using System.Diagnostics.Metrics;

namespace Harvestry.Shared.Observability;

/// <summary>
/// Centralized custom metrics for Harvestry platform monitoring.
/// These metrics complement the automatic instrumentation from OpenTelemetry.
/// </summary>
public sealed class HarvestryMetrics
{
    public const string MeterName = "Harvestry.Metrics";
    
    private readonly Meter _meter;
    
    // =========================================================================
    // Telemetry Metrics
    // =========================================================================
    
    /// <summary>Counter for total sensor readings ingested.</summary>
    public Counter<long> TelemetryIngestCount { get; }
    
    /// <summary>Histogram for telemetry ingest latency in milliseconds.</summary>
    public Histogram<double> TelemetryIngestLatency { get; }
    
    /// <summary>Counter for duplicate messages detected during ingest.</summary>
    public Counter<long> TelemetryDuplicateCount { get; }
    
    /// <summary>Gauge for active WebSocket subscribers.</summary>
    public ObservableGauge<int> TelemetrySubscriberCount { get; }
    
    /// <summary>Counter for alerts fired.</summary>
    public Counter<long> AlertsFiredCount { get; }
    
    /// <summary>Counter for alerts acknowledged.</summary>
    public Counter<long> AlertsAcknowledgedCount { get; }
    
    // =========================================================================
    // Authentication Metrics
    // =========================================================================
    
    /// <summary>Counter for badge login attempts.</summary>
    public Counter<long> AuthBadgeLoginAttempts { get; }
    
    /// <summary>Counter for successful badge logins.</summary>
    public Counter<long> AuthBadgeLoginSuccess { get; }
    
    /// <summary>Counter for failed badge logins.</summary>
    public Counter<long> AuthBadgeLoginFailure { get; }
    
    /// <summary>Counter for sessions revoked.</summary>
    public Counter<long> AuthSessionsRevoked { get; }
    
    /// <summary>Counter for ABAC policy denials.</summary>
    public Counter<long> AuthPolicyDenials { get; }
    
    // =========================================================================
    // Task Metrics
    // =========================================================================
    
    /// <summary>Counter for tasks created.</summary>
    public Counter<long> TasksCreated { get; }
    
    /// <summary>Counter for tasks completed.</summary>
    public Counter<long> TasksCompleted { get; }
    
    /// <summary>Counter for tasks blocked by gating.</summary>
    public Counter<long> TasksBlocked { get; }
    
    /// <summary>Histogram for task completion time in hours.</summary>
    public Histogram<double> TaskCompletionTime { get; }
    
    // =========================================================================
    // Genetics Metrics
    // =========================================================================
    
    /// <summary>Counter for batches created.</summary>
    public Counter<long> BatchesCreated { get; }
    
    /// <summary>Counter for batch stage transitions.</summary>
    public Counter<long> BatchStageTransitions { get; }
    
    /// <summary>Counter for propagation events.</summary>
    public Counter<long> PropagationEvents { get; }
    
    // =========================================================================
    // Integration Metrics
    // =========================================================================
    
    /// <summary>Counter for Slack messages sent.</summary>
    public Counter<long> SlackMessagesSent { get; }
    
    /// <summary>Counter for Slack message failures.</summary>
    public Counter<long> SlackMessagesFailed { get; }
    
    /// <summary>Counter for external API calls.</summary>
    public Counter<long> ExternalApiCalls { get; }
    
    /// <summary>Counter for circuit breaker trips.</summary>
    public Counter<long> CircuitBreakerTrips { get; }
    
    // =========================================================================
    // Constructor
    // =========================================================================
    
    private Func<int>? _subscriberCountCallback;
    
    public HarvestryMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName, "1.0.0");
        
        // Telemetry metrics
        TelemetryIngestCount = _meter.CreateCounter<long>(
            "harvestry.telemetry.ingest.count",
            unit: "readings",
            description: "Total number of sensor readings ingested");
        
        TelemetryIngestLatency = _meter.CreateHistogram<double>(
            "harvestry.telemetry.ingest.latency",
            unit: "ms",
            description: "Latency of telemetry ingest operations");
        
        TelemetryDuplicateCount = _meter.CreateCounter<long>(
            "harvestry.telemetry.duplicates.count",
            unit: "messages",
            description: "Number of duplicate messages detected");
        
        TelemetrySubscriberCount = _meter.CreateObservableGauge<int>(
            "harvestry.telemetry.subscribers.count",
            observeValue: () => _subscriberCountCallback?.Invoke() ?? 0,
            unit: "connections",
            description: "Current number of real-time subscribers");
        
        AlertsFiredCount = _meter.CreateCounter<long>(
            "harvestry.alerts.fired.count",
            unit: "alerts",
            description: "Total number of alerts fired");
        
        AlertsAcknowledgedCount = _meter.CreateCounter<long>(
            "harvestry.alerts.acknowledged.count",
            unit: "alerts",
            description: "Total number of alerts acknowledged");
        
        // Authentication metrics
        AuthBadgeLoginAttempts = _meter.CreateCounter<long>(
            "harvestry.auth.badge_login.attempts",
            unit: "attempts",
            description: "Total badge login attempts");
        
        AuthBadgeLoginSuccess = _meter.CreateCounter<long>(
            "harvestry.auth.badge_login.success",
            unit: "logins",
            description: "Successful badge logins");
        
        AuthBadgeLoginFailure = _meter.CreateCounter<long>(
            "harvestry.auth.badge_login.failure",
            unit: "failures",
            description: "Failed badge logins");
        
        AuthSessionsRevoked = _meter.CreateCounter<long>(
            "harvestry.auth.sessions.revoked",
            unit: "sessions",
            description: "Sessions revoked");
        
        AuthPolicyDenials = _meter.CreateCounter<long>(
            "harvestry.auth.policy.denials",
            unit: "denials",
            description: "ABAC policy denials");
        
        // Task metrics
        TasksCreated = _meter.CreateCounter<long>(
            "harvestry.tasks.created",
            unit: "tasks",
            description: "Tasks created");
        
        TasksCompleted = _meter.CreateCounter<long>(
            "harvestry.tasks.completed",
            unit: "tasks",
            description: "Tasks completed");
        
        TasksBlocked = _meter.CreateCounter<long>(
            "harvestry.tasks.blocked",
            unit: "tasks",
            description: "Tasks blocked by gating");
        
        TaskCompletionTime = _meter.CreateHistogram<double>(
            "harvestry.tasks.completion_time",
            unit: "hours",
            description: "Time to complete tasks");
        
        // Genetics metrics
        BatchesCreated = _meter.CreateCounter<long>(
            "harvestry.genetics.batches.created",
            unit: "batches",
            description: "Batches created");
        
        BatchStageTransitions = _meter.CreateCounter<long>(
            "harvestry.genetics.batches.stage_transitions",
            unit: "transitions",
            description: "Batch stage transitions");
        
        PropagationEvents = _meter.CreateCounter<long>(
            "harvestry.genetics.propagation.events",
            unit: "events",
            description: "Propagation events recorded");
        
        // Integration metrics
        SlackMessagesSent = _meter.CreateCounter<long>(
            "harvestry.integrations.slack.messages_sent",
            unit: "messages",
            description: "Slack messages sent successfully");
        
        SlackMessagesFailed = _meter.CreateCounter<long>(
            "harvestry.integrations.slack.messages_failed",
            unit: "messages",
            description: "Slack messages that failed to send");
        
        ExternalApiCalls = _meter.CreateCounter<long>(
            "harvestry.integrations.external_api.calls",
            unit: "calls",
            description: "External API calls made");
        
        CircuitBreakerTrips = _meter.CreateCounter<long>(
            "harvestry.integrations.circuit_breaker.trips",
            unit: "trips",
            description: "Circuit breaker trips");
    }
    
    /// <summary>
    /// Registers a callback to report the current subscriber count.
    /// </summary>
    public void RegisterSubscriberCountCallback(Func<int> callback)
    {
        _subscriberCountCallback = callback;
    }
    
    // =========================================================================
    // Convenience Methods
    // =========================================================================
    
    /// <summary>Records a successful telemetry ingest operation.</summary>
    public void RecordTelemetryIngest(int messageCount, double latencyMs, string streamType)
    {
        TelemetryIngestCount.Add(messageCount, 
            new KeyValuePair<string, object?>("stream_type", streamType));
        TelemetryIngestLatency.Record(latencyMs,
            new KeyValuePair<string, object?>("stream_type", streamType));
    }
    
    /// <summary>Records a badge login attempt result.</summary>
    public void RecordBadgeLogin(bool success, string? failureReason = null)
    {
        AuthBadgeLoginAttempts.Add(1);
        if (success)
        {
            AuthBadgeLoginSuccess.Add(1);
        }
        else
        {
            AuthBadgeLoginFailure.Add(1,
                new KeyValuePair<string, object?>("reason", failureReason ?? "unknown"));
        }
    }
    
    /// <summary>Records a task state change.</summary>
    public void RecordTaskEvent(string eventType)
    {
        switch (eventType.ToLowerInvariant())
        {
            case "created":
                TasksCreated.Add(1);
                break;
            case "completed":
                TasksCompleted.Add(1);
                break;
            case "blocked":
                TasksBlocked.Add(1);
                break;
        }
    }
    
    /// <summary>Records an external API call.</summary>
    public void RecordExternalApiCall(string integrationName, bool success)
    {
        ExternalApiCalls.Add(1,
            new KeyValuePair<string, object?>("integration", integrationName),
            new KeyValuePair<string, object?>("success", success));
    }
}

