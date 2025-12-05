namespace Harvestry.Telemetry.Domain.Enums;

/// <summary>
/// Quality codes for sensor readings following OPC UA standard.
/// Lower values indicate better quality.
/// </summary>
public enum QualityCode : byte
{
    /// <summary>
    /// Good quality - reading is valid and reliable
    /// </summary>
    Good = 0,
    
    /// <summary>
    /// Uncertain/suspect quality - reading may be questionable
    /// </summary>
    Uncertain = 64,
    
    /// <summary>
    /// Bad quality - reading should not be trusted
    /// </summary>
    Bad = 192,
    
    // Specific bad quality codes
    
    /// <summary>
    /// Sensor not connected or communication failed
    /// </summary>
    BadNoCommunication = 193,
    
    /// <summary>
    /// Reading out of valid range for sensor type
    /// </summary>
    BadOutOfRange = 194,
    
    /// <summary>
    /// Sensor reading is stale (hasn't updated in expected timeframe)
    /// </summary>
    BadStale = 195,
    
    /// <summary>
    /// Sensor is in calibration mode
    /// </summary>
    BadCalibration = 196,
    
    /// <summary>
    /// Timestamp is in the future (clock skew or invalid)
    /// </summary>
    BadFutureTimestamp = 197,
    
    /// <summary>
    /// Sensor configuration error
    /// </summary>
    BadConfigurationError = 198
}

