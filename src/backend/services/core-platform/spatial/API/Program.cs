using FluentValidation;
using FluentValidation.AspNetCore;
using Harvestry.Spatial.API.Middleware;
using Harvestry.Spatial.API.Validators;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.Services;
using Harvestry.Spatial.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Harvestry Spatial API",
        Version = "v1"
    });
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateRoomRequestValidator>();

builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return SpatialDataSourceFactory.CreateFromConfiguration(configuration, "Spatial");
});

builder.Services.AddScoped<SpatialDbContext>();
builder.Services.AddSingleton<IRlsContextAccessor, AsyncLocalRlsContextAccessor>();

builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IInventoryLocationRepository, InventoryLocationRepository>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<IEquipmentChannelRepository, EquipmentChannelRepository>();
builder.Services.AddScoped<ICalibrationRepository, EquipmentCalibrationRepository>();
builder.Services.AddScoped<IValveZoneMappingRepository, ValveZoneMappingRepository>();

builder.Services.AddScoped<ISpatialHierarchyService, SpatialHierarchyService>();
builder.Services.AddScoped<IEquipmentRegistryService, EquipmentRegistryService>();
builder.Services.AddScoped<ICalibrationService, CalibrationService>();
builder.Services.AddScoped<IValveZoneMappingService, ValveZoneMappingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RlsContextMiddleware>();
app.UseRouting();
app.MapControllers();

app.Run();
