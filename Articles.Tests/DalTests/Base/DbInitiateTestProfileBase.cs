using Articles.Dal.PostgresEfCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Tests.DalTests.Base;

[TestClass]
public class DbInitiateTestProfileBase
{
    protected static IServiceProvider ServiceProvider = null!;
    
    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static async Task Initialize(TestContext context)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("testconfig.localhost.json")
            .Build();
        
        var services = new ServiceCollection();
        services.AddArticlesPgServices(configuration);
        
        ServiceProvider = services.BuildServiceProvider();
        
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ArticlesDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    protected static async Task WithNewScope<T>(Func<T, Task> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<T>();
        await action(service);
    }
    
    protected static async Task WithNewScopedDbContext(Func<ArticlesDbContext, Task> action)
    {
        await WithNewScope(action);
    }
}