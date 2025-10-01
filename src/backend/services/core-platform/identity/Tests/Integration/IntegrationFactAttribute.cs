using Xunit;

namespace Harvestry.Identity.Tests.Integration;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class IntegrationFactAttribute : FactAttribute
{
    public IntegrationFactAttribute()
    {
        if (!IntegrationTestEnvironment.TryGetConnectionString(configuration: null, out _))
        {
            Skip = "Integration test connection string not configured.";
        }
    }
}
