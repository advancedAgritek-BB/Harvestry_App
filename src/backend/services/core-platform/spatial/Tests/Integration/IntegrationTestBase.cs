using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.Services;
using Harvestry.Spatial.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Xunit;

namespace Harvestry.Spatial.Tests.Integration;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly IServiceCollection _services = new ServiceCollection();
    private ILoggerFactory? _loggerFactory;
    private NpgsqlTransaction? _transaction;
    private NpgsqlDataSource? _dataSource;

    protected IServiceProvider ServiceProvider = null!;

    public virtual async Task InitializeAsync()
    {
        var configuration = BuildConfiguration();
        ConfigureServices(configuration);
        ServiceProvider = _services.BuildServiceProvider();

        var dbContext = ServiceProvider.GetRequiredService<SpatialDbContext>();
        _transaction = await dbContext.BeginTransactionAsync(IsolationLevel.ReadCommitted, CancellationToken.None);

        await SpatialTestDataSeeder.SeedAsync(ServiceProvider, _transaction, CancellationToken.None);
    }

    public virtual async Task DisposeAsync()
    {
        // Dispose all resources with individual try-catch to ensure all cleanup attempts are made
        if (_transaction != null)
        {
            try
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing transaction: {ex.Message}");
            }
        }

        if (_dataSource != null)
        {
            try
            {
                await _dataSource.DisposeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing data source: {ex.Message}");
            }
        }

        if (ServiceProvider is IAsyncDisposable asyncDisposable)
        {
            try
            {
                await asyncDisposable.DisposeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing service provider: {ex.Message}");
            }
        }
        else if (ServiceProvider is IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing service provider: {ex.Message}");
            }
        }

        if (_loggerFactory != null)
        {
            try
            {
                _loggerFactory.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing logger factory: {ex.Message}");
            }
        }
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
        if (!IntegrationTestEnvironment.TryGetConnectionString(configuration, out var connectionString))
        {
            throw new InvalidOperationException("Spatial integration test connection string not configured.");
        }

        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        _services.AddSingleton<ILoggerFactory>(_loggerFactory);
        _services.AddLogging();
        _services.AddSingleton(configuration);

        _dataSource = SpatialDataSourceFactory.Create(connectionString);
        _services.AddSingleton(_dataSource);
        _services.AddScoped(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<SpatialDbContext>>();
            var ds = sp.GetRequiredService<NpgsqlDataSource>();
            return new SpatialDbContext(ds, logger);
        });

        _services.AddScoped<IRlsContextAccessor, AsyncLocalRlsContextAccessor>();
        _services.AddScoped<IRoomRepository, RoomRepository>();
        _services.AddScoped<IInventoryLocationRepository, InventoryLocationRepository>();
        _services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        _services.AddScoped<IEquipmentChannelRepository, EquipmentChannelRepository>();
        _services.AddScoped<ICalibrationRepository, EquipmentCalibrationRepository>();
        _services.AddScoped<IValveZoneMappingRepository, ValveZoneMappingRepository>();

        _services.AddScoped<ISpatialHierarchyService, SpatialHierarchyService>();
        _services.AddScoped<IEquipmentRegistryService, EquipmentRegistryService>();
        _services.AddScoped<IValveZoneMappingService, ValveZoneMappingService>();
        _services.AddScoped<ICalibrationService, CalibrationService>();
    }

    protected IRlsContextAccessor GetRlsAccessor() => ServiceProvider.GetRequiredService<IRlsContextAccessor>();

    protected void SetUserContext(Guid userId, string role, Guid? siteId)
    {
        GetRlsAccessor().Set(new RlsContext(userId, role, siteId));
    }
}
