using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Cultivar response curve models how a specific strain responds to environmental parameters.
/// Used by Autosteer MPC for optimization decisions. Each curve type (VPD, EC, Temperature, etc.)
/// defines the strain's performance response across a range of values, including optimal zones
/// for vegetative and generative steering.
/// </summary>
public sealed class CultivarResponseCurve : AggregateRoot<Guid>
{
    // Private constructor for EF Core/rehydration
    private CultivarResponseCurve(Guid id) : base(id) { }

    private CultivarResponseCurve(
        Guid id,
        Guid siteId,
        Guid strainId,
        GrowthPhase growthPhase,
        ResponseCurveType curveType,
        IReadOnlyList<ResponsePoint> dataPoints,
        decimal? optimalValue,
        decimal vegetativeZoneMin,
        decimal vegetativeZoneMax,
        decimal generativeZoneMin,
        decimal generativeZoneMax,
        string? source,
        decimal? confidenceScore,
        Guid createdByUserId) : base(id)
    {
        ValidateConstructorArgs(siteId, strainId, dataPoints, vegetativeZoneMin, vegetativeZoneMax,
            generativeZoneMin, generativeZoneMax, confidenceScore, createdByUserId);

        SiteId = siteId;
        StrainId = strainId;
        GrowthPhase = growthPhase;
        CurveType = curveType;
        DataPoints = dataPoints.ToList();
        OptimalValue = optimalValue;
        VegetativeZoneMin = vegetativeZoneMin;
        VegetativeZoneMax = vegetativeZoneMax;
        GenerativeZoneMin = generativeZoneMin;
        GenerativeZoneMax = generativeZoneMax;
        Source = source;
        ConfidenceScore = confidenceScore;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Site this curve belongs to</summary>
    public Guid SiteId { get; private set; }

    /// <summary>Strain this response curve is for</summary>
    public Guid StrainId { get; private set; }

    /// <summary>Growth phase this curve applies to (Propagation, Vegetative, Flowering, etc.)</summary>
    public GrowthPhase GrowthPhase { get; private set; }

    /// <summary>Type of response curve (VPD, EC, Temperature, VWC, etc.)</summary>
    public ResponseCurveType CurveType { get; private set; }

    /// <summary>
    /// Response curve data points defining the performance response.
    /// X = environmental parameter value, Y = relative performance (0-100).
    /// </summary>
    public List<ResponsePoint> DataPoints { get; private set; } = new();

    /// <summary>Optimal value for this metric (peak of the response curve)</summary>
    public decimal? OptimalValue { get; private set; }

    /// <summary>Minimum value for vegetative steering zone</summary>
    public decimal VegetativeZoneMin { get; private set; }

    /// <summary>Maximum value for vegetative steering zone</summary>
    public decimal VegetativeZoneMax { get; private set; }

    /// <summary>Minimum value for generative steering zone</summary>
    public decimal GenerativeZoneMin { get; private set; }

    /// <summary>Maximum value for generative steering zone</summary>
    public decimal GenerativeZoneMax { get; private set; }

    /// <summary>Source of this data (e.g., 'user_observed', 'research', 'inferred', 'breeder_data')</summary>
    public string? Source { get; private set; }

    /// <summary>Confidence score for this curve (0.00 to 1.00)</summary>
    public decimal? ConfidenceScore { get; private set; }

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create a new response curve.
    /// </summary>
    public static CultivarResponseCurve Create(
        Guid siteId,
        Guid strainId,
        GrowthPhase growthPhase,
        ResponseCurveType curveType,
        IReadOnlyList<ResponsePoint> dataPoints,
        decimal vegetativeZoneMin,
        decimal vegetativeZoneMax,
        decimal generativeZoneMin,
        decimal generativeZoneMax,
        Guid createdByUserId,
        decimal? optimalValue = null,
        string? source = null,
        decimal? confidenceScore = null)
    {
        return new CultivarResponseCurve(
            Guid.NewGuid(),
            siteId,
            strainId,
            growthPhase,
            curveType,
            dataPoints,
            optimalValue,
            vegetativeZoneMin,
            vegetativeZoneMax,
            generativeZoneMin,
            generativeZoneMax,
            source,
            confidenceScore,
            createdByUserId);
    }

    /// <summary>
    /// Factory method to rehydrate from persistence.
    /// </summary>
    public static CultivarResponseCurve FromPersistence(
        Guid id,
        Guid siteId,
        Guid strainId,
        GrowthPhase growthPhase,
        ResponseCurveType curveType,
        IReadOnlyList<ResponsePoint> dataPoints,
        decimal? optimalValue,
        decimal vegetativeZoneMin,
        decimal vegetativeZoneMax,
        decimal generativeZoneMin,
        decimal generativeZoneMax,
        string? source,
        decimal? confidenceScore,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var curve = new CultivarResponseCurve(id)
        {
            SiteId = siteId,
            StrainId = strainId,
            GrowthPhase = growthPhase,
            CurveType = curveType,
            DataPoints = dataPoints.ToList(),
            OptimalValue = optimalValue,
            VegetativeZoneMin = vegetativeZoneMin,
            VegetativeZoneMax = vegetativeZoneMax,
            GenerativeZoneMin = generativeZoneMin,
            GenerativeZoneMax = generativeZoneMax,
            Source = source,
            ConfidenceScore = confidenceScore,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return curve;
    }

    /// <summary>
    /// Update the response curve data points.
    /// </summary>
    public void UpdateDataPoints(
        IReadOnlyList<ResponsePoint> dataPoints,
        decimal? optimalValue,
        Guid updatedByUserId)
    {
        if (dataPoints == null || dataPoints.Count < 2)
            throw new ArgumentException("At least 2 data points are required", nameof(dataPoints));

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        DataPoints = dataPoints.ToList();
        OptimalValue = optimalValue;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update the steering zones.
    /// </summary>
    public void UpdateSteeringZones(
        decimal vegetativeZoneMin,
        decimal vegetativeZoneMax,
        decimal generativeZoneMin,
        decimal generativeZoneMax,
        Guid updatedByUserId)
    {
        ValidateZones(vegetativeZoneMin, vegetativeZoneMax, generativeZoneMin, generativeZoneMax);

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        VegetativeZoneMin = vegetativeZoneMin;
        VegetativeZoneMax = vegetativeZoneMax;
        GenerativeZoneMin = generativeZoneMin;
        GenerativeZoneMax = generativeZoneMax;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update metadata (source, confidence).
    /// </summary>
    public void UpdateMetadata(
        string? source,
        decimal? confidenceScore,
        Guid updatedByUserId)
    {
        if (confidenceScore.HasValue && (confidenceScore.Value < 0 || confidenceScore.Value > 1))
            throw new ArgumentException("Confidence score must be between 0 and 1", nameof(confidenceScore));

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        Source = source;
        ConfidenceScore = confidenceScore;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Interpolate performance value for a given input.
    /// </summary>
    public decimal? InterpolatePerformance(decimal inputValue)
    {
        if (DataPoints.Count < 2)
            return null;

        var sortedPoints = DataPoints.OrderBy(p => p.X).ToList();

        // Below range
        if (inputValue <= sortedPoints[0].X)
            return sortedPoints[0].Y;

        // Above range
        if (inputValue >= sortedPoints[^1].X)
            return sortedPoints[^1].Y;

        // Find surrounding points and interpolate
        for (int i = 0; i < sortedPoints.Count - 1; i++)
        {
            if (inputValue >= sortedPoints[i].X && inputValue <= sortedPoints[i + 1].X)
            {
                var x1 = sortedPoints[i].X;
                var x2 = sortedPoints[i + 1].X;
                var y1 = sortedPoints[i].Y;
                var y2 = sortedPoints[i + 1].Y;

                // Linear interpolation
                var t = (inputValue - x1) / (x2 - x1);
                return y1 + t * (y2 - y1);
            }
        }

        return null;
    }

    /// <summary>
    /// Check if a value falls within the vegetative steering zone.
    /// </summary>
    public bool IsInVegetativeZone(decimal value)
        => value >= VegetativeZoneMin && value <= VegetativeZoneMax;

    /// <summary>
    /// Check if a value falls within the generative steering zone.
    /// </summary>
    public bool IsInGenerativeZone(decimal value)
        => value >= GenerativeZoneMin && value <= GenerativeZoneMax;

    private static void ValidateConstructorArgs(
        Guid siteId,
        Guid strainId,
        IReadOnlyList<ResponsePoint> dataPoints,
        decimal vegetativeZoneMin,
        decimal vegetativeZoneMax,
        decimal generativeZoneMin,
        decimal generativeZoneMax,
        decimal? confidenceScore,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (strainId == Guid.Empty)
            throw new ArgumentException("Strain ID cannot be empty", nameof(strainId));

        if (dataPoints == null || dataPoints.Count < 2)
            throw new ArgumentException("At least 2 data points are required", nameof(dataPoints));

        ValidateZones(vegetativeZoneMin, vegetativeZoneMax, generativeZoneMin, generativeZoneMax);

        if (confidenceScore.HasValue && (confidenceScore.Value < 0 || confidenceScore.Value > 1))
            throw new ArgumentException("Confidence score must be between 0 and 1", nameof(confidenceScore));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }

    private static void ValidateZones(
        decimal vegetativeZoneMin,
        decimal vegetativeZoneMax,
        decimal generativeZoneMin,
        decimal generativeZoneMax)
    {
        if (vegetativeZoneMin >= vegetativeZoneMax)
            throw new ArgumentException("Vegetative zone min must be less than max");

        if (generativeZoneMin >= generativeZoneMax)
            throw new ArgumentException("Generative zone min must be less than max");
    }
}

/// <summary>
/// A single point on a response curve.
/// </summary>
public readonly record struct ResponsePoint(
    /// <summary>Input value (e.g., VPD in kPa, EC in mS/cm)</summary>
    decimal X,
    /// <summary>Response/performance value (typically 0-100 scale)</summary>
    decimal Y
);

/// <summary>
/// Growth phase for response curve applicability.
/// </summary>
public enum GrowthPhase
{
    /// <summary>Seedling/clone establishment</summary>
    Propagation = 1,
    
    /// <summary>Vegetative growth stage</summary>
    Vegetative = 2,
    
    /// <summary>Transition/pre-flower</summary>
    Transition = 3,
    
    /// <summary>Early flowering (weeks 1-3)</summary>
    EarlyFlower = 4,
    
    /// <summary>Mid flowering (weeks 4-6)</summary>
    MidFlower = 5,
    
    /// <summary>Late flowering/ripening (weeks 7+)</summary>
    LateFlower = 6,
    
    /// <summary>Flush period before harvest</summary>
    Flush = 7
}

/// <summary>
/// Type of environmental response curve.
/// </summary>
public enum ResponseCurveType
{
    /// <summary>Vapor Pressure Deficit response</summary>
    VPD = 1,
    
    /// <summary>Substrate EC response</summary>
    SubstrateEC = 2,
    
    /// <summary>Air temperature response</summary>
    Temperature = 3,
    
    /// <summary>Volumetric Water Content response</summary>
    VWC = 4,
    
    /// <summary>CO2 concentration response</summary>
    CO2 = 5,
    
    /// <summary>Light intensity (PPFD) response</summary>
    LightIntensity = 6,
    
    /// <summary>Root zone temperature response</summary>
    RootZoneTemp = 7,
    
    /// <summary>Humidity response</summary>
    Humidity = 8
}
