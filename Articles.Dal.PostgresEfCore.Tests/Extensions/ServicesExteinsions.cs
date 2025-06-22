using Articles.Dal.PostgresEfCore.Mapping;
using Articles.Services.Impl.Mapping;
using Mapster;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServicesExteinsions
{
    public static IServiceCollection AddMapster(this IServiceCollection services)
    {
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        services.AddSingleton(typeAdapterConfig);
        return services;
    }
}