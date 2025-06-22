using Articles.Dal;
using Articles.Dal.PostgresEfCore;
using Articles.Dal.PostgresEfCore.Mapping;
using Articles.Dal.PostgresEfCore.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServicesExtensions
{
    public static IServiceCollection AddArticlesPgServices(this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddDbContext<ArticlesDbContext>((prov, options) =>
                    options
                        .UseNpgsql(configuration.GetConnectionString("postgres"))
                        .UseSnakeCaseNamingConvention()
                        .AddInterceptors(new CreateUpdateDateInterceptor())
#if DEBUG
                        //.LogTo(Console.WriteLine, LogLevel.Information)
                        //.EnableSensitiveDataLogging()
#endif
            )
            .AddScoped<IArticlesRepository, ArticlesRepository>()
            .AddScoped<ISectionsRepository, SectionsRepository>()
            .AddScoped<ITagsRepository, TagsRepository>()
        ;
}