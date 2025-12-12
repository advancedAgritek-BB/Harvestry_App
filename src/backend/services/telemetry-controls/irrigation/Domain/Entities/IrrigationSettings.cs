using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Irrigation.Domain.Entities;

/// <summary>
/// Site-level irrigation system settings including flow rate limits
/// Used to prevent irrigation events from exceeding system capacity
/// </summary>
public sealed class IrrigationSettings : AggregateRoot<Guid>
{
    // Private constructor for EF Core
    private IrrigationSettings(Guid id) : base(id) { }

    private IrrigationSettings(
        Guid id,
        Guid siteId,
        Guid createdByUserId,
        decimal maxSystemFlowRateLitersPerMinute,
        decimal flowRateSafetyMarginPercent = 5.0m) : base(id)
    {
        ValidateConstructorArgs(siteId, maxSystemFlowRateLitersPerMinute, flowRateSafetyMarginPercent);

        SiteId = siteId;
        MaxSystemFlowRateLitersPerMinute = maxSystemFlowRateLitersPerMinute;
        FlowRateSafetyMarginPercent = flowRateSafetyMarginPercent;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    
    /// <summary>
    /// Maximum system flow rate in liters per minute
    /// This is the total capacity of the irrigation system (pump, lines, etc.)
    /// </summary>
    public decimal MaxSystemFlowRateLitersPerMinute { get; private set; }
    
    /// <summary>
    /// Safety margin percentage (default 5%)
    /// Effective max = MaxSystemFlowRate * (1 - SafetyMargin/100)
    /// </summary>
    public decimal FlowRateSafetyMarginPercent { get; private set; }
    
    /// <summary>
    /// Enable automatic queuing when flow rate would be exceeded
    /// </summary>
    public bool EnableFlowRateQueuing { get; private set; } = true;
    
    /// <summary>
    /// Enable smart suggestions for schedule optimization
    /// </summary>
    public bool EnableSmartSuggestions { get; private set; } = true;
    
    /// <summary>
    /// Minimum number of consecutive queue events before showing suggestions
    /// </summary>
    public int SuggestionThresholdCount { get; private set; } = 3;
    
    public Guid CreatedByUserId { get; private set; }
    public Guid UpdatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Calculate the effective maximum flow rate after applying safety margin
    /// </summary>
    public decimal EffectiveMaxFlowRateLitersPerMinute =>
        MaxSystemFlowRateLitersPerMinute * (1 - FlowRateSafetyMarginPercent / 100);

    public static IrrigationSettings Create(
        Guid siteId,
        Guid createdByUserId,
        decimal maxSystemFlowRateLitersPerMinute,
        decimal flowRateSafetyMarginPercent = 5.0m)
    {
        return new IrrigationSettings(
            Guid.NewGuid(),
            siteId,
            createdByUserId,
            maxSystemFlowRateLitersPerMinute,
            flowRateSafetyMarginPercent);
    }

    public void UpdateFlowRateSettings(
        decimal maxSystemFlowRateLitersPerMinute,
        decimal flowRateSafetyMarginPercent,
        Guid updatedByUserId)
    {
        if (maxSystemFlowRateLitersPerMinute <= 0)
            throw new ArgumentException("Max flow rate must be positive", nameof(maxSystemFlowRateLitersPerMinute));
        if (flowRateSafetyMarginPercent < 0 || flowRateSafetyMarginPercent > 50)
            throw new ArgumentException("Safety margin must be between 0 and 50 percent", nameof(flowRateSafetyMarginPercent));

        MaxSystemFlowRateLitersPerMinute = maxSystemFlowRateLitersPerMinute;
        FlowRateSafetyMarginPercent = flowRateSafetyMarginPercent;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateQueuingSettings(
        bool enableFlowRateQueuing,
        bool enableSmartSuggestions,
        int suggestionThresholdCount,
        Guid updatedByUserId)
    {
        if (suggestionThresholdCount < 1)
            throw new ArgumentException("Suggestion threshold must be at least 1", nameof(suggestionThresholdCount));

        EnableFlowRateQueuing = enableFlowRateQueuing;
        EnableSmartSuggestions = enableSmartSuggestions;
        SuggestionThresholdCount = suggestionThresholdCount;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        decimal maxSystemFlowRateLitersPerMinute,
        decimal flowRateSafetyMarginPercent)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));
        if (maxSystemFlowRateLitersPerMinute <= 0)
            throw new ArgumentException("Max flow rate must be positive", nameof(maxSystemFlowRateLitersPerMinute));
        if (flowRateSafetyMarginPercent < 0 || flowRateSafetyMarginPercent > 50)
            throw new ArgumentException("Safety margin must be between 0 and 50 percent", nameof(flowRateSafetyMarginPercent));
    }
}









