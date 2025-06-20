using Articles.Dal.PostgresEfCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Dal.PostgresEfCore.Tests.Unit;

[TestClass]
public class DbInitiateTestProfileBase
{
    protected static IServiceProvider ServiceProvider = null!;

    [AssemblyInitialize]
    public static void Init(TestContext context)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("testconfig.localhost.json")
            .Build();
        
        var services = new ServiceCollection();
        services.AddArticlesPgServices(configuration);

        ServiceProvider = services.BuildServiceProvider();
    }
    
    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static async Task Initialize(TestContext context)
    {
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