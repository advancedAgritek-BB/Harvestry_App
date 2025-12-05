using Harvestry.Genetics.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Mother plant health log - immutable health assessment record
/// </summary>
public sealed class MotherHealthLog : Entity<Guid>
{
    // Private constructor for EF Core/rehydration
    private MotherHealthLog(Guid id) : base(id) { }

    private MotherHealthLog(
        Guid id,
        Guid siteId,
        Guid motherPlantId,
        DateOnly logDate,
        HealthAssessment assessment,
        Guid loggedByUserId) : base(id)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (motherPlantId == Guid.Empty)
            throw new ArgumentException("Mother plant ID cannot be empty", nameof(motherPlantId));

        if (loggedByUserId == Guid.Empty)
            throw new ArgumentException("Logged by user ID cannot be empty", nameof(loggedByUserId));

        SiteId = siteId;
        MotherPlantId = motherPlantId;
        LogDate = logDate;
        HealthStatus = assessment.Status;
        PestPressure = assessment.PestPressure;
        DiseasePressure = assessment.DiseasePressure;
        NutrientDeficiencies = assessment.NutrientDeficiencies.ToArray();
        Observations = assessment.Observations;
        TreatmentsApplied = assessment.TreatmentsApplied;
        EnvironmentalNotes = assessment.EnvironmentalNotes;
        PhotoUrls = assessment.PhotoUrls.Select(u => u.ToString()).ToArray();
        LoggedByUserId = loggedByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid MotherPlantId { get; private set; }
    public DateOnly LogDate { get; private set; }
    public Enums.HealthStatus HealthStatus { get; private set; }
    public Enums.PressureLevel PestPressure { get; private set; }
    public Enums.PressureLevel DiseasePressure { get; private set; }
    public string[] NutrientDeficiencies { get; private set; } = Array.Empty<string>();
    public string? Observations { get; private set; }
    public string? TreatmentsApplied { get; private set; }
    public string? EnvironmentalNotes { get; private set; }
    public string[] PhotoUrls { get; private set; } = Array.Empty<string>();
    public Guid LoggedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Factory method to create new health log
    /// </summary>
    public static MotherHealthLog Create(
        Guid siteId,
        Guid motherPlantId,
        DateOnly logDate,
        HealthAssessment assessment,
        Guid loggedByUserId)
    {
        return new MotherHealthLog(
            Guid.NewGuid(),
            siteId,
            motherPlantId,
            logDate,
            assessment,
            loggedByUserId);
    }

    public static MotherHealthLog FromPersistence(
        Guid id,
        Guid siteId,
        Guid motherPlantId,
        DateOnly logDate,
        Enums.HealthStatus healthStatus,
        Enums.PressureLevel pestPressure,
        Enums.PressureLevel diseasePressure,
        string[] nutrientDeficiencies,
        string? observations,
        string? treatmentsApplied,
        string? environmentalNotes,
        string[] photoUrls,
        Guid loggedByUserId,
        DateTime createdAt)
    {
        var log = new MotherHealthLog(id)
        {
            SiteId = siteId,
            MotherPlantId = motherPlantId,
            LogDate = logDate,
            HealthStatus = healthStatus,
            PestPressure = pestPressure,
            DiseasePressure = diseasePressure,
            NutrientDeficiencies = nutrientDeficiencies ?? Array.Empty<string>(),
            Observations = observations,
            TreatmentsApplied = treatmentsApplied,
            EnvironmentalNotes = environmentalNotes,
            PhotoUrls = photoUrls ?? Array.Empty<string>(),
            LoggedByUserId = loggedByUserId,
            CreatedAt = createdAt
        };

        return log;
    }

    /// <summary>
    /// Reconstruct the health assessment value object
    /// </summary>
    public HealthAssessment GetHealthAssessment()
    {
        return new HealthAssessment(
            HealthStatus,
            PestPressure,
            DiseasePressure,
            NutrientDeficiencies,
            Observations,
            TreatmentsApplied,
            EnvironmentalNotes,
            PhotoUrls.Select(url => new Uri(url)).ToArray());
    }
}

