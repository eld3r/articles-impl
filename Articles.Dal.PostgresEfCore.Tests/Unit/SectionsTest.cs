using Articles.Dal.PostgresEfCore.Models;
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
    }

    private static async Task WithNewScopedRepo(Func<ISectionsRepository, Task> action)
    {
        await WithNewScope(action);
    }

    [TestMethod]
    public async Task GetAllSectionsTest()
    {
        await WithNewScopedDbContext(async dbContext =>
        {
            var tags = Enumerable.Range(1, 5).Select(i => new TagEntity() { Name = $"Tag {i}" }).ToList();
            
            var section = new SectionEntity()
            {
                Name = "section 1",
                Tags = tags
            };
            await dbContext.Sections.AddAsync(section);

            var article = new ArticleEntity()
            {
                Title = "Article 1",
                Tags = tags
            };
            
            await dbContext.Articles.AddAsync(article);
            await dbContext.SaveChangesAsync();
        });
        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.GetSections();
            result.PrintToConsole();
        });
    }
}