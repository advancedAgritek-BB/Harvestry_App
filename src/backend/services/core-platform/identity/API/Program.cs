using System;
using System.Linq;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Harvestry.Identity.API.Middleware;
using Harvestry.Identity.API.Security;
using Harvestry.Identity.API.Validators;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Application.Services;
using Harvestry.Identity.Infrastructure.External;
using Harvestry.Identity.Infrastructure.Health;
using Harvestry.Identity.Infrastructure.Jobs;
using Harvestry.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();

    if (!context.Configuration.GetSection("Serilog").Exists())
    {
        loggerConfiguration.WriteTo.Console(new JsonFormatter());
    }
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Title = "Validation failed",
            Detail = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        return new BadRequestObjectResult(problemDetails);
    };
});

builder.Services.AddProblemDetails();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<BadgeLoginRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

builder.Services.AddAuthorization();

builder.Services.AddAuthentication("Header")
    .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>("Header", _ => { });

var rateLimitSettings = ResolveRateLimitSettings(builder.Configuration);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Title = "Rate limit exceeded",
            Detail = "Too many requests. Please retry later.",
            Status = StatusCodes.Status429TooManyRequests,
            Instance = context.HttpContext.Request.Path
        };

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            problem.Extensions["retryAfter"] = retryAfter.TotalSeconds;
        }

        await context.HttpContext.Response
            .WriteAsJsonAsync(problem, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    };

    options.AddFixedWindowLimiter("badge-login", limiterOptions =>
    {
        limiterOptions.PermitLimit = rateLimitSettings.PermitLimit;
        limiterOptions.Window = rateLimitSettings.Window;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = rateLimitSettings.QueueLimit;
    });
});

var corsSettings = ResolveCorsSettings(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("IdentityCors", policy =>
    {
        if (corsSettings.AllowedOrigins.Length > 0)
        {
            policy.WithOrigins(corsSettings.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();

            if (corsSettings.AllowCredentials)
            {
                policy.AllowCredentials();
            }
        }
        else
        {
            policy.SetIsOriginAllowed(_ => false);
        }
    });
});

builder.Services.AddSingleton<IRlsContextAccessor, AsyncLocalRlsContextAccessor>();

builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Identity")
        ?? configuration["Identity:ConnectionString"]
        ?? Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("PostgreSQL connection string for Identity service was not provided.");
    }

    return IdentityDataSourceFactory.Create(connectionString);
});

builder.Services.AddScoped(sp =>
{
    var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
    var logger = sp.GetRequiredService<ILogger<IdentityDbContext>>();
    return new IdentityDbContext(dataSource, logger);
});

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBadgeRepository, BadgeRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ISiteRepository, SiteRepository>();
builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();
builder.Services.AddScoped<ITwoPersonApprovalRepository, TwoPersonApprovalRepository>();
builder.Services.AddScoped<IAuthorizationAuditRepository, AuthorizationAuditRepository>();

// Application services
builder.Services.AddScoped<IPolicyEvaluationService, PolicyEvaluationService>();
builder.Services.AddScoped<IBadgeAuthService, BadgeAuthService>();
builder.Services.AddScoped<ITaskGatingService, TaskGatingService>();
builder.Services.AddSingleton<INotificationService, LoggingNotificationService>();

// Background jobs
builder.Services.AddHostedService<AuditChainVerificationJob>();
builder.Services.AddHostedService<SessionCleanupJob>();
builder.Services.AddHostedService<BadgeExpirationNotificationJob>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("IdentityCors");

if (rateLimitSettings.Enabled)
{
    app.UseRateLimiter();
}

app.UseAuthentication();
app.UseMiddleware<RlsContextMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();

static (bool Enabled, int PermitLimit, TimeSpan Window, int QueueLimit) ResolveRateLimitSettings(IConfiguration configuration)
{
    var section = configuration.GetSection("RateLimiting");
    var enabled = section.Exists() ? section.GetValue<bool?>("Enabled") ?? true : true;
    var permitLimit = section.GetValue<int?>("PermitLimit") ?? 10;
    var queueLimit = section.GetValue<int?>("QueueLimit") ?? 5;
    var window = section.GetValue<TimeSpan?>("Window") ?? TimeSpan.FromMinutes(1);

    if (permitLimit <= 0)
    {
        permitLimit = 10;
    }

    if (queueLimit < 0)
    {
        queueLimit = 0;
    }

    if (window <= TimeSpan.Zero)
    {
        window = TimeSpan.FromMinutes(1);
    }

    return (enabled, permitLimit, window, queueLimit);
}

static (string[] AllowedOrigins, bool AllowCredentials) ResolveCorsSettings(IConfiguration configuration)
{
    var corsSection = configuration.GetSection("Identity:Cors");
    if (!corsSection.Exists())
    {
        corsSection = configuration.GetSection("Security:CORS");
    }

    var allowedOrigins = corsSection.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    var allowCredentials = corsSection.GetValue<bool?>("AllowCredentials") ?? false;

    allowedOrigins = allowedOrigins
        .Where(origin => !string.IsNullOrWhiteSpace(origin))
        .Select(origin => origin.Trim())
        .ToArray();

    return (allowedOrigins, allowCredentials);
}

public partial class Program
{ }
