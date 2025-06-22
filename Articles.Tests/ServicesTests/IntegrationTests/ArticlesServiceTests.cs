using Articles.Dal.PostgresEfCore;
using Articles.Dal.PostgresEfCore.Models;
using Articles.Services;
using Articles.Services.DTO;
using Articles.Tests.Extensions;
using DeepEqual.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Tests.ServicesTests.IntegrationTests;

[TestClass]
public class ArticlesServiceTests : IntegrationTestsBaseProfile
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

    private static async Task WithNewScopedRepo(Func<IArticlesService, Task> action)
    {
        await WithNewScope(action);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public async Task AddArticleTest(int existingTagsCount)
    {
        if (existingTagsCount > 0)
        {
            await WithNewScope<ArticlesDbContext>(async db =>
            {
                await db.Tags.AddRangeAsync(Enumerable.Range(1, existingTagsCount).Select(s => new TagEntity() { Name = $"tag{s}" }));
                await db.SaveChangesAsync();
            });
        }

        await WithNewScopedRepo(async target =>
        {
            var createDto = new CreateArticleRequest() { Title = "my article 1", Tags = ["tag1", "tag2", "tag3"] };

            var created = await target.Create(createDto);

            var db = ServiceProvider.GetRequiredService<ArticlesDbContext>();
            var article = await db.Articles
                .Include(q => q.Tags)
                .FirstOrDefaultAsync(f => f.Id == created.Id);

            article.ShouldNotBeNull().PrintToConsole();
            article.Id.ShouldBeGreaterThan(0);
            article.Id.ShouldBe(created.Id);
            article.Title.ShouldBe(createDto.Title);
            article.Tags.ShouldNotBeEmpty();
            article.DateCreated.Date.ShouldBe(DateTime.Today);

            db.Remove(article);
            await db.SaveChangesAsync();
        });
    }

    public async Task AddRepeatedArticleTest()
    {
        var createDto = new CreateArticleRequest() { Title = "my article 1", Tags = ["tag1", "tag2", "tag3"] };

        await WithNewScopedRepo(async target =>
        {
            var created = await target.Create(createDto);
        });

        await WithNewScopedRepo(async target =>
        {
            var created = await target.Create(createDto);

            var db = ServiceProvider.GetRequiredService<ArticlesDbContext>();
            var article = await db.Articles
                .Include(q => q.Tags)
                .FirstOrDefaultAsync(f => f.Id == created.Id);

            (await db.Articles.ToListAsync()).PrintToConsole().Count.ShouldBe(2);
            article.ShouldNotBeNull().PrintToConsole();
            article.Id.ShouldBeGreaterThan(0);
            article.Id.ShouldBe(created.Id);
            article.Title.ShouldBe(createDto.Title);
            article.Tags.ShouldNotBeEmpty();
            article.DateCreated.Date.ShouldBe(DateTime.Today);
        });
    }

    [TestMethod]
    [DataRow("Update nothing", "my article 1", "tag1;tag2;tag3")]
    [DataRow("Update title only", "ARTICLE 2", "tag1;tag2;tag3")]
    [DataRow("Full update tags", "asdasd", "tag4;tag5;tag6")]
    [DataRow("Update some tags", "ddddaaa", "tag1;tag2;tag5")]
    [DataRow("Update tags order", "ddddaaa", "tag3;tag1;tag2")]
    [DataRow("Update nothing", "my article 1", "tag1;tag2;tag3", true)]
    [DataRow("Update title only", "ARTICLE 2", "tag1;tag2;tag3", true)]
    [DataRow("Full update tags", "asdasd", "tag4;tag5;tag6", true)]
    [DataRow("Update some tags", "ddddaaa", "tag1;tag2;tag5", true)]
    [DataRow("Update tags order", "ddddaaa", "tag3;tag1;tag2", true)]
    public async Task UpdateArticleTest(string caseName, string title, string tags, bool insertPreExisingTags = false)
    {
        if (insertPreExisingTags)
        {
            await WithNewScope<ArticlesDbContext>(async db =>
            {
                await db.Tags.AddRangeAsync(Enumerable.Range(1, 6).Select(s => new TagEntity() { Name = $"tag{s}" }));
                await db.SaveChangesAsync();
            });
        }

        ArticleDto createdArticle = null!;
        await WithNewScopedRepo(async target =>
        {
            var createDto = new CreateArticleRequest() { Title = "my article 1", Tags = ["tag1", "tag2", "tag3"] };

            createdArticle = await target.Create(createDto);
            createdArticle.DateModified.ShouldBeNull();
        });

        createdArticle.ShouldNotBeNull();

        await WithNewScopedRepo(async target =>
        {
            var updateDto = new UpdateArticleRequest()
                { Id = createdArticle.Id, Title = title, Tags = tags.Split(';').ToList() };

            await target.Update(updateDto);

            var db = ServiceProvider.GetRequiredService<ArticlesDbContext>();
            var article = await db.Articles
                .Include(q => q.Tags)
                .FirstOrDefaultAsync(f => f.Id == updateDto.Id);

            article.ShouldNotBeNull().PrintToConsole();
            article.Id.ShouldBeGreaterThan(0);
            article.Id.ShouldBe(updateDto.Id);
            article.Title.ShouldBe(updateDto.Title);
            article.Tags.ShouldNotBeEmpty();
            article.Tags.Select(t => t.Name).ShouldDeepEqual(updateDto.Tags);
            article.DateCreated.Date.ShouldBe(DateTime.Today);
            if (createdArticle.Title != updateDto.Title || !createdArticle.Tags.IsDeepEqual(updateDto.Tags))
                article.DateModified.ShouldNotBeNull();
        });
    }
}