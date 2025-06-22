using Articles.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Tests.ServicesTests.Base;

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