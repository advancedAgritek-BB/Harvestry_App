namespace Harvestry.Telemetry.Infrastructure.Configuration;

public class TelemetrySimulationOptions
{
    public const string SectionName = "Telemetry:Simulation";

    public bool Enabled { get; set; } = false;
    public int IntervalSeconds { get; set; } = 5;
}





