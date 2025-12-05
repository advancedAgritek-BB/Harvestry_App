using AutoMapper;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.ValueObjects;

namespace Harvestry.Telemetry.Application.Mappers;

/// <summary>
/// AutoMapper profile for telemetry entity and DTO mappings.
/// </summary>
public class TelemetryMappingProfile : Profile
{
    public TelemetryMappingProfile()
    {
        ConfigureSensorStreamMappings();
        ConfigureSensorReadingMappings();
        ConfigureAlertRuleMappings();
        ConfigureAlertInstanceMappings();
        ConfigureIngestionErrorMappings();
    }
    
    private void ConfigureSensorStreamMappings()
    {
        // SensorStream -> SensorStreamDto
        CreateMap<SensorStream, SensorStreamDto>();
        
        // CreateSensorStreamRequestDto -> SensorStream (handled by factory method)
        // UpdateSensorStreamRequestDto doesn't map directly (uses entity methods)
    }
    
    private void ConfigureSensorReadingMappings()
    {
        // SensorReading -> TelemetryReadingResponseDto
        CreateMap<SensorReading, TelemetryReadingResponseDto>()
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.Time))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.QualityCode, opt => opt.MapFrom(src => src.QualityCode));
        
        // SensorReading -> LatestReadingDto
        CreateMap<SensorReading, LatestReadingDto>()
            .ForMember(dest => dest.StreamId, opt => opt.MapFrom(src => src.StreamId))
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.Time))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.QualityCode, opt => opt.MapFrom(src => src.QualityCode))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => DateTimeOffset.UtcNow - src.Time));
    }
    
    private void ConfigureAlertRuleMappings()
    {
        // AlertRule -> AlertRuleDto
        CreateMap<AlertRule, AlertRuleDto>()
            .ForMember(dest => dest.StreamIds, opt => opt.MapFrom(src => src.StreamIds))
            .ForMember(dest => dest.NotifyChannels, opt => opt.MapFrom(src => src.NotifyChannels));
        
        // CreateAlertRuleRequestDto -> AlertRule (handled by factory method)
        // UpdateAlertRuleRequestDto doesn't map directly (uses entity methods)
    }
    
    private void ConfigureAlertInstanceMappings()
    {
        // AlertInstance -> AlertInstanceDto
        CreateMap<AlertInstance, AlertInstanceDto>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive()))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.GetDuration(null)));
    }
    
    private void ConfigureIngestionErrorMappings()
    {
        // IngestionError -> IngestionErrorDto
        CreateMap<IngestionError, IngestionErrorDto>();
    }
}

