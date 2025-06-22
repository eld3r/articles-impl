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
    public async Task GetArticleTest()
    {
        var tags = Enumerable.Range(1, 5)
            .Select(s => new TagEntity() { Name = $"tag{s}" }).ToList();

        var article = new ArticleEntity()
        {
            Title = "art1", TagLinks = tags
                .Select(t => new ArticleTagEntity() { Tag = t })
                .ToList()
        };
        
        await WithNewScope<ArticlesDbContext>(async db =>
        {
            await db.Tags.AddRangeAsync(tags);
            await db.Articles.AddAsync(article);
            await db.SaveChangesAsync();
        });

        await WithNewScopedRepo(async repo =>
        {
            var articleDto = await repo.GetById(article.Id);

            articleDto.ShouldNotBeNull().PrintToConsole();
            articleDto.Title.ShouldBe(article.Title);
            articleDto.Tags.ShouldDeepEqual(tags.Select(t => t.Name).ToList());
        });

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
                await db.Tags.AddRangeAsync(Enumerable.Range(1, existingTagsCount)
                    .Select(s => new TagEntity() { Name = $"tag{s}" }));
                await db.SaveChangesAsync();
            });
        }

        var requestForCreate = new CreateArticleRequest() { Title = "my article 1", Tags = ["TAG1", "TAG2", "TAG3"] };

        await WithNewScopedRepo(async target =>
        {
            var createdDto = await target.Create(requestForCreate);

            var db = ServiceProvider.GetRequiredService<ArticlesDbContext>();
            var dbArticle = await db.Articles
                .Include(q => q.TagLinks)
                .ThenInclude(q => q.Tag)
                .FirstOrDefaultAsync(f => f.Id == createdDto.Id);

            dbArticle.ShouldNotBeNull();
            dbArticle.Id.ShouldBeGreaterThan(0);
            dbArticle.Id.ShouldBe(createdDto.Id);
            dbArticle.Title.ShouldBe(requestForCreate.Title);
            dbArticle.TagLinks
                .OrderBy(o => o.Order)
                .Select(s => s.Tag?.Name)
                .ShouldDeepEqual(requestForCreate.Tags.Select(s => s.ToLower()));
            dbArticle.DateCreated.Date.ShouldBe(DateTime.UtcNow.Date);
            
            createdDto.PrintToConsole().Tags.ShouldDeepEqual(requestForCreate.Tags.Select(s => s.ToLower()));

            db.Remove(dbArticle);
            await db.SaveChangesAsync();
        });
    }

    [TestMethod]
    public async Task AddRepeatedArticleTest()
    {
        var createDto = new CreateArticleRequest() { Title = "my article 1", Tags = ["TAG1", "TAG2", "TAG3"] };

        await WithNewScopedRepo(async target =>
        {
            var created = await target.Create(createDto);
        });

        await WithNewScopedRepo(async target =>
        {
            var created = await target.Create(createDto);

            var db = ServiceProvider.GetRequiredService<ArticlesDbContext>();
            var article = await db.Articles
                .Include(q => q.TagLinks)
                .ThenInclude(q => q.Tag)
                .FirstOrDefaultAsync(f => f.Id == created.Id);

            (await db.Articles.ToListAsync()).PrintToConsole().Count.ShouldBe(2);
            article.ShouldNotBeNull().PrintToConsole();
            article.Id.ShouldBeGreaterThan(0);
            article.Id.ShouldBe(created.Id);
            article.Title.ShouldBe(createDto.Title);
            article.TagLinks
                .OrderBy(o => o.Order)
                .Select(s => s.Tag?.Name)
                .ShouldDeepEqual(createDto.Tags.Select(s => s.ToLower()));
            article.DateCreated.Date.ShouldBe(DateTime.UtcNow.Date);
        });
    }

    [TestMethod]
    [DataRow("1. Update nothing", "my article 1", "Tag1;Tag2;Tag3")]
    [DataRow("2. Update title only", "ARTICLE 2", "Tag1;Tag2;Tag3")]
    [DataRow("3. Full update tags", "asdasd", "Tag4;Tag5;Tag6")]
    [DataRow("4. Update some tags", "ddddaaa", "Tag1;Tag2;Tag5")]
    [DataRow("5. Update tags order", "ddddaaa", "Tag3;Tag1;Tag2")]
    [DataRow("6. Update with repeated tags", "rep", "tag;tAg;TAG")]
    [DataRow("1. Update nothing", "my article 1", "Tag1;Tag2;Tag3", true)]
    [DataRow("2. Update title only", "ARTICLE 2", "Tag1;Tag2;Tag3", true)]
    [DataRow("3. Full update tags", "asdasd", "Tag4;Tag5;Tag6", true)]
    [DataRow("4. Update some tags", "ddddaaa", "Tag1;Tag2;Tag5", true)]
    [DataRow("5. Update tags order", "ddddaaa", "Tag3;Tag1;Tag2", true)]
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
                { Title = title, Tags = tags.Split(';').ToList() };

            await target.Update(createdArticle.Id, updateDto);

            var db = ServiceProvider.GetRequiredService<ArticlesDbContext>();
            var article = await db.Articles
                .Include(q => q.TagLinks)
                .ThenInclude(q => q.Tag)
                .FirstOrDefaultAsync(f => f.Id == createdArticle.Id);

            article.ShouldNotBeNull().PrintToConsole();
            article.Id.ShouldBeGreaterThan(0);
            article.Id.ShouldBe(createdArticle.Id);
            article.Title.ShouldBe(updateDto.Title);
            article.TagLinks.ShouldNotBeNull().ShouldNotBeEmpty();
            article.TagLinks
                .Select(t => t.Tag?.Name)
                .ShouldDeepEqual(updateDto.Tags.Select(s => s.ToLower()).Distinct());
            article.DateCreated.Date.ShouldBe(DateTime.UtcNow.Date);
            if (createdArticle.Title != updateDto.Title || !createdArticle.Tags.IsDeepEqual(updateDto.Tags.Select(s => s.ToLower())))
                article.DateModified.ShouldNotBeNull();
        });
    }
}