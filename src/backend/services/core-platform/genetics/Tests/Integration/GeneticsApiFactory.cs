using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Harvestry.Genetics.API;
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Harvestry.Genetics.Tests.Integration;

/// <summary>
/// Factory for integration tests using mocked services
/// </summary>
public sealed class GeneticsApiFactory : WebApplicationFactory<Program>
{
    public Mock<IGeneticsManagementService> GeneticsServiceMock { get; } = new(MockBehavior.Strict);
    public Mock<IBatchLifecycleService> BatchLifecycleServiceMock { get; } = new(MockBehavior.Strict);
    public Mock<IBatchStageConfigurationService> BatchStageConfigurationServiceMock { get; } = new(MockBehavior.Strict);

    public GeneticsApiFactory()
    {
        GeneticsServiceMock
            .Setup(service => service.GetGeneticsBySiteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<GeneticsResponse>());

        GeneticsServiceMock
            .Setup(service => service.GetGeneticsByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeneticsResponse?)null);

        GeneticsServiceMock
            .Setup(service => service.GetStrainsBySiteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<StrainResponse>());

        GeneticsServiceMock
            .Setup(service => service.GetStrainsByGeneticsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<StrainResponse>());

        BatchLifecycleServiceMock
            .Setup(service => service.GetActiveBatchesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<BatchResponse>());

        BatchStageConfigurationServiceMock
            .Setup(service => service.GetActiveStagesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<BatchStageResponse>());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:GeneticsDb"] =
                    "Host=localhost;Username=postgres;Password=postgres;Database=genetics_test"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IGeneticsManagementService));
            services.RemoveAll(typeof(IBatchLifecycleService));
            services.RemoveAll(typeof(IBatchStageConfigurationService));
            services.AddSingleton<IGeneticsManagementService>(_ => GeneticsServiceMock.Object);
            services.AddSingleton<IBatchLifecycleService>(_ => BatchLifecycleServiceMock.Object);
            services.AddSingleton<IBatchStageConfigurationService>(_ => BatchStageConfigurationServiceMock.Object);
        });
    }
}
