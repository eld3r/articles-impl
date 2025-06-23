using Articles.Dal;
using Articles.Dal.PostgresEfCore;
using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Articles.Tests.DalTests.Base;
using Articles.Tests.Extensions;
using DeepEqual.Syntax;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Tests.DalTests;

[TestClass]
[TestCategory("Dal-Integration")]
public class ArticlesRepositoryTests : DbInitiateTestProfileBase
{
    [TestInitialize]
    public async Task TestInit()
    {
        var dbContext = ServiceProvider.GetRequiredService<ArticlesDbContext>();
        dbContext.Tags.RemoveRange(dbContext.Tags);
        dbContext.Articles.RemoveRange(dbContext.Articles);
        await dbContext.SaveChangesAsync();
    }

    private static async Task WithNewScopedRepo(Func<IArticlesRepository, Task> action)
    {
        await WithNewScope(action);
    }

    [TestMethod]
    public async Task AddArticleWithTooLooongTagTest()
    {
        var article = new Article()
        {
            Title = "Статья 1",
            Tags = new List<string>()
                    { "биология", "физика", new string(Enumerable.Range(0, 257).Select(x => 'a').ToArray()) }
                .Select(str => new Tag { Name = str })
                .ToList()
        };

        await WithNewScopedRepo(async repo => { await Should.ThrowAsync<DbUpdateException>(repo.Add(article)); });
    }

    [TestMethod]
    public async Task AddArticleTest()
    {
        var article = new Article()
        {
            Title = "Статья 1",
            Tags = new List<string>() { "биология", "физика", "наука" }
                .Select(str => new Tag { Name = str })
                .ToList()
        };

        await WithNewScopedRepo(async repo => { await repo.Add(article); });

        await WithNewScopedDbContext(async db =>
        {
            var dbArticle = await db.Articles
                .Include(q => q.TagLinks)
                .FirstOrDefaultAsync(x => x.Id == article.Id);

            dbArticle.ShouldNotBeNull().PrintToConsole();
            dbArticle.Title.ShouldBe(article.Title);
            dbArticle.TagLinks.ShouldNotBeEmpty();
            dbArticle.DateCreated.Date.ShouldBe(DateTime.UtcNow.Date);
        });
    }

    [TestMethod]
    public async Task AddNullTest()
    {
        await WithNewScopedRepo(async repo =>
        {
            await Should.ThrowAsync<ArgumentNullException>(() => repo.Add(null!));
        });
    }

    [TestMethod]
    public async Task UpdateArticleTest()
    {
        var article = new Article()
        {
            Title = "Статья 1",
            Tags = new List<string>() { "биология", "физика", "наука" }
                .Select(str => new Tag { Name = str })
                .ToList()
        };

        const string title = "Новое название для статьи";

        await WithNewScopedRepo(async repo =>
        {
            await repo.Add(article);
            var updateArticle = article.Adapt<Article>();
            updateArticle.Title = title;
            await repo.Update(updateArticle);
            updateArticle.DateModified.ShouldNotBeNull();
        });

        await WithNewScopedDbContext(async db =>
        {
            var dbArticle = await db.Articles
                .Include(q => q.TagLinks)
                .ThenInclude(q => q.Tag)
                .FirstOrDefaultAsync(x => x.Id == article.Id);

            dbArticle.ShouldNotBeNull().PrintToConsole();
            dbArticle.Title.ShouldBe(title);
            dbArticle.TagLinks.Select(s=>s.Tag.Name).ShouldDeepEqual(new List<string>{"биология", "физика", "наука"});
            dbArticle.DateCreated.Date.ShouldBe(DateTime.UtcNow.Date);
            dbArticle.DateModified.ShouldNotBeNull().Date.ShouldBe(DateTime.UtcNow.Date);

            dbArticle.DateCreated.ShouldBeLessThan(dbArticle.DateModified.Value);
        });
    }

    [TestMethod]
    public async Task GetNonExistingArticleTest() =>
        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.GetById(-1);
            result.ShouldBeNull();
        });

    [TestMethod]
    public async Task GetArticleTest()
    {
        var articleId = 0L;
        await WithNewScopedDbContext(async db =>
        {
            var article = new ArticleEntity()
            {
                Title = "title",
                TagLinks = new[] { "tag", "tag1", "tag2" }
                    .Select(s => new ArticleTagEntity { Tag = new TagEntity() { Name = s } }).ToList()
            };
            await db.AddAsync(article);
            await db.SaveChangesAsync();
            articleId = article.Id;
        });

        articleId.ShouldNotBe(0);

        await WithNewScopedRepo(async repo =>
        {
            var result = await repo.GetById(articleId);
            result.ShouldNotBeNull();
            result.Title.ShouldBe("title");
            result.Tags.ShouldNotBeEmpty();
        });
    }

    [TestMethod]
    public async Task AddWithTooLongName() =>
        await WithNewScopedRepo(async repo =>
        {
            await Should.ThrowAsync<DbUpdateException>(() => repo.Add(new Article()
            {
                Title = new string(Enumerable
                    .Range(0, 1025)
                    .Select(_ => 'a')
                    .ToArray())
            }));
        });
}