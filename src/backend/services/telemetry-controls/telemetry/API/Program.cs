using FluentValidation;
using FluentValidation.AspNetCore;
using Harvestry.Telemetry.API.Hubs;
using Harvestry.Telemetry.API.Middleware;
using Harvestry.Telemetry.API.Realtime;
using Harvestry.Telemetry.API.Validators;
using Harvestry.Telemetry.Application.DeviceAdapters;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Application.Mappers;
using Harvestry.Telemetry.Application.Services;
using Harvestry.Telemetry.Application.Services.Simulation;
using Harvestry.Telemetry.Infrastructure.Persistence;
using Harvestry.Telemetry.Infrastructure.Configuration;
using Harvestry.Telemetry.Infrastructure.Realtime;
using Harvestry.Telemetry.Infrastructure.Repositories;
using Harvestry.Telemetry.Infrastructure.Workers;
using System.Threading.RateLimiting;
using Harvestry.Shared.Observability;
using Harvestry.Shared.Observability.Tracing;
using HealthChecks.NpgSql;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<HttpIngestRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// OpenTelemetry instrumentation
builder.Services.AddHarvestryOpenTelemetry(
    builder.Configuration,
    serviceName: "Harvestry.Telemetry",
    serviceVersion: "1.0.0");

// Health checks
var telemetryConnStr = builder.Configuration.GetConnectionString("TelemetryDb")
    ?? throw new InvalidOperationException("Connection string 'TelemetryDb' not found");
builder.Services.AddHealthChecks()
    .AddNpgSql(telemetryConnStr, name: "database", tags: new[] { "db", "ready" })
    .AddCheck("startup", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Telemetry service is running"), tags: new[] { "live" });

// Rate limiting for telemetry ingest
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Sliding window rate limiter for ingest endpoint
    options.AddSlidingWindowLimiter("ingest", limiter =>
    {
        var config = builder.Configuration.GetSection("RateLimiting:Ingest");
        limiter.Window = TimeSpan.FromMinutes(config.GetValue<int>("WindowMinutes", 1));
        limiter.SegmentsPerWindow = config.GetValue<int>("Segments", 4);
        limiter.PermitLimit = config.GetValue<int>("PermitLimit", 10000);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = config.GetValue<int>("QueueLimit", 100);
    });
    
    // Token bucket for burst handling
    options.AddTokenBucketLimiter("burst", limiter =>
    {
        var config = builder.Configuration.GetSection("RateLimiting:Burst");
        limiter.TokenLimit = config.GetValue<int>("TokenLimit", 1000);
        limiter.ReplenishmentPeriod = TimeSpan.FromSeconds(config.GetValue<int>("ReplenishSeconds", 1));
        limiter.TokensPerPeriod = config.GetValue<int>("TokensPerPeriod", 100);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = config.GetValue<int>("QueueLimit", 50);
    });
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Rate limit exceeded",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra) ? (int)ra.TotalSeconds : 60
        }, token);
    };
});

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("TelemetryDb")
    ?? throw new InvalidOperationException("Connection string 'TelemetryDb' not found");

// Options binding
builder.Services.Configure<TelemetryMqttOptions>(builder.Configuration.GetSection("Telemetry:Mqtt"));
builder.Services.Configure<TelemetryWalReplicationOptions>(builder.Configuration.GetSection("Telemetry:WalReplication"));
builder.Services.Configure<TelemetrySubscriptionMonitorOptions>(builder.Configuration.GetSection("Telemetry:Subscriptions"));
builder.Services.Configure<TelemetrySimulationOptions>(builder.Configuration.GetSection("Telemetry:Simulation"));
builder.Services.PostConfigure<TelemetryWalReplicationOptions>(options =>
{
    var connectionToUse = string.IsNullOrWhiteSpace(options.ConnectionString)
        ? connectionString
        : options.ConnectionString;

    var replicationBuilder = new NpgsqlConnectionStringBuilder(connectionToUse);
    replicationBuilder["Replication"] = "database";

    options.ConnectionString = replicationBuilder.ConnectionString;
});

// Configure NpgsqlDataSource (connection pooling)
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);

// Configure DbContext
builder.Services.AddScoped<ITelemetryRlsContextAccessor, AsyncLocalTelemetryRlsContextAccessor>();
builder.Services.AddScoped<ITelemetryConnectionFactory, TelemetryConnectionFactory>();
builder.Services.AddScoped<TelemetryConnectionInterceptor>();

builder.Services.AddDbContext<TelemetryDbContext>((serviceProvider, options) =>
{
    var npgsqlDataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
    options.UseNpgsql(npgsqlDataSource, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });

    options.AddInterceptors(serviceProvider.GetRequiredService<TelemetryConnectionInterceptor>());
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(TelemetryMappingProfile));

// Real-time infrastructure
builder.Services.AddSingleton<ITelemetrySubscriptionRegistry, TelemetrySubscriptionRegistry>();

// Application Services
builder.Services.AddSingleton<ITelemetrySimulationService, TelemetrySimulationService>();
builder.Services.AddScoped<ITelemetryIngestService, TelemetryIngestService>();
builder.Services.AddScoped<INormalizationService, NormalizationService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();
builder.Services.AddScoped<IAlertEvaluationService, AlertEvaluationService>();
builder.Services.AddScoped<IHttpIngestAdapter, HttpIngestAdapter>();
builder.Services.AddScoped<IMqttIngestAdapter, MqttIngestAdapter>();
builder.Services.AddScoped<ITelemetryRealtimeDispatcher, TelemetryRealtimeDispatcher>();
builder.Services.AddScoped<IIngestionSessionRepository, IngestionSessionRepository>();
builder.Services.AddScoped<IIngestionErrorRepository, IngestionErrorRepository>();
builder.Services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
builder.Services.AddScoped<IAlertInstanceRepository, AlertInstanceRepository>();
builder.Services.AddScoped<ITelemetryQueryRepository, TelemetryQueryRepository>();

// Repositories
builder.Services.AddScoped<ISensorStreamRepository, SensorStreamRepository>();
builder.Services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();

// Background workers
builder.Services.AddHostedService<AlertEvaluationWorker>();
builder.Services.AddHostedService<TelemetryRealtimePollingWorker>();
builder.Services.AddHostedService<RollupFreshnessMonitorWorker>();
builder.Services.AddHostedService<SessionCleanupWorker>();
builder.Services.AddHostedService<MqttTelemetryWorker>();
builder.Services.AddHostedService<WalFanoutWorker>();
builder.Services.AddHostedService<TelemetrySubscriptionMonitorWorker>();
builder.Services.AddHostedService<TelemetrySimulationWorker>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseMiddleware<TelemetryRlsContextMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHub<TelemetryHub>("/hubs/telemetry");

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
