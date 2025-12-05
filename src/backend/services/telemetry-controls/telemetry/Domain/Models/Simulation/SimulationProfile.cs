using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Domain.Models.Simulation;

/// <summary>
/// Configuration for how a specific stream type should be simulated.
/// </summary>
public class SimulationProfile
{
    public SimulationBehavior Behavior { get; set; }
    
    /// <summary>
    /// Minimum expected value (clamped or baseline).
    /// </summary>
    public double Min { get; set; }
    
    /// <summary>
    /// Maximum expected value.
    /// </summary>
    public double Max { get; set; }
    
    /// <summary>
    /// Magnitude of random noise to add.
    /// </summary>
    public double Noise { get; set; }
    
    /// <summary>
    /// For SineWave: Amplitude of the wave (Max - Min / 2 effectively, but explicit).
    /// </summary>
    public double Amplitude => (Max - Min) / 2.0;
    
    /// <summary>
    /// For SineWave: The baseline/midpoint.
    /// </summary>
    public double MidPoint => (Max + Min) / 2.0;

    /// <summary>
    /// For RandomWalk: The maximum amount to step in one iteration.
    /// </summary>
    public double Volatility { get; set; } = 0.5;

    public SimulationProfile(SimulationBehavior behavior, double min, double max, double noise = 0.0)
    {
        Behavior = behavior;
        Min = min;
        Max = max;
        Noise = noise;
    }
}






