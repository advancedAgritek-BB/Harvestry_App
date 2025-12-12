using Harvestry.Telemetry.Domain.Entities;

namespace Harvestry.Telemetry.Domain.Models.Simulation;

public class SimulationState
{
    public Guid StreamId { get; set; }
    public SensorStream Stream { get; set; }
    public SimulationProfile Profile { get; set; }
    public double LastValue { get; set; }
    
    // For Sawtooth
    public double CurrentPhase { get; set; } 

    public SimulationState(SensorStream stream, SimulationProfile profile)
    {
        Stream = stream;
        StreamId = stream.Id;
        Profile = profile;
        LastValue = profile.MidPoint; // Start at midpoint
    }
}











