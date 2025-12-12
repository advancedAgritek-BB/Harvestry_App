namespace Harvestry.Genetics.Domain.ValueObjects;

/// <summary>
/// Steering mode indicating the cultivation strategy direction.
/// Vegetative promotes leaf/stem growth; Generative promotes flower/fruit development.
/// </summary>
public enum SteeringMode
{
    /// <summary>Push plants toward vegetative (leaf/stem) growth</summary>
    Vegetative = 1,
    
    /// <summary>Push plants toward generative (flower/fruit) development</summary>
    Generative = 2,
    
    /// <summary>Balanced approach between vegetative and generative</summary>
    Balanced = 3
}

/// <summary>
/// Daily irrigation cycle phases.
/// P1 (Ramp): Initial saturation after lights-on.
/// P2 (Maintenance): Sustain VWC during active photosynthesis.
/// P3 (Dryback): Controlled dryback before lights-off.
/// </summary>
public enum DailyPhase
{
    /// <summary>Night period - lights off, minimal/no irrigation</summary>
    Night = 0,
    
    /// <summary>P1 Ramp phase - saturate substrate after lights-on</summary>
    P1Ramp = 1,
    
    /// <summary>P2 Maintenance phase - maintain target VWC during photosynthesis</summary>
    P2Maintenance = 2,
    
    /// <summary>P3 Dryback phase - controlled drying before lights-off</summary>
    P3Dryback = 3
}

/// <summary>
/// A steering lever represents an environmental or irrigation parameter
/// that can be adjusted to push plants toward vegetative or generative growth.
/// Based on crop steering quick reference: Substrate EC, VWC, VPD, Temperature,
/// Irrigation Frequency, and Feed Duration.
/// </summary>
/// <remarks>
/// Note: "Water content (WC%)" from reference maps to VWC (Volumetric Water Content)
/// which aligns with StreamType.SoilMoisture (type 20) in the telemetry system.
/// </remarks>
public readonly record struct SteeringLever(
    /// <summary>
    /// Metric identifier (e.g., "SubstrateEC", "VWC", "VPD", "Temperature", 
    /// "IrrigationFrequency", "FeedDuration")
    /// </summary>
    string MetricName,
    
    /// <summary>Direction to adjust for vegetative growth (e.g., "Lower", "Higher")</summary>
    string VegetativeTrend,
    
    /// <summary>Direction to adjust for generative growth (e.g., "Higher", "Lower")</summary>
    string GenerativeTrend,
    
    /// <summary>Minimum value for vegetative steering range</summary>
    decimal? VegetativeMinValue,
    
    /// <summary>Maximum value for vegetative steering range</summary>
    decimal? VegetativeMaxValue,
    
    /// <summary>Minimum value for generative steering range</summary>
    decimal? GenerativeMinValue,
    
    /// <summary>Maximum value for generative steering range</summary>
    decimal? GenerativeMaxValue,
    
    /// <summary>Unit of measurement (e.g., "mS/cm", "%", "kPa", "°F", "min")</summary>
    string Unit
)
{
    /// <summary>
    /// Default steering levers based on crop steering quick reference.
    /// </summary>
    public static IReadOnlyList<SteeringLever> DefaultLevers => new List<SteeringLever>
    {
        // Substrate EC: Lower for veg, Higher for gen
        new("SubstrateEC", "Lower", "Higher", 1.5m, 2.5m, 2.5m, 4.0m, "mS/cm"),
        
        // VWC (Volumetric Water Content): Higher for veg, Lower for gen
        new("VWC", "Higher", "Lower", 55m, 70m, 40m, 55m, "%"),
        
        // VPD: Lower for veg, Higher for gen
        new("VPD", "Lower", "Higher", 0.8m, 1.0m, 1.2m, 1.5m, "kPa"),
        
        // Temperature: Higher for veg, Lower for gen
        new("Temperature", "Higher", "Lower", 78m, 84m, 72m, 78m, "°F"),
        
        // Irrigation Frequency: More frequent for veg, Less frequent for gen
        new("IrrigationFrequency", "MoreFrequent", "LessFrequent", null, null, null, null, "events/day"),
        
        // Feed Duration: Longer for veg, Shorter for gen
        new("FeedDuration", "Longer", "Shorter", null, null, null, null, "min")
    };
}

/// <summary>
/// Irrigation signal parameters for vegetative vs generative steering.
/// Phase-specific values for shot sizing, intervals, and dryback targets.
/// </summary>
public readonly record struct IrrigationSignal(
    /// <summary>Signal name (e.g., "ShotSize", "IrrigationInterval", "DailyDryback", "IntershotDryback")</summary>
    string SignalName,
    
    /// <summary>Target value or range for vegetative steering (e.g., "2-4%", "Every 15-40 min")</summary>
    string VegetativeValue,
    
    /// <summary>Target value or range for generative steering (e.g., "4-10%", "Every 40 min-2 hr")</summary>
    string GenerativeValue,
    
    /// <summary>
    /// Applicable daily phase: "P1", "P2", "P3", or "All".
    /// Some signals vary by phase; others apply throughout the day.
    /// </summary>
    string ApplicablePhase,
    
    /// <summary>Minimum numeric value for vegetative range (if applicable)</summary>
    decimal? VegetativeMinValue,
    
    /// <summary>Maximum numeric value for vegetative range (if applicable)</summary>
    decimal? VegetativeMaxValue,
    
    /// <summary>Minimum numeric value for generative range (if applicable)</summary>
    decimal? GenerativeMinValue,
    
    /// <summary>Maximum numeric value for generative range (if applicable)</summary>
    decimal? GenerativeMaxValue,
    
    /// <summary>Unit of measurement</summary>
    string Unit
)
{
    /// <summary>
    /// Default irrigation signals based on crop steering quick reference.
    /// </summary>
    public static IReadOnlyList<IrrigationSignal> DefaultSignals => new List<IrrigationSignal>
    {
        // Shot size (as % of substrate volume): 2-4% veg, 4-10% gen
        new("ShotSize", "2-4%", "4-10%", "All", 2m, 4m, 4m, 10m, "% substrate volume"),
        
        // Irrigation interval during lights-on: 15-40 min veg, 40 min-2 hr gen
        new("IrrigationInterval", "Every 15-40 min", "Every 40 min-2 hr", "All", 15m, 40m, 40m, 120m, "min"),
        
        // Daily dryback (max to min VWC): 10-20% veg (~25% field capacity framing), 25-50% gen (~50% field capacity framing)
        new("DailyDryback", "10-20%", "25-50%", "P3", 10m, 20m, 25m, 50m, "% VWC change"),
        
        // Intershot dryback (drop between events): 1-4% veg, 4-6% gen
        new("IntershotDryback", "1-4%", "4-6%", "P1,P2", 1m, 4m, 4m, 6m, "% VWC")
    };
}

/// <summary>
/// Dryback targets for daily irrigation cycle management.
/// Controls the extent of substrate drying during P3 and between shots.
/// </summary>
public readonly record struct DrybackTargets(
    /// <summary>Target daily dryback percentage for vegetative steering</summary>
    decimal VegetativeDailyDrybackPercent,
    
    /// <summary>Target daily dryback percentage for generative steering</summary>
    decimal GenerativeDailyDrybackPercent,
    
    /// <summary>Target intershot dryback percentage for vegetative steering</summary>
    decimal VegetativeIntershotDrybackPercent,
    
    /// <summary>Target intershot dryback percentage for generative steering</summary>
    decimal GenerativeIntershotDrybackPercent,
    
    /// <summary>Example framing: % of max field capacity for daily dryback (veg)</summary>
    decimal? VegetativeFieldCapacityFraming,
    
    /// <summary>Example framing: % of max field capacity for daily dryback (gen)</summary>
    decimal? GenerativeFieldCapacityFraming
)
{
    /// <summary>
    /// Default dryback targets based on crop steering quick reference.
    /// </summary>
    public static DrybackTargets Default => new(
        VegetativeDailyDrybackPercent: 15m,       // 10-20% range, middle value
        GenerativeDailyDrybackPercent: 37.5m,    // 25-50% range, middle value
        VegetativeIntershotDrybackPercent: 2.5m, // 1-4% range, middle value
        GenerativeIntershotDrybackPercent: 5m,   // 4-6% range, middle value
        VegetativeFieldCapacityFraming: 25m,     // ~25% of max field capacity
        GenerativeFieldCapacityFraming: 50m      // ~50% of max field capacity
    );
}

/// <summary>
/// Phase-specific configuration for a daily irrigation cycle phase (P1, P2, or P3).
/// Contains VWC targets, shot parameters, and dryback settings for each phase.
/// </summary>
public readonly record struct PhaseConfig(
    /// <summary>Phase this configuration applies to</summary>
    DailyPhase Phase,
    
    /// <summary>Minimum target VWC percentage for this phase</summary>
    decimal TargetVwcMin,
    
    /// <summary>Maximum target VWC percentage for this phase</summary>
    decimal TargetVwcMax,
    
    /// <summary>Shot size as percentage of substrate volume</summary>
    decimal ShotSizePercent,
    
    /// <summary>Minimum irrigation interval in minutes</summary>
    int IrrigationIntervalMinMinutes,
    
    /// <summary>Maximum irrigation interval in minutes</summary>
    int IrrigationIntervalMaxMinutes,
    
    /// <summary>Target dryback percentage for this phase</summary>
    decimal DrybackTargetPercent,
    
    /// <summary>Target intershot dryback percentage</summary>
    decimal IntershotDrybackPercent
)
{
    /// <summary>
    /// Default P1 (Ramp) configuration for vegetative steering.
    /// Goal: Saturate substrate quickly after lights-on.
    /// </summary>
    public static PhaseConfig DefaultP1Vegetative => new(
        Phase: DailyPhase.P1Ramp,
        TargetVwcMin: 60m,
        TargetVwcMax: 70m,
        ShotSizePercent: 3m,
        IrrigationIntervalMinMinutes: 15,
        IrrigationIntervalMaxMinutes: 30,
        DrybackTargetPercent: 0m,  // No dryback target in P1
        IntershotDrybackPercent: 2m
    );

    /// <summary>
    /// Default P1 (Ramp) configuration for generative steering.
    /// </summary>
    public static PhaseConfig DefaultP1Generative => new(
        Phase: DailyPhase.P1Ramp,
        TargetVwcMin: 50m,
        TargetVwcMax: 60m,
        ShotSizePercent: 6m,
        IrrigationIntervalMinMinutes: 30,
        IrrigationIntervalMaxMinutes: 60,
        DrybackTargetPercent: 0m,
        IntershotDrybackPercent: 5m
    );

    /// <summary>
    /// Default P2 (Maintenance) configuration for vegetative steering.
    /// Goal: Maintain consistent VWC during active photosynthesis.
    /// </summary>
    public static PhaseConfig DefaultP2Vegetative => new(
        Phase: DailyPhase.P2Maintenance,
        TargetVwcMin: 55m,
        TargetVwcMax: 65m,
        ShotSizePercent: 3m,
        IrrigationIntervalMinMinutes: 20,
        IrrigationIntervalMaxMinutes: 40,
        DrybackTargetPercent: 5m,
        IntershotDrybackPercent: 2.5m
    );

    /// <summary>
    /// Default P2 (Maintenance) configuration for generative steering.
    /// </summary>
    public static PhaseConfig DefaultP2Generative => new(
        Phase: DailyPhase.P2Maintenance,
        TargetVwcMin: 45m,
        TargetVwcMax: 55m,
        ShotSizePercent: 7m,
        IrrigationIntervalMinMinutes: 45,
        IrrigationIntervalMaxMinutes: 90,
        DrybackTargetPercent: 10m,
        IntershotDrybackPercent: 5m
    );

    /// <summary>
    /// Default P3 (Dryback) configuration for vegetative steering.
    /// Goal: Controlled drying before lights-off.
    /// </summary>
    public static PhaseConfig DefaultP3Vegetative => new(
        Phase: DailyPhase.P3Dryback,
        TargetVwcMin: 45m,
        TargetVwcMax: 55m,
        ShotSizePercent: 0m,  // No shots during dryback typically
        IrrigationIntervalMinMinutes: 0,
        IrrigationIntervalMaxMinutes: 0,
        DrybackTargetPercent: 15m,  // 10-20% daily dryback
        IntershotDrybackPercent: 0m
    );

    /// <summary>
    /// Default P3 (Dryback) configuration for generative steering.
    /// More aggressive dryback for generative.
    /// </summary>
    public static PhaseConfig DefaultP3Generative => new(
        Phase: DailyPhase.P3Dryback,
        TargetVwcMin: 35m,
        TargetVwcMax: 45m,
        ShotSizePercent: 0m,
        IrrigationIntervalMinMinutes: 0,
        IrrigationIntervalMaxMinutes: 0,
        DrybackTargetPercent: 37.5m,  // 25-50% daily dryback
        IntershotDrybackPercent: 0m
    );
}

/// <summary>
/// Complete steering configuration combining levers, signals, and phase configs.
/// Used as JSONB storage for crop steering profiles.
/// </summary>
public readonly record struct SteeringConfiguration(
    /// <summary>Target steering mode</summary>
    SteeringMode TargetMode,
    
    /// <summary>Configured steering levers with target ranges</summary>
    IReadOnlyList<SteeringLever> Levers,
    
    /// <summary>Configured irrigation signals</summary>
    IReadOnlyList<IrrigationSignal> Signals,
    
    /// <summary>Dryback targets</summary>
    DrybackTargets DrybackTargets,
    
    /// <summary>P1 Ramp phase configuration</summary>
    PhaseConfig P1Config,
    
    /// <summary>P2 Maintenance phase configuration</summary>
    PhaseConfig P2Config,
    
    /// <summary>P3 Dryback phase configuration</summary>
    PhaseConfig P3Config
)
{
    /// <summary>
    /// Create default vegetative steering configuration.
    /// </summary>
    public static SteeringConfiguration DefaultVegetative => new(
        TargetMode: SteeringMode.Vegetative,
        Levers: SteeringLever.DefaultLevers,
        Signals: IrrigationSignal.DefaultSignals,
        DrybackTargets: DrybackTargets.Default,
        P1Config: PhaseConfig.DefaultP1Vegetative,
        P2Config: PhaseConfig.DefaultP2Vegetative,
        P3Config: PhaseConfig.DefaultP3Vegetative
    );

    /// <summary>
    /// Create default generative steering configuration.
    /// </summary>
    public static SteeringConfiguration DefaultGenerative => new(
        TargetMode: SteeringMode.Generative,
        Levers: SteeringLever.DefaultLevers,
        Signals: IrrigationSignal.DefaultSignals,
        DrybackTargets: DrybackTargets.Default,
        P1Config: PhaseConfig.DefaultP1Generative,
        P2Config: PhaseConfig.DefaultP2Generative,
        P3Config: PhaseConfig.DefaultP3Generative
    );
}

