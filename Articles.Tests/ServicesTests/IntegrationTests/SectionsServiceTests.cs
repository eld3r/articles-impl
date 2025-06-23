using Articles.Dal.PostgresEfCore;
using Articles.Services;
using Articles.Tests.Extensions;
using DeepEqual.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Tests.ServicesTests.IntegrationTests;

[TestClass]
[TestCategory("Integration")]
public class SectionsServiceTests : IntegrationTestsBaseProfile
{
    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        var db = ServiceProvider.GetRequiredService<ArticlesDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        var db = ServiceProvider.GetRequiredService<ArticlesDbContext>();
        db.Tags.RemoveRange(db.Tags);
        db.Articles.RemoveRange(db.Articles);
        db.Sections.RemoveRange(db.Sections);
        await db.SaveChangesAsync();
    }

    private static async Task WithNewScopedSectionsService(Func<ISectionsService, Task> action)
    {
        await WithNewScopedService(action);
    }

    [TestMethod]
    public async Task GetAllSectionsTest()
    {
        var sections = FakeDataFactory.CreateFakeSections(3).PrintToConsole();
        await WithNewScopedService<ArticlesDbContext>(async db =>
        {
            await db.Tags.AddRangeAsync(sections.SelectMany(s => s.Tags));
            await db.Sections.AddRangeAsync(sections);
            await db.SaveChangesAsync();
        });

        await WithNewScopedSectionsService(async serice =>
        {
            var result = await serice.GetAllSections();

            result.PrintToConsole().Count.ShouldBe(sections.Count);

            foreach (var section in result)
            {
                section.Id.ShouldBeGreaterThan(0);

                var givenEntity = sections.FirstOrDefault(f => f.Id == section.Id).ShouldNotBeNull();

                section.Name.ShouldNotBeNull(givenEntity.Name);
                section.ArticlesCount.ShouldBe(givenEntity.Articles.Count);
                section.Tags.ShouldDeepEqual(givenEntity.Tags.Select(s => s.Name));
            }

            result.Select(s => s.Id)
                .ShouldDeepEqual(sections.OrderByDescending(s => s.Articles.Count).Select(s => s.Id));
        });
    }

    [TestMethod]
    public async Task GetSectionByIdTest()
    {
        var sections = FakeDataFactory.CreateFakeSections(1).PrintToConsole();
        await WithNewScopedService<ArticlesDbContext>(async db =>
        {
            await db.Tags.AddRangeAsync(sections.SelectMany(s => s.Tags));
            await db.Sections.AddRangeAsync(sections);
            await db.SaveChangesAsync();
        });

        await WithNewScopedSectionsService(async serice =>
        {
            var section = await serice.GetDetailedSection(sections.First().Id);

            section.PrintToConsole();

            section.Id.ShouldBeGreaterThan(0);

            section.Name.ShouldNotBeNull(sections.First().Name);
            section.ArticlesCount.ShouldBe(sections.First().Articles.Count);
            section.Tags.ShouldDeepEqual(sections.First().Tags.Select(s => s.Name));
            section.Articles.ShouldNotBeNull().Select(s => s.Title)
                .ShouldDeepEqual(sections.First().Articles.OrderByDescending(o => o.DateModified ?? o.DateCreated)
                    .Select(s => s.Title));
        });
    }
}