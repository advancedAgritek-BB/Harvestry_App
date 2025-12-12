using Harvestry.Edge.HAL.Interfaces;
using Harvestry.Edge.Storage;
using Harvestry.Edge.Adapters.Mqtt;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harvestry.Edge.Engine;

public class MainEngine : BackgroundService
{
    private readonly IHardwareProvider _hal;
    private readonly LocalStorage _storage;
    private readonly CloudBridge _cloud;
    private readonly ILogger<MainEngine> _logger;
    private readonly SafetyInterlocks _safety;

    public MainEngine(
        IHardwareProvider hal, 
        LocalStorage storage, 
        CloudBridge cloud,
        ILogger<MainEngine> logger)
    {
        _hal = hal;
        _storage = storage;
        _cloud = cloud;
        _logger = logger;
        _safety = new SafetyInterlocks(hal, logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Edge Engine Starting...");

        // Init Hardware
        _hal.Initialize();
        
        // Connect to Cloud
        try 
        {
            await _cloud.ConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to cloud on startup. Continuing in Offline Mode.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 1. Read Sensors
                var inputs = ReadAllSensors();

                // 2. Safety Check (Highest Priority)
                if (!_safety.CheckInterlocks(inputs))
                {
                    // Safety tripped!
                    _logger.LogCritical("SAFETY INTERLOCK TRIPPED. HALTING.");
                    _safety.EmergencyStop();
                    await _cloud.PublishTelemetryAsync("device/alarm", "SAFETY_TRIP");
                }
                else 
                {
                    // 3. Normal Logic / Rule Execution
                    // await _scheduler.EvaluateAsync(inputs);
                }

                // 4. Telemetry Push
                // await _cloud.PublishTelemetryAsync("device/telemetry", JsonSerializer.Serialize(inputs));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Control Loop Error");
            }

            // Loop @ 10Hz
            await Task.Delay(100, stoppingToken);
        }
    }

    private Dictionary<string, double> ReadAllSensors()
    {
        // Example reading
        return new Dictionary<string, double>
        {
            { "Temp", _hal.ReadAnalog(0) }, // Just an example
            // ...
        };
    }
}

public class SafetyInterlocks
{
    private readonly IHardwareProvider _hal;
    private readonly ILogger _logger;

    public SafetyInterlocks(IHardwareProvider hal, ILogger logger)
    {
        _hal = hal;
        _logger = logger;
    }

    public bool CheckInterlocks(Dictionary<string, double> inputs)
    {
        // Example: Check E-Stop Input (Pin 0)
        // Assuming Pin 0 is High = OK, Low = STOP (Fail-Safe)
        var estop = _hal.Read(0);
        if (estop == PinValue.Low)
        {
            return false;
        }

        // Example: Check Leak Sensor
        // ...

        return true;
    }

    public void EmergencyStop()
    {
        // Shut everything down
        for (int i = 0; i < 16; i++) // Assuming 16 valves
        {
            try { _hal.Write(i, PinValue.Low); } catch {}
        }
    }
}





