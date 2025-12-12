namespace Harvestry.Telemetry.Domain.Enums;

/// <summary>
/// Defines the behavior pattern for data simulation.
/// </summary>
public enum SimulationBehavior
{
    /// <summary>
    /// Generates a sine wave pattern over a 24-hour period.
    /// Good for Temperature, Light.
    /// </summary>
    SineWave24H,

    /// <summary>
    /// Generates an inverse sine wave (peaks when SineWave24H troughs).
    /// Good for Humidity (often inverse to Temp).
    /// </summary>
    InverseSineWave24H,

    /// <summary>
    /// Random walk / Brownian motion. Value drifts from previous value.
    /// Good for pH, Soil Moisture (slow changes).
    /// </summary>
    RandomWalk,

    /// <summary>
    /// Constant value with simple Gaussian noise.
    /// Good for CO2 (regulated), stable environments.
    /// </summary>
    StaticWithNoise,
    
    /// <summary>
    /// Sawtooth pattern. Rises steadily then drops.
    /// Good for tank levels (filling/draining).
    /// </summary>
    Sawtooth
}











