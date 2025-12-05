using FluentValidation;
using FluentValidation.AspNetCore;
using Harvestry.Tasks.Infrastructure;
using Harvestry.Tasks.API.Middleware;
using Harvestry.Shared.Observability;
using Harvestry.Shared.Observability.Tracing;
using HealthChecks.NpgSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "X-User-Id";
    options.DefaultChallengeScheme = "X-User-Id";
})
.AddScheme<AuthenticationSchemeOptions, UserIdAuthenticationHandler>("X-User-Id", null);

builder.Services.AddAuthorization();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Harvestry.Tasks.API.Validators.CreateTaskRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Harvestry Tasks API",
        Version = "v1",
        Description = "Task workflow, conversations, and messaging endpoints"
    });
});

builder.Services.AddTasksInfrastructure(builder.Configuration);

// OpenTelemetry instrumentation
builder.Services.AddHarvestryOpenTelemetry(
    builder.Configuration,
    serviceName: "Harvestry.Tasks",
    serviceVersion: "1.0.0");

// Health checks
var tasksConnStr = builder.Configuration.GetConnectionString("TasksDb")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=harvestry_tasks;Username=postgres;Password=postgres";
builder.Services.AddHealthChecks()
    .AddNpgSql(tasksConnStr, name: "database", tags: new[] { "db", "ready" })
    .AddCheck("startup", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Tasks service is running"), tags: new[] { "live" });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

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
