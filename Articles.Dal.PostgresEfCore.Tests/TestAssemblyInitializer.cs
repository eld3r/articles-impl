
using Microsoft.Extensions.DependencyInjection;

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