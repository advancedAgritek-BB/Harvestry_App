using System.Security.Claims;
using FluentValidation;
using FluentValidation.AspNetCore;
using Harvestry.Genetics.API.Controllers;
using Harvestry.Genetics.API.Validators;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Application.Services;
using Harvestry.Genetics.Infrastructure.Middleware;
using Harvestry.Genetics.Infrastructure.Persistence;
using Harvestry.Shared.Authentication;
using Harvestry.Shared.Observability;
using Harvestry.Shared.Observability.Tracing;
using Microsoft.AspNetCore.Authorization;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ===== Configuration =====
var geneticsConnectionString = builder.Configuration.GetConnectionString("GeneticsDb") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not found");

// ===== Database Configuration =====
var npgsqlBuilder = new NpgsqlConnectionStringBuilder(geneticsConnectionString);
var disablePasswordProvider = builder.Configuration.GetValue<bool?>("Database:DisablePasswordProvider") ?? false;
var password = npgsqlBuilder.Password;
var dataSourceConnectionString = geneticsConnectionString;

if (!disablePasswordProvider && !string.IsNullOrEmpty(password))
{
    npgsqlBuilder.Password = null;
    dataSourceConnectionString = npgsqlBuilder.ConnectionString;
}

var dataSourceBuilder = new NpgsqlDataSourceBuilder(dataSourceConnectionString);
dataSourceBuilder.EnableDynamicJson();
if (!disablePasswordProvider && !string.IsNullOrEmpty(password))
{
    dataSourceBuilder.UsePeriodicPasswordProvider((_, _) => new ValueTask<string>(password), TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(30));
}

builder.Services.AddSingleton(dataSourceBuilder.Build());

// ===== OpenTelemetry Instrumentation =====
builder.Services.AddHarvestryOpenTelemetry(
    builder.Configuration,
    serviceName: "Harvestry.Genetics",
    serviceVersion: "1.0.0");

// ===== Core Infrastructure =====
builder.Services.AddScoped<GeneticsDbContext>();
builder.Services.AddScoped<IRlsContextAccessor, RlsContextAccessor>();

// ===== Repositories - Slice 1: Genetics =====
builder.Services.AddScoped<IGeneticsRepository, GeneticsRepository>();
builder.Services.AddScoped<IPhenotypeRepository, PhenotypeRepository>();
builder.Services.AddScoped<IStrainRepository, StrainRepository>();

// ===== Repositories - Slice 2: Batches =====
builder.Services.AddScoped<IBatchRepository, BatchRepository>();
builder.Services.AddScoped<IBatchEventRepository, BatchEventRepository>();
builder.Services.AddScoped<IBatchRelationshipRepository, BatchRelationshipRepository>();
builder.Services.AddScoped<IBatchStageDefinitionRepository, BatchStageDefinitionRepository>();
builder.Services.AddScoped<IBatchStageTransitionRepository, BatchStageTransitionRepository>();
builder.Services.AddScoped<IBatchStageHistoryRepository, BatchStageHistoryRepository>();
builder.Services.AddScoped<IBatchCodeRuleRepository, BatchCodeRuleRepository>();

builder.Services.AddScoped<IMotherPlantRepository, MotherPlantRepository>();
builder.Services.AddScoped<IMotherHealthLogRepository, MotherHealthLogRepository>();
builder.Services.AddScoped<IPropagationSettingsRepository, PropagationSettingsRepository>();
builder.Services.AddScoped<IPropagationOverrideRequestRepository, PropagationOverrideRequestRepository>();

// ===== Services - Slice 1: Genetics =====
builder.Services.AddScoped<IGeneticsManagementService, GeneticsManagementService>();

// ===== Services - Slice 2: Batches =====
builder.Services.AddScoped<IBatchLifecycleService, BatchLifecycleService>();
builder.Services.AddScoped<IBatchStageConfigurationService, BatchStageConfigurationService>();
builder.Services.AddScoped<IBatchCodeRuleService, BatchCodeRuleService>();

builder.Services.AddScoped<IMotherHealthService, MotherHealthService>();

// ===== Validators =====
builder.Services.AddValidatorsFromAssemblyContaining<CreateGeneticsRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// ===== Authentication & Authorization =====
// Use Supabase JWT in production, fallback to header auth in development
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

// ===== Controllers & API =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Harvestry Genetics API",
        Version = "v1",
        Description = "Genetics, Strains, and Batch Management API"
    });
});

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("GeneticsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                             ?? Array.Empty<string>();
        
        if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
        {
            // Development fallback only
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Production with no origins configured - deny all
            policy.WithOrigins("https://localhost:5001"); // Minimal safe default
        }
    });
});

// ===== Health Checks =====
builder.Services.AddHealthChecks()
    .AddNpgSql(geneticsConnectionString, name: "genetics-database", tags: new[] { "db", "genetics" });

// ===== Build Application =====
var app = builder.Build();

// ===== Middleware Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("GeneticsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// RLS Context Middleware - Sets PostgreSQL session variables for RLS policies
// Uses JWT claims from Supabase or header-based auth for development
app.Use(async (context, next) =>
{
    var rlsAccessor = context.RequestServices.GetRequiredService<IRlsContextAccessor>();

    // Extract user context from authenticated principal using shared extensions
    var userId = context.User.GetUserId() ?? Guid.Empty;
    var role = context.User.GetRole();

    // Determine site scope (header preferred, route fallback)
    Guid? siteId = null;
    if (context.Request.Headers.TryGetValue("X-Site-Id", out var siteHeader) &&
        Guid.TryParse(siteHeader, out var headerSiteId))
    {
        siteId = headerSiteId;
    }
    else if (context.Request.RouteValues.TryGetValue("siteId", out var siteValue) &&
             Guid.TryParse(siteValue?.ToString(), out var routeSiteId))
    {
        siteId = routeSiteId;
    }

    rlsAccessor.Set(new RlsContext(userId, role, siteId));

    try
    {
        await next(context);
    }
    finally
    {
        rlsAccessor.Clear();
    }
});

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
