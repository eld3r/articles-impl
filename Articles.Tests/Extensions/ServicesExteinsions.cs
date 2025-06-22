using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Tests.Extensions;

public static class ServicesExteinsions
{
    public static IServiceCollection AddMapster(this IServiceCollection services)
    {
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        services.AddSingleton(typeAdapterConfig);
        return services;
    }
}