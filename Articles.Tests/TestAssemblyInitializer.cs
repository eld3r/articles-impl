
using Articles.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Tests;

public static class TestAssemblyInitializer
{
    [AssemblyInitialize]
    public static void Init(TestContext context)
    {
        var services = new ServiceCollection();
        services.AddMapster();

        var serviceProvider = services.BuildServiceProvider();
    }
}