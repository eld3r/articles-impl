using Articles.Dal.PostgresEfCore.Mapping;
using Articles.Services.Impl.Mapping;
using Mapster;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServicesExtensions
{
    public static IServiceCollection AddMapster(this IServiceCollection services)
    {
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        typeAdapterConfig.Scan(typeof(ServicesMapsterConfig).Assembly);
        typeAdapterConfig.Scan(typeof(DalMapsterConfig).Assembly);
        services.AddSingleton(typeAdapterConfig);
        return services;
    }
}