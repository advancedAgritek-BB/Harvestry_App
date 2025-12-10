namespace Harvestry.Edge.HAL.Interfaces;

/// <summary>
/// Digital Pin Modes
/// </summary>
public enum PinMode
{
    Input,
    Output,
    InputPullUp,
    InputPullDown
}

/// <summary>
/// Digital Pin Logic Level
/// </summary>
public enum PinValue
{
    Low = 0,
    High = 1
}

/// <summary>
/// Hardware Abstraction Layer Interface
/// </summary>
public interface IHardwareProvider
{
    /// <summary>
    /// Initialize the hardware provider
    /// </summary>
    void Initialize();

    /// <summary>
    /// Open a digital pin
    /// </summary>
    void OpenPin(int pinNumber, PinMode mode);

    /// <summary>
    /// Write to a digital pin
    /// </summary>
    void Write(int pinNumber, PinValue value);

    /// <summary>
    /// Read from a digital pin
    /// </summary>
    PinValue Read(int pinNumber);

    /// <summary>
    /// Read analog value (0.0 to 1.0)
    /// </summary>
    double ReadAnalog(int channel);

    /// <summary>
    /// Write analog value (0.0 to 1.0) e.g. for PWM or DAC
    /// </summary>
    void WriteAnalog(int channel, double value);

    /// <summary>
    /// Check if a specific pin is currently supported/available
    /// </summary>
    bool IsPinSupported(int pinNumber);
}




