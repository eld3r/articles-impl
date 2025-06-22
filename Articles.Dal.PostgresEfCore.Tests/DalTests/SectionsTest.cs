using Articles.Dal.PostgresEfCore.Models;
using Articles.Dal.PostgresEfCore.Tests.Unit.Base;
using Articles.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Dal.PostgresEfCore.Tests.Unit;

[TestClass]
public class SectionsTests : DbInitiateTestProfileBase
{
    [TestInitialize]
    public async Task TestInit()
    {
        var dbContext = ServiceProvider.GetRequiredService<ArticlesDbContext>();
        dbContext.Tags.RemoveRange(dbContext.Tags);
        dbContext.Sections.RemoveRange(dbContext.Sections);
        dbContext.Articles.RemoveRange(dbContext.Articles);
        await dbContext.SaveChangesAsync();

        //TODO без лишних вставок
        var tags = Enumerable.Range(1, 5).Select(i => new TagEntity() { Name = $"Tag {i}" }).ToList();
        var tags1 = Enumerable.Range(1, 5).Select(i => new TagEntity() { Name = $"Tag {i + 10}" }).ToList();

        var sections = new List<SectionEntity>()
        {
            new()
            {
                Name = "section 1",
                Tags = tags
            },
            new()
            {
                Name = "section 2",
                Tags = tags1
            }
        };

        var articles = new List<ArticleEntity>()
        {
            new()
            {
                Title = "Article 1",
                Tags = tags,
                Section = sections[0],
            },
            new()
            {
                Title = "Article 2",
                Tags = tags1,
                Section = sections[1],
            },
            new()
            {
                Title = "Article 3",
                Tags = tags1,
                Section = sections[1],
            }
        };

        await dbContext.Articles.AddRangeAsync(articles);
        await dbContext.Sections.AddRangeAsync(sections);
        await dbContext.SaveChangesAsync();
    }

    private static async Task WithNewScopedRepo(Func<ISectionsRepository, Task> action)
    {
        await WithNewScope(action);
    }

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
    public async Task AddSectionTest()
    {
        var section = new Section()
        {
            Name = "section to remove",
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
            var sectionEntity = await db.Sections.FirstOrDefaultAsync(f => f.Id == section.Id);
            sectionEntity.ShouldNotBeNull();
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
            await repo.RemoveSection(sectionId);
        });
        
        await WithNewScopedDbContext(async dbContext =>
        {
            (await dbContext.Sections.FirstOrDefaultAsync())
                .ShouldBeNull();
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
        
        await WithNewScopedRepo(async repo =>
        {
            await repo.RemoveSection(section.Id);
        });

        await WithNewScopedDbContext(async db =>
        {
            var sectionEntity = await db.Sections.FirstOrDefaultAsync(f => f.Id == section.Id);
            sectionEntity.ShouldBeNull();
        });
    }
}