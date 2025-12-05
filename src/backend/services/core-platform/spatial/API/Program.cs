using FluentValidation;
using FluentValidation.AspNetCore;
using Harvestry.Spatial.API.Middleware;
using Harvestry.Spatial.API.Validators;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.Services;
using Harvestry.Spatial.Infrastructure.Persistence;
using Harvestry.Shared.Authentication;
using Harvestry.Shared.Observability;
using Harvestry.Shared.Observability.Tracing;
using HealthChecks.NpgSql;
using Microsoft.AspNetCore.Authorization;
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

// ===== Authentication & Authorization =====
var supabaseConfigured = builder.Configuration.GetSection("Supabase").Exists() 
    && !string.IsNullOrWhiteSpace(builder.Configuration["Supabase:JwtSecret"]);

if (supabaseConfigured)
{
    builder.Services.AddSupabaseJwtAuthentication(builder.Configuration);
}
else if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication("Header")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, 
            Harvestry.Shared.Authentication.HeaderAuthenticationHandler>("Header", null);
}
else
{
    throw new InvalidOperationException(
        "Supabase authentication must be configured in production.");
}

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// OpenTelemetry instrumentation
builder.Services.AddHarvestryOpenTelemetry(
    builder.Configuration,
    serviceName: "Harvestry.Spatial",
    serviceVersion: "1.0.0");

// Health checks
var spatialConnStr = builder.Configuration.GetConnectionString("Spatial")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found");
builder.Services.AddHealthChecks()
    .AddNpgSql(spatialConnStr, name: "database", tags: new[] { "db", "ready" })
    .AddCheck("startup", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Spatial service is running"), tags: new[] { "live" });

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

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RlsContextMiddleware>();
app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();
