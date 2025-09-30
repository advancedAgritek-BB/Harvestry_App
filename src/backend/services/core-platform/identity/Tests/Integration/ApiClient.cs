using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Harvestry.Identity.Tests.Integration;

internal sealed class ApiClient : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    public HttpClient Client { get; }

    public static async Task<ApiClient> CreateAsync()
    {
        var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        return new ApiClient(factory, client);
    }

    private ApiClient(WebApplicationFactory<Program> factory, HttpClient client)
    {
        _factory = factory;
        Client = client;
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
    }
}
