using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Articles.Services.Impl.Tests;

public static class TestHostBuilder 
{
    public static IHost CreateTestHost() =>
        Host.CreateDefaultBuilder(null)
            .ConfigureServices((context, services) =>
            {
                services.AddArticlesServices();
            }).Build();
}