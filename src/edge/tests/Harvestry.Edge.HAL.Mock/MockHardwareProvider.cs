using System.Collections.Concurrent;
using Harvestry.Edge.HAL.Interfaces;

namespace Harvestry.Edge.HAL.Mock;

public class MockHardwareProvider : IHardwareProvider
{
    private readonly ConcurrentDictionary<int, PinValue> _digitalPins = new();
    private readonly ConcurrentDictionary<int, double> _analogChannels = new();
    private readonly ConcurrentDictionary<int, PinMode> _pinModes = new();

    public void Initialize()
    {
        // No-op for mock
    }

    public void OpenPin(int pinNumber, PinMode mode)
    {
        _pinModes[pinNumber] = mode;
        if (!_digitalPins.ContainsKey(pinNumber))
        {
            _digitalPins[pinNumber] = PinValue.Low;
        }
    }

    public void Write(int pinNumber, PinValue value)
    {
        if (!_pinModes.ContainsKey(pinNumber))
        {
            throw new InvalidOperationException($"Pin {pinNumber} not open.");
        }
        if (_pinModes[pinNumber] != PinMode.Output)
        {
            // In reality, writing to input might toggle pull-up, but for simplicity:
            // throw new InvalidOperationException($"Pin {pinNumber} is not configured as Output.");
        }
        _digitalPins[pinNumber] = value;
    }

    public PinValue Read(int pinNumber)
    {
        if (!_pinModes.ContainsKey(pinNumber))
        {
            throw new InvalidOperationException($"Pin {pinNumber} not open.");
        }
        return _digitalPins.TryGetValue(pinNumber, out var val) ? val : PinValue.Low;
    }

    public double ReadAnalog(int channel)
    {
        return _analogChannels.TryGetValue(channel, out var val) ? val : 0.0;
    }

    public void WriteAnalog(int channel, double value)
    {
        _analogChannels[channel] = value;
    }

    public bool IsPinSupported(int pinNumber)
    {
        return true; // Mock supports all pins
    }

    // Helper methods for tests to inject state
    public void SetDigitalInput(int pinNumber, PinValue value)
    {
        _digitalPins[pinNumber] = value;
    }

    public void SetAnalogInput(int channel, double value)
    {
        _analogChannels[channel] = value;
    }

    public PinValue GetDigitalOutput(int pinNumber)
    {
        return _digitalPins.TryGetValue(pinNumber, out var val) ? val : PinValue.Low;
    }
}




