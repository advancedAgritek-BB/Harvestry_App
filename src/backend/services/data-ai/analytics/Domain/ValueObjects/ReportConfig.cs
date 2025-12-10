using System.Text.Json.Serialization;

namespace Harvestry.Analytics.Domain.ValueObjects;

public record ReportConfig
{
    public string Source { get; init; } = string.Empty;
    public List<ReportColumn> Columns { get; init; } = new();
    public List<ReportFilter> Filters { get; init; } = new();
    public List<ReportSort> Sorts { get; init; } = new();

    [JsonConstructor]
    public ReportConfig(string source, List<ReportColumn> columns, List<ReportFilter> filters, List<ReportSort> sorts)
    {
        Source = source;
        Columns = columns ?? new();
        Filters = filters ?? new();
        Sorts = sorts ?? new();
    }

    public static ReportConfig Create(string source) => new(source, new(), new(), new());
}

public record ReportColumn(string Field, string? Aggregation = null, string? Alias = null, string? Format = null);

public record ReportFilter(string Field, string Operator, object? Value);

public record ReportSort(string Field, string Direction = "asc");




