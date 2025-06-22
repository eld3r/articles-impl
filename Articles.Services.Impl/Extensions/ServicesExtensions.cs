using Articles.Services;
using Articles.Services.Impl;
using Articles.Services.Impl.Mapping;
using Mapster;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServicesExtensions
{
    public static IServiceCollection AddArticlesServices(this IServiceCollection services) =>
        services
            .AddScoped<IArticlesService, ArticlesService>()
            .AddScoped<ISectionsService, SectionsService>()
        ;
}