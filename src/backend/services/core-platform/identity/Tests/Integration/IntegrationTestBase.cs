using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Application.Services;
using Harvestry.Identity.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Xunit;

namespace Harvestry.Identity.Tests.Integration;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly IServiceCollection _services = new ServiceCollection();
    private ILoggerFactory? _loggerFactory;
    protected IServiceProvider ServiceProvider = null!;
    protected readonly Guid CurrentUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public virtual async Task InitializeAsync()
    {
        var configuration = BuildConfiguration();
        ConfigureServices(configuration);
        ServiceProvider = _services.BuildServiceProvider();

        await TestDataSeeder.SeedAsync(ServiceProvider, CancellationToken.None);
    }

    public virtual async Task DisposeAsync()
    {
        if (ServiceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _loggerFactory?.Dispose();
    }

    protected virtual IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    protected virtual void ConfigureServices(IConfiguration configuration)
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        _services.AddSingleton<ILoggerFactory>(_loggerFactory);
        _services.AddLogging();

        _services.AddSingleton<IConfiguration>(configuration);

        var connectionString = configuration.GetConnectionString("Identity")
            ?? configuration["Identity:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION")
            ?? throw new InvalidOperationException("Integration test connection string not configured.");

        var dataSource = IdentityDataSourceFactory.Create(connectionString);
        _services.AddSingleton(dataSource);
        _services.AddScoped(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<IdentityDbContext>>();
            return new IdentityDbContext(dataSource, logger);
        });
        // Note: NpgsqlConnection registration removed - use IdentityDbContext.GetOpenConnectionAsync() instead

        _services.AddScoped<IRlsContextAccessor, AsyncLocalRlsContextAccessor>();
        _services.AddScoped<IUserRepository, UserRepository>();
        _services.AddScoped<IBadgeRepository, BadgeRepository>();
        _services.AddScoped<ISessionRepository, SessionRepository>();
        _services.AddScoped<IRoleRepository, RoleRepository>();
        _services.AddScoped<ISiteRepository, SiteRepository>();
        _services.AddScoped<IDatabaseRepository, DatabaseRepository>();
        _services.AddScoped<ITwoPersonApprovalRepository, TwoPersonApprovalRepository>();
        _services.AddScoped<IAuthorizationAuditRepository, AuthorizationAuditRepository>();
        _services.AddScoped<IPolicyEvaluationService, PolicyEvaluationService>();
        _services.AddScoped<IBadgeAuthService, BadgeAuthService>();
        _services.AddScoped<ITaskGatingService, TaskGatingService>();
    }

    protected IRlsContextAccessor GetRlsAccessor() => ServiceProvider.GetRequiredService<IRlsContextAccessor>();

    protected void SetUserContext(Guid userId, string role, Guid? siteId)
    {
        GetRlsAccessor().Set(new RlsContext(userId, role, siteId));
    }

    protected async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = ServiceProvider.GetRequiredService<IdentityDbContext>();
        return await dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
    }

    protected async Task ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default)
    {
        await using var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
