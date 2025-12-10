using Harvestry.LabTests.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.LabTests.Domain.Entities;

/// <summary>
/// Individual lab test result within a batch
/// </summary>
public sealed class LabTestResult : Entity<Guid>
{
    // Private constructor for EF Core
    private LabTestResult(Guid id) : base(id) { }

    private LabTestResult(
        Guid id,
        Guid labTestBatchId,
        LabTestType testType,
        string testTypeName,
        bool passed,
        string? analyteName = null,
        decimal? resultValue = null,
        string? resultUnit = null,
        decimal? limitValue = null,
        string? notes = null) : base(id)
    {
        LabTestBatchId = labTestBatchId;
        TestType = testType;
        TestTypeName = testTypeName;
        Passed = passed;
        AnalyteName = analyteName;
        ResultValue = resultValue;
        ResultUnit = resultUnit;
        LimitValue = limitValue;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid LabTestBatchId { get; private set; }
    public LabTestType TestType { get; private set; }
    public string TestTypeName { get; private set; } = string.Empty;
    public string? AnalyteName { get; private set; }
    public decimal? ResultValue { get; private set; }
    public string? ResultUnit { get; private set; }
    public decimal? LimitValue { get; private set; }
    public bool Passed { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Factory method to create a new test result
    /// </summary>
    public static LabTestResult Create(
        Guid labTestBatchId,
        LabTestType testType,
        string testTypeName,
        bool passed,
        string? analyteName = null,
        decimal? resultValue = null,
        string? resultUnit = null,
        decimal? limitValue = null,
        string? notes = null)
    {
        if (labTestBatchId == Guid.Empty)
            throw new ArgumentException("Lab test batch ID cannot be empty", nameof(labTestBatchId));

        if (string.IsNullOrWhiteSpace(testTypeName))
            throw new ArgumentException("Test type name cannot be empty", nameof(testTypeName));

        return new LabTestResult(
            Guid.NewGuid(),
            labTestBatchId,
            testType,
            testTypeName.Trim(),
            passed,
            analyteName?.Trim(),
            resultValue,
            resultUnit?.Trim(),
            limitValue,
            notes?.Trim());
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static LabTestResult Restore(
        Guid id,
        Guid labTestBatchId,
        LabTestType testType,
        string testTypeName,
        string? analyteName,
        decimal? resultValue,
        string? resultUnit,
        decimal? limitValue,
        bool passed,
        string? notes,
        DateTime createdAt)
    {
        return new LabTestResult(id)
        {
            LabTestBatchId = labTestBatchId,
            TestType = testType,
            TestTypeName = testTypeName,
            AnalyteName = analyteName,
            ResultValue = resultValue,
            ResultUnit = resultUnit,
            LimitValue = limitValue,
            Passed = passed,
            Notes = notes,
            CreatedAt = createdAt
        };
    }
}








