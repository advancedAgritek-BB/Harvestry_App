using Harvestry.ProcessingJobs.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.ProcessingJobs.Domain.Entities;

/// <summary>
/// Processing job aggregate root - represents a manufacturing/processing operation
/// </summary>
public sealed partial class ProcessingJob : AggregateRoot<Guid>
{
    private readonly List<ProcessingJobInput> _inputs = new();
    private readonly List<ProcessingJobOutput> _outputs = new();

    // Private constructor for EF Core
    private ProcessingJob(Guid id) : base(id) { }

    private ProcessingJob(
        Guid id,
        Guid siteId,
        Guid jobTypeId,
        string jobTypeName,
        Guid createdByUserId) : base(id)
    {
        ValidateConstructorArgs(siteId, jobTypeId, jobTypeName, createdByUserId);

        SiteId = siteId;
        JobTypeId = jobTypeId;
        JobTypeName = jobTypeName.Trim();
        Status = ProcessingJobStatus.Active;
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Core identification
    public Guid SiteId { get; private set; }
    public Guid JobTypeId { get; private set; }
    public string JobTypeName { get; private set; } = string.Empty;

    // Dates
    public DateOnly StartDate { get; private set; }
    public DateOnly? FinishDate { get; private set; }

    // Status
    public ProcessingJobStatus Status { get; private set; }
    public string? Notes { get; private set; }

    // METRC sync tracking
    public long? MetrcProcessingJobId { get; private set; }
    public DateTime? MetrcLastSyncAt { get; private set; }
    public string? MetrcSyncStatus { get; private set; }

    // Metadata
    public Dictionary<string, object> Metadata { get; private set; } = new();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    // Navigation
    public IReadOnlyCollection<ProcessingJobInput> Inputs => _inputs.AsReadOnly();
    public IReadOnlyCollection<ProcessingJobOutput> Outputs => _outputs.AsReadOnly();

    /// <summary>
    /// Factory method to create a new processing job
    /// </summary>
    public static ProcessingJob Create(
        Guid siteId,
        Guid jobTypeId,
        string jobTypeName,
        Guid createdByUserId)
    {
        return new ProcessingJob(
            Guid.NewGuid(),
            siteId,
            jobTypeId,
            jobTypeName,
            createdByUserId);
    }

    /// <summary>
    /// Add input package to the job
    /// </summary>
    public ProcessingJobInput AddInput(
        string packageLabel,
        decimal quantity,
        string unitOfMeasure,
        Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (string.IsNullOrWhiteSpace(packageLabel))
            throw new ArgumentException("Package label cannot be empty", nameof(packageLabel));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        var input = ProcessingJobInput.Create(
            Id,
            packageLabel,
            quantity,
            unitOfMeasure);

        _inputs.Add(input);
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        return input;
    }

    /// <summary>
    /// Add output package from the job
    /// </summary>
    public ProcessingJobOutput AddOutput(
        string packageLabel,
        Guid itemId,
        string itemName,
        decimal quantity,
        string unitOfMeasure,
        Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (string.IsNullOrWhiteSpace(packageLabel))
            throw new ArgumentException("Package label cannot be empty", nameof(packageLabel));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        var output = ProcessingJobOutput.Create(
            Id,
            packageLabel,
            itemId,
            itemName,
            quantity,
            unitOfMeasure);

        _outputs.Add(output);
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        return output;
    }

    /// <summary>
    /// Finish the processing job
    /// </summary>
    public void Finish(DateOnly finishDate, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (!_outputs.Any())
            throw new InvalidOperationException("Cannot finish job without any outputs");

        Status = ProcessingJobStatus.Finished;
        FinishDate = finishDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unfinish/reopen the job
    /// </summary>
    public void Unfinish(Guid userId)
    {
        ValidateUserId(userId);

        if (Status != ProcessingJobStatus.Finished)
            throw new InvalidOperationException("Job is not finished");

        Status = ProcessingJobStatus.Active;
        FinishDate = null;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update METRC sync information
    /// </summary>
    public void UpdateMetrcSync(long metrcProcessingJobId, string? syncStatus = null)
    {
        MetrcProcessingJobId = metrcProcessingJobId;
        MetrcLastSyncAt = DateTime.UtcNow;
        MetrcSyncStatus = syncStatus ?? "Synced";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add notes
    /// </summary>
    public void AddNote(string note, Guid userId)
    {
        ValidateUserId(userId);

        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("Note cannot be empty", nameof(note));

        Notes = string.IsNullOrWhiteSpace(Notes)
            ? note.Trim()
            : $"{Notes}\n\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {note.Trim()}";
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set inputs from persistence
    /// </summary>
    public void SetInputs(IEnumerable<ProcessingJobInput> inputs)
    {
        _inputs.Clear();
        if (inputs != null)
            _inputs.AddRange(inputs);
    }

    /// <summary>
    /// Set outputs from persistence
    /// </summary>
    public void SetOutputs(IEnumerable<ProcessingJobOutput> outputs)
    {
        _outputs.Clear();
        if (outputs != null)
            _outputs.AddRange(outputs);
    }

    private void ValidateActiveStatus()
    {
        if (Status == ProcessingJobStatus.Finished)
            throw new InvalidOperationException("Cannot modify a finished processing job");

        if (Status == ProcessingJobStatus.Cancelled)
            throw new InvalidOperationException("Cannot modify a cancelled processing job");
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        Guid jobTypeId,
        string jobTypeName,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (jobTypeId == Guid.Empty)
            throw new ArgumentException("Job type ID cannot be empty", nameof(jobTypeId));

        if (string.IsNullOrWhiteSpace(jobTypeName))
            throw new ArgumentException("Job type name cannot be empty", nameof(jobTypeName));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}









