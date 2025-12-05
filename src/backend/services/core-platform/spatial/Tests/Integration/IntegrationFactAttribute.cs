using System;
using Xunit;

namespace Harvestry.Spatial.Tests.Integration;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class IntegrationFactAttribute : FactAttribute
{
    public IntegrationFactAttribute()
    {
        if (!IntegrationTestEnvironment.TryGetConnectionString(configuration: null, out _))
        {
            Skip = "Spatial integration test connection string not configured.";
        }
    }
}
