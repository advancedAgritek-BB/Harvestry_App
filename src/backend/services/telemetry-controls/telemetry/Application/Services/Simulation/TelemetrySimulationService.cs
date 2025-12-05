using System.Collections.Concurrent;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Domain.Models.Simulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.Application.Services.Simulation;

public class TelemetrySimulationService : ITelemetrySimulationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelemetrySimulationService> _logger;
    private readonly TimeProvider _timeProvider;
    
    private readonly ConcurrentDictionary<Guid, SimulationState> _activeSimulations = new();
    private readonly ConcurrentDictionary<StreamType, SimulationProfile> _profiles = new();

    public TelemetrySimulationService(
        IServiceScopeFactory scopeFactory,
        ILogger<TelemetrySimulationService> logger,
        TimeProvider? timeProvider = null)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
        
        InitializeDefaultProfiles();
    }

    private void InitializeDefaultProfiles()
    {
        _profiles[StreamType.Temperature] = new SimulationProfile(SimulationBehavior.SineWave24H, 65, 85, 0.5);
        _profiles[StreamType.Humidity] = new SimulationProfile(SimulationBehavior.InverseSineWave24H, 40, 60, 2.0);
        _profiles[StreamType.Co2] = new SimulationProfile(SimulationBehavior.StaticWithNoise, 800, 1200, 50);
        _profiles[StreamType.Ph] = new SimulationProfile(SimulationBehavior.RandomWalk, 5.5, 6.5, 0.1) { Volatility = 0.05 };
        _profiles[StreamType.Ec] = new SimulationProfile(SimulationBehavior.RandomWalk, 1.8, 2.5, 0.1) { Volatility = 0.05 };
        _profiles[StreamType.WaterLevel] = new SimulationProfile(SimulationBehavior.Sawtooth, 0, 100, 1.0);
        _profiles[StreamType.LightPpfd] = new SimulationProfile(SimulationBehavior.SineWave24H, 0, 1200, 20);
    }

    public async Task StartSimulationAsync(StreamType streamType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting simulation for all streams of type {StreamType}", streamType);
        
        // Note: Ideally we would query all streams of this type.
        // For now, we rely on individual toggling or future expansion.
        _logger.LogWarning("StartSimulation(StreamType) logic pending Repo update for GetByType. Use ToggleSimulation for specific streams.");
        
        if (!_profiles.ContainsKey(streamType))
        {
            _profiles[streamType] = new SimulationProfile(SimulationBehavior.StaticWithNoise, 0, 100, 1);
        }
        
        await Task.CompletedTask;
    }

    public Task StopSimulationAsync(StreamType streamType, CancellationToken cancellationToken = default)
    {
        foreach (var kvp in _activeSimulations)
        {
            if (kvp.Value.Stream.StreamType == streamType)
            {
                _activeSimulations.TryRemove(kvp.Key, out _);
            }
        }
        return Task.CompletedTask;
    }

    public async Task ToggleSimulationAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        if (_activeSimulations.ContainsKey(streamId))
        {
            _activeSimulations.TryRemove(streamId, out _);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var streamRepository = scope.ServiceProvider.GetRequiredService<ISensorStreamRepository>();

        var stream = await streamRepository.GetByIdAsync(streamId, cancellationToken);
        if (stream == null) 
        {
            _logger.LogWarning("Stream {StreamId} not found for simulation", streamId);
            return;
        }

        if (!_profiles.TryGetValue(stream.StreamType, out var profile))
        {
            profile = new SimulationProfile(SimulationBehavior.StaticWithNoise, 0, 100, 1);
        }

        var state = new SimulationState(stream, profile);
        _activeSimulations.TryAdd(streamId, state);
    }

    public Task UpdateProfileAsync(StreamType streamType, SimulationProfile profile, CancellationToken cancellationToken = default)
    {
        _profiles[streamType] = profile;
        // Update active
        foreach (var state in _activeSimulations.Values.Where(s => s.Stream.StreamType == streamType))
        {
            state.Profile = profile;
        }
        return Task.CompletedTask;
    }

    public IEnumerable<SimulationState> GetActiveSimulations()
    {
        return _activeSimulations.Values;
    }

    public async Task GenerateAndIngestAsync(CancellationToken cancellationToken = default)
    {
        if (_activeSimulations.IsEmpty) return;

        var now = _timeProvider.GetUtcNow();
        var readingsByEquipment = new Dictionary<(Guid SiteId, Guid EquipmentId), List<SensorReadingDto>>();

        foreach (var state in _activeSimulations.Values)
        {
            var value = CalculateValue(state, now);
            state.LastValue = value; // Update state

            var key = (state.Stream.SiteId, state.Stream.EquipmentId);
            if (!readingsByEquipment.ContainsKey(key))
            {
                readingsByEquipment[key] = new List<SensorReadingDto>();
            }

            readingsByEquipment[key].Add(new SensorReadingDto(
                state.StreamId,
                now,
                value,
                state.Stream.Unit,
                now
            ));
        }

        using var scope = _scopeFactory.CreateScope();
        var ingestService = scope.ServiceProvider.GetRequiredService<ITelemetryIngestService>();

        foreach (var group in readingsByEquipment)
        {
            var (siteId, equipmentId) = group.Key;
            var readings = group.Value;

            var request = new IngestTelemetryRequestDto(
                siteId,
                equipmentId,
                IngestionProtocol.Simulation,
                readings
            );

            try 
            {
                await ingestService.IngestBatchAsync(siteId, request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ingesting simulation batch for equipment {EquipmentId}", equipmentId);
            }
        }
    }

    private double CalculateValue(SimulationState state, DateTimeOffset now)
    {
        var profile = state.Profile;
        var random = Random.Shared;
        double noise = (random.NextDouble() * 2 - 1) * profile.Noise; // +/- Noise

        switch (profile.Behavior)
        {
            case SimulationBehavior.SineWave24H:
                // Peak at 2 PM (14:00), Trough at 2 AM (02:00)
                // Period = 24h
                // Shift: Max at 14. Sin(PI/2)=1. (14-Shift)/24 * 2PI = PI/2 => Shift=8.
                double hours = now.TimeOfDay.TotalHours;
                return profile.MidPoint + profile.Amplitude * Math.Sin(2 * Math.PI * (hours - 8) / 24) + noise;

            case SimulationBehavior.InverseSineWave24H:
                hours = now.TimeOfDay.TotalHours;
                return profile.MidPoint - profile.Amplitude * Math.Sin(2 * Math.PI * (hours - 8) / 24) + noise;

            case SimulationBehavior.RandomWalk:
                // LastValue +/- Volatility
                double change = (random.NextDouble() * 2 - 1) * profile.Volatility;
                double next = state.LastValue + change;
                // Clamp
                if (next > profile.Max) next = profile.Max;
                if (next < profile.Min) next = profile.Min;
                return next + noise;

            case SimulationBehavior.Sawtooth:
                // Rise
                state.LastValue += profile.Volatility;
                if (state.LastValue >= profile.Max) state.LastValue = profile.Min;
                return state.LastValue + noise;

            case SimulationBehavior.StaticWithNoise:
            default:
                return profile.MidPoint + noise;
        }
    }
}
