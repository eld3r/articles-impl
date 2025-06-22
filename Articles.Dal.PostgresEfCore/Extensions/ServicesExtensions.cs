using Articles.Dal.PostgresEfCore.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Articles.Dal.PostgresEfCore.Extensions;

public static class ServicesExtensions
{
    public static IServiceCollection AddArticlesPgServices(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddDbContext<ArticlesDbContext>((prov, options) =>
                options
                    .UseNpgsql(configuration.GetConnectionString("postgres"))
                    .UseSnakeCaseNamingConvention()
                    .AddInterceptors(new CreateUpdateDateInterceptor())
#if DEBUG
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
#endif
                )
            .AddScoped<IArticlesRepository, ArticlesRepository>()
            .AddScoped<ISectionsRepository, SectionsRepository>()
            .AddScoped<ITagRepository, TagsRepository>()
            .AddMapster()
        ;    
    public static IServiceCollection AddMapster(this IServiceCollection services)
    {
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        typeAdapterConfig.Scan(typeof(ServicesExtensions).Assembly);
        
        services.AddSingleton(typeAdapterConfig);
        return services;
    }
}