using System.Diagnostics;

namespace Harvestry.Shared.Observability.Tracing;

/// <summary>
/// Centralized ActivitySource definitions for distributed tracing across Harvestry services.
/// Each service has its own ActivitySource for fine-grained control over trace sampling.
/// </summary>
public static class ActivitySources
{
    // Service names matching OpenTelemetry conventions
    public const string IdentityServiceName = "Harvestry.Identity";
    public const string TelemetryServiceName = "Harvestry.Telemetry";
    public const string GeneticsServiceName = "Harvestry.Genetics";
    public const string SpatialServiceName = "Harvestry.Spatial";
    public const string TasksServiceName = "Harvestry.Tasks";
    public const string IntegrationsServiceName = "Harvestry.Integrations";

    /// <summary>
    /// ActivitySource for the Identity service (authentication, authorization, ABAC).
    /// </summary>
    public static readonly ActivitySource Identity = new(IdentityServiceName, "1.0.0");

    /// <summary>
    /// ActivitySource for the Telemetry service (sensor data ingestion, alerts).
    /// </summary>
    public static readonly ActivitySource Telemetry = new(TelemetryServiceName, "1.0.0");

    /// <summary>
    /// ActivitySource for the Genetics service (strains, batches, mother plants).
    /// </summary>
    public static readonly ActivitySource Genetics = new(GeneticsServiceName, "1.0.0");

    /// <summary>
    /// ActivitySource for the Spatial service (rooms, zones, equipment).
    /// </summary>
    public static readonly ActivitySource Spatial = new(SpatialServiceName, "1.0.0");

    /// <summary>
    /// ActivitySource for the Tasks service (task lifecycle, messaging, Slack).
    /// </summary>
    public static readonly ActivitySource Tasks = new(TasksServiceName, "1.0.0");

    /// <summary>
    /// ActivitySource for integration services (METRC, QuickBooks, etc.).
    /// </summary>
    public static readonly ActivitySource Integrations = new(IntegrationsServiceName, "1.0.0");
}

/// <summary>
/// Extension methods for creating spans with consistent naming conventions.
/// </summary>
public static class ActivityExtensions
{
    /// <summary>
    /// Starts a new activity with the given name and optional tags.
    /// </summary>
    public static Activity? StartActivity(
        this ActivitySource source,
        string operationName,
        ActivityKind kind = ActivityKind.Internal,
        IDictionary<string, object?>? tags = null)
    {
        var activity = source.StartActivity(operationName, kind);
        
        if (activity != null && tags != null)
        {
            foreach (var tag in tags)
            {
                if (tag.Value != null)
                {
                    activity.SetTag(tag.Key, tag.Value);
                }
            }
        }

        return activity;
    }

    /// <summary>
    /// Records an exception on the current activity.
    /// </summary>
    public static void RecordException(this Activity? activity, Exception exception)
    {
        if (activity == null) return;

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
        {
            { "exception.type", exception.GetType().FullName },
            { "exception.message", exception.Message },
            { "exception.stacktrace", exception.StackTrace }
        }));
    }

    /// <summary>
    /// Sets success status on the activity.
    /// </summary>
    public static void SetSuccess(this Activity? activity, string? description = null)
    {
        activity?.SetStatus(ActivityStatusCode.Ok, description);
    }

    /// <summary>
    /// Sets error status on the activity.
    /// </summary>
    public static void SetError(this Activity? activity, string description)
    {
        activity?.SetStatus(ActivityStatusCode.Error, description);
    }

    /// <summary>
    /// Adds a structured event to the activity.
    /// </summary>
    public static void AddEvent(
        this Activity? activity,
        string name,
        IDictionary<string, object?>? attributes = null)
    {
        if (activity == null) return;

        if (attributes != null)
        {
            var tags = new ActivityTagsCollection();
            foreach (var attr in attributes)
            {
                if (attr.Value != null)
                {
                    tags.Add(attr.Key, attr.Value);
                }
            }
            activity.AddEvent(new ActivityEvent(name, tags: tags));
        }
        else
        {
            activity.AddEvent(new ActivityEvent(name));
        }
    }
}

