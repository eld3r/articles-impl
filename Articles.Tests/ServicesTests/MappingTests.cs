using Articles.Domain.Entities;
using Articles.Tests.Extensions;
using Articles.Tests.ServicesTests.Base;
using Mapster;

namespace Articles.Tests.ServicesTests;

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