
using Articles.Dal.PostgresEfCore.Mapping;
using Articles.Services.Impl.Mapping;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Tests;

[TestClass]
public static class TestAssemblyInitializer
{
    [AssemblyInitialize]
    public static void Init(TestContext context)
    {
        var services = new ServiceCollection();
        
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(ServicesMapsterConfig).Assembly);
        config.Scan(typeof(DalMapsterConfig).Assembly);
        
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        typeAdapterConfig.Scan(typeof(ServicesMapsterConfig).Assembly);
        typeAdapterConfig.Scan(typeof(DalMapsterConfig).Assembly);
        
        services.AddSingleton(typeAdapterConfig);
        
        var serviceProvider = services.BuildServiceProvider();
    }
}