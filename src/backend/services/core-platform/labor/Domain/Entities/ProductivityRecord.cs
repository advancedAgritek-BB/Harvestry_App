using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Labor.Domain.Entities;

public sealed class ProductivityRecord : AggregateRoot<Guid>
{
    private ProductivityRecord(Guid id) : base(id) { }

    private ProductivityRecord(
        Guid id,
        Guid siteId,
        string metric,
        decimal value,
        string unit,
        DateTime observedAtUtc,
        string? referenceId) : base(id)
    {
        SiteId = siteId;
        Metric = metric;
        Value = value;
        Unit = unit;
        ObservedAtUtc = observedAtUtc;
        ReferenceId = referenceId;
    }

    public Guid SiteId { get; private set; }
    public string Metric { get; private set; } = string.Empty;
    public decimal Value { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public DateTime ObservedAtUtc { get; private set; }
    public string? ReferenceId { get; private set; }
    public string? Source { get; private set; }

    public static ProductivityRecord Create(
        Guid siteId,
        string metric,
        decimal value,
        string unit,
        DateTime observedAtUtc,
        string? referenceId,
        string? source)
    {
        return new ProductivityRecord(Guid.NewGuid(), siteId, metric, value, unit, observedAtUtc, referenceId)
        {
            Source = source
        };
    }
}



