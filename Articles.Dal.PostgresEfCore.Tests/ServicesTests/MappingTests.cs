using Articles.Domain.Entities;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Dal.PostgresEfCore.Tests.ServicesTests;

[TestClass]
public class MappingTests : ServicesTestsBase
{
    [TestMethod]
    public void MapStringToTag()
    {
        var testString = "teststring";
        var tag = testString.Adapt<Tag>();
        tag
            .ShouldNotBeNull()
            .PrintToConsole()
            .Name
                .ShouldBe(testString);
    }
}