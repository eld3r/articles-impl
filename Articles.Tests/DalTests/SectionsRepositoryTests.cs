using Articles.Dal;
using Articles.Dal.PostgresEfCore;
using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Articles.Tests.DalTests.Base;
using Articles.Tests.Extensions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Tests.DalTests;

[TestClass]
public class SectionsRepositoryTests : DbInitiateTestProfileBase
{
    [TestInitialize]
    public async Task TestInit()
    {
        var dbContext = ServiceProvider.GetRequiredService<ArticlesDbContext>();
        dbContext.Tags.RemoveRange(dbContext.Tags);
        dbContext.Sections.RemoveRange(dbContext.Sections);
        dbContext.Articles.RemoveRange(dbContext.Articles);
        await dbContext.SaveChangesAsync();
    }

    private static async Task WithNewScopedRepo(Func<ISectionsRepository, Task> action)
    {
        await WithNewScope(action);
    }

    [Ignore("Сломан, нужно починить начальную установку")]
    [TestMethod]
    public async Task GetAllSectionsTest()
    {
        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.GetAllSections();
            result.PrintToConsole();
            result.Count.ShouldBe(2);
        });
    }

    [Ignore("Сломан, нужно починить начальную установку")]
    [TestMethod]
    public async Task GetSectionByIdTest()
    {
        long sectionId = 0;

        await WithNewScopedDbContext(async dbContext =>
        {
            sectionId = (await dbContext.Sections.FirstOrDefaultAsync()).ShouldNotBeNull().Id;
        });

        sectionId.ShouldBeGreaterThan(0);

        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.GetSectionById(sectionId);
            result.ShouldNotBeNull().PrintToConsole();
        });
    }

    [TestMethod]
    public async Task GetNonExistingSectionByIdTest()
    {
        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.GetSectionById(-1);
            result.ShouldBeNull();
        });
    }

    [Ignore("Сломан, нужно починить начальную установку")]
    [TestMethod]
    public async Task GetAllSectionsOrderTest()
    {
        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.GetAllSections();
            result.PrintToConsole();
            result.Count.ShouldBe(2);
            result[0].Name.ShouldBe("section 2");
        });
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public async Task AddSectionWithExistedTagsTest(int preExisteTagsCount)
    {
        if (preExisteTagsCount > 0)
        {
            await WithNewScopedDbContext(async db =>
            {
                await db.AddRangeAsync(Enumerable.Range(1, preExisteTagsCount)
                    .Select(s => new TagEntity() { Name = $"tag{s}" }));
                await db.SaveChangesAsync();
            });
        }
        var section = new Section()
        {
            Name = "section to remove",
            Tags = new List<string> { "tag1", "tag2", "tag3" }.Select(s => new Tag() { Name = s }).ToList(),
        };

        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.AddSection(section);
            result.PrintToConsole();
            result.ShouldBeGreaterThan(0);
            result.ShouldBe(section.Id);
        });

        await WithNewScopedDbContext(async db =>
        {
            var sectionEntity = await db.Sections
                .Include(q => q.Tags)
                .FirstOrDefaultAsync(f => f.Id == section.Id);
            sectionEntity.ShouldNotBeNull().PrintToConsole();
            sectionEntity.Name.ShouldBe(section.Name);
        });
    }

    [TestMethod]
    public async Task RemoveNonExistingSectionTest()
    {
        await WithNewScopedRepo(async repo =>
        {
            await Should.ThrowAsync<InvalidOperationException>(() => repo.RemoveSection(-1));
        });
    }

    [Ignore("Сломан, нужно починить начальную установку")]
    [TestMethod]
    public async Task RemoveSectionWithArticlesTest()
    {
        long sectionId = 0;

        await WithNewScopedDbContext(async dbContext =>
        {
            sectionId = (await dbContext.Sections.FirstOrDefaultAsync()).ShouldNotBeNull().Id;
        });

        sectionId.ShouldBeGreaterThan(0);

        await WithNewScopedRepo(async repo =>
        {
            await Should.ThrowAsync<DbUpdateException>(() => repo.RemoveSection(sectionId));
        });
    }

    [TestMethod]
    public async Task RemoveSectionWithoutArticlesTest()
    {
        var section = new Section()
        {
            Name = "section to remove",
        };

        await WithNewScopedDbContext(async db =>
        {
            var sectionEntity = await db.Sections.AddAsync(section.Adapt<SectionEntity>());
            await db.SaveChangesAsync();
            section.Id = sectionEntity.Entity.Id;
        });

        section.Id.ShouldBeGreaterThan(0);

        await WithNewScopedRepo(async repo => { await repo.RemoveSection(section.Id); });

        await WithNewScopedDbContext(async db =>
        {
            var sectionEntity = await db.Sections.FirstOrDefaultAsync(f => f.Id == section.Id);
            sectionEntity.ShouldBeNull();
        });
    }

    //todo тесты GetSectionForTags...
}