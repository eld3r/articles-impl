using Articles.Dal;
using Articles.Domain.Entities;
using Articles.Services.Impl;
using Articles.Tests.Extensions;
using FakeItEasy;

namespace Articles.Tests.ServicesTests.Unit;

[TestClass]
[TestCategory("Unit")]
public class SectionsServiceTests
{
    private static ISectionsRepository _sectionsRepository = null!;

    [ClassInitialize]
    public static void Init(TestContext context)
    {
        _sectionsRepository = A.Fake<ISectionsRepository>();

        A.CallTo(() => _sectionsRepository.GetAllSections())
            .ReturnsLazily(() =>
            [
                new Section
                {
                    Id = 1, Name = "tag;tag1", 
                    ArticlesCount = 3,
                    Tags = new List<string>() { "tag", "tag1" }.Select(s => new Tag() { Name = s }).ToList()
                },
                new Section
                {
                    Id = 1, Name = "tag;tag2", 
                    ArticlesCount = 4,
                    Tags = new List<string>() { "tag", "tag2" }.Select(s => new Tag() { Name = s }).ToList()
                }
            ]);

        A.CallTo(() => _sectionsRepository.GetSectionById(A<long>._))
            .ReturnsLazily((long id) => new Section
            {
                Id = id, Name = "tag;tag3", 
                ArticlesCount = 5,
                Tags = new List<string>() { "tag", "tag3" }.Select(s => new Tag() { Name = s }).ToList()
            });
    }

    [TestMethod]
    public async Task GetAllSectionsTest()
    {
        var target = new SectionsService(_sectionsRepository);

        var sections = await target.GetAllSections();

        sections.ShouldNotBeNull()
            .PrintToConsole()
            .ShouldNotBeEmpty();

        sections.ShouldAllBe(e => e.Id != 0);
        sections.ShouldAllBe(e => !string.IsNullOrEmpty(e.Name));
        sections.ShouldAllBe(e => e.ArticlesCount > 0);
        sections.ShouldAllBe(e => e.Tags.Count > 0);
    }
    
    [TestMethod]
    public async Task GetSectionByIdTest()
    {
        var target = new SectionsService(_sectionsRepository);

        var sections = await target.GetDetailedSection(2);

        sections.ShouldNotBeNull()
            .PrintToConsole();

        sections.Id.ShouldBe(2);
        sections.Name.ShouldNotBeEmpty();
        sections.ArticlesCount.ShouldBeGreaterThan(0);
        sections.Tags.ShouldNotBeEmpty();
    }
}