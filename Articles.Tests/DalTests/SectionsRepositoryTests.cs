using Articles.Dal;
using Articles.Dal.PostgresEfCore;
using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Articles.Tests.DalTests.Base;
using Articles.Tests.Extensions;
using DeepEqual.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Tests.DalTests;

[TestClass]
[TestCategory("Dal-Integration")]
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

    private List<SectionEntity> GetTestSections()
    {
        return
        [
            new SectionEntity()
            {
                Id = 0,
                Name = "tag1, tag2, tag3",
                Articles = new List<ArticleEntity>
                {
                    new() { Title = "Title 1" },
                    new() { Title = "Title 2" }
                },
                Tags = new List<TagEntity>()
                {
                    new() { Name = "tag1" },
                    new() { Name = "tag2" },
                    new() { Name = "tag3" }
                }
            },
            new SectionEntity()
            {
                Id = 0,
                Name = "tag4, tag5, tag6",
                Articles = new List<ArticleEntity>
                {
                    new() { Title = "Title 4" },
                    new() { Title = "Title 5" },
                    new() { Title = "Title 6" }
                },
                Tags = new List<TagEntity>()
                {
                    new() { Name = "tag4" },
                    new() { Name = "tag5" },
                    new() { Name = "tag6" }
                }
            }
        ];
    }

    [TestMethod]
    public async Task GetAllSectionsTest()
    {
        var testSectionEntities = GetTestSections();
        await WithNewScopedDbContext(async db =>
        {
            await db.AddRangeAsync(testSectionEntities);
            await db.SaveChangesAsync();
        });

        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.GetAllSections();
            result.PrintToConsole();

            result.Count.ShouldBe(2);

            foreach (var section in result)
            {
                var sectionEntity = testSectionEntities.FirstOrDefault(f => f.Name == section.Name).ShouldNotBeNull();
                section.Id.ShouldNotBe(0);
                section.Name.ShouldNotBeEmpty();
                section.ArticlesCount.ShouldBeGreaterThan(0);
                section.Tags.ShouldNotBeNull().ShouldNotBeEmpty();
                section.Tags.Select(s => s.Name).ShouldDeepEqual(sectionEntity.Tags.Select(f => f.Name));
            }
        });
    }

    [TestMethod]
    public async Task GetSectionByIdTest()
    {
        var testSectionEntities = GetTestSections();

        await WithNewScopedDbContext(async dbContext =>
        {
            await dbContext.AddRangeAsync(testSectionEntities);
            await dbContext.SaveChangesAsync();
        });

        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.GetSectionById(testSectionEntities[0].Id);
            result.ShouldNotBeNull().PrintToConsole();
            result.Name.ShouldNotBeEmpty();
            result.ArticlesCount.ShouldBeGreaterThan(0);
            result.Tags.ShouldNotBeEmpty();
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
        var testSectionEntities = GetTestSections();
        await WithNewScopedDbContext(async db =>
        {
            await db.AddRangeAsync(testSectionEntities);
            await db.SaveChangesAsync();
        });

        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.GetAllSections();
            result.PrintToConsole();
            result.Count.ShouldBe(2);
            result[0].Name.ShouldBe(testSectionEntities.MaxBy(s => s.Articles.Count).ShouldNotBeNull().Name);
        });
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public async Task AddSectionTest(int preExistedTagsCount)
    {
        if (preExistedTagsCount > 0)
        {
            await WithNewScopedDbContext(async db =>
            {
                await db.AddRangeAsync(Enumerable.Range(1, preExistedTagsCount)
                    .Select(s => new TagEntity() { Name = $"tag{s}" }));
                await db.SaveChangesAsync();
            });
        }

        var section = new Section()
        {
            Name = "section1",
            Tags = new List<string> { "tag1", "tag2", "tag3" }.Select(s => new Tag() { Name = s }).ToList(),
            Articles = new List<Article>() { new() { Title = "123" } }
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
                .Include(q => q.Articles)
                .FirstOrDefaultAsync(f => f.Id == section.Id);
            sectionEntity.ShouldNotBeNull().PrintToConsole();
            sectionEntity.Name.ShouldBe(section.Name);
            sectionEntity.Articles.Count.ShouldBe(section.Articles.Count);
        });
    }

    [TestMethod]
    public async Task AddRepeatedSectionTest()
    {
        var section = new Section()
        {
            Name = "section1",
            Tags = new List<string> { "tag1", "tag2", "tag3" }.Select(s => new Tag() { Name = s }).ToList(),
        };

        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.AddSection(section);
            result.PrintToConsole();
            result.ShouldBeGreaterThan(0);
            result.ShouldBe(section.Id);
        });

        var prevSectionId = section.Id;
        prevSectionId.ShouldNotBe(0);

        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.AddSection(section);
            result.ShouldBe(prevSectionId);
        });

        await WithNewScopedRepo(async repo =>
        {
            section.Id = 0;
            await Should.ThrowAsync<DbUpdateException>(() => repo.AddSection(section));
        });

        await WithNewScopedDbContext(async db =>
        {
            var sectionEntity = await db.Sections
                .ToListAsync();

            sectionEntity.Count.ShouldBe(1);
        });
    }

    [TestMethod]
    [DataRow("tag3;tag4;tag5")]
    [DataRow("tag5;tag4;tag3")]
    [DataRow("TAG3;tag4;tag5")]
    [DataRow("TAG3;TAG4;TAG5")]
    [DataRow("tag3;tag4;tag5;tag5;tag5;tag5")]
    [DataRow("tag3;tag4;tag5;tag6", false)]
    public async Task FindSectionByTagsTest(string searchTagsString, bool shouldFindSection = true)
    {
        SectionEntity targetSection = null!;
        await WithNewScopedDbContext(async db =>
        {
            var tagEntities = Enumerable.Range(1, 10)
                .Select(s => new TagEntity() { Name = $"tag{s}" })
                .ToList();

            await db.Tags.AddRangeAsync(tagEntities);
            
            targetSection = new SectionEntity()
            {
                Name = "3-4-5",
                Tags = tagEntities.Where(c =>
                        Enumerable.Range(3, 3)
                            .Any(a => c.Name.Contains(a.ToString())))
                    .ToList(),
            };
            await db.Sections.AddAsync(targetSection);

            await db.SaveChangesAsync();
        });

        targetSection.ShouldNotBeNull().Id.ShouldNotBe(0);

        await WithNewScopedRepo(async repo =>
        {
            var searchTags = searchTagsString.Split(';')
                .Select(s => new Tag() { Name = s })
                .ToList();

            searchTags.Select(s => s.Name).PrintToConsole("Search tags:");

            var result = await repo.FindSectionByTags(searchTags);

            if (!shouldFindSection)
            {
                result.ShouldBeNull();
                return;
            }

            result.ShouldNotBeNull().PrintToConsole();
            result.Name.ShouldBe(targetSection.Name);
            result.Tags.Select(s => s.Name).ShouldBe(targetSection.Tags.Select(f => f.Name));
            result.Id.ShouldBe(targetSection.Id);
        });
    }
}