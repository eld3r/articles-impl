using Microsoft.Extensions.DependencyInjection;

namespace Articles.Dal.PostgresEfCore.Tests.ServicesTests;

[TestClass]
public class ServicesTestsBase
{
    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void ClassInitialize(TestContext testContext)
    {
        var services = new ServiceCollection();
        services.AddArticlesServices();
        services.AddMapster();
    }
}