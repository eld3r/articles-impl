using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Articles.Services.Impl.Tests;

[TestClass]
public class ArticlesServiceTests
{
    private static IHost Host = null!;
    [ClassInitialize]
    public static void InitializeTests(TestContext context)
    {
        Host = TestHostBuilder.CreateTestHost();
    }
    
    [TestMethod]
    public void CreateArticleTest()
    {
        var target = Host.Services.GetRequiredService<IArticlesService>();
        
        
    }
}