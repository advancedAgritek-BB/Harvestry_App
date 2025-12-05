using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Domain.Models.Simulation;
using Harvestry.Telemetry.Infrastructure.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Harvestry.Telemetry.API.Controllers;

[ApiController]
[Route("api/v1/simulation")]
public class SimulationController : ControllerBase
{
    private readonly ITelemetrySimulationService _simulationService;
    private readonly TelemetrySimulationOptions _options;

    public SimulationController(
        ITelemetrySimulationService simulationService,
        IOptions<TelemetrySimulationOptions> options)
    {
        _simulationService = simulationService;
        _options = options.Value;
    }

    /// <summary>
    /// Starts simulation for a specific stream type.
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> StartSimulation([FromQuery] StreamType type, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return BadRequest("Simulation is disabled in configuration.");

        await _simulationService.StartSimulationAsync(type, cancellationToken);
        return Ok($"Simulation started for {type}");
    }

    /// <summary>
    /// Stops simulation for a specific stream type.
    /// </summary>
    [HttpPost("stop")]
    public async Task<IActionResult> StopSimulation([FromQuery] StreamType type, CancellationToken cancellationToken)
    {
        await _simulationService.StopSimulationAsync(type, cancellationToken);
        return Ok($"Simulation stopped for {type}");
    }

    /// <summary>
    /// Toggles simulation for a specific stream ID.
    /// </summary>
    [HttpPost("toggle/{streamId}")]
    public async Task<IActionResult> ToggleSimulation([FromRoute] Guid streamId, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return BadRequest("Simulation is disabled in configuration.");

        await _simulationService.ToggleSimulationAsync(streamId, cancellationToken);
        return Ok($"Simulation toggled for stream {streamId}");
    }

    /// <summary>
    /// Updates the simulation profile/behavior for a stream type.
    /// </summary>
    [HttpPost("config")]
    public async Task<IActionResult> UpdateProfile([FromQuery] StreamType type, [FromBody] SimulationProfile profile, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return BadRequest("Simulation is disabled in configuration.");

        await _simulationService.UpdateProfileAsync(type, profile, cancellationToken);
        return Ok($"Simulation profile updated for {type}");
    }

    /// <summary>
    /// Gets currently active simulations.
    /// </summary>
    [HttpGet("active")]
    public IActionResult GetActiveSimulations()
    {
        var active = _simulationService.GetActiveSimulations();
        return Ok(active);
    }
}






