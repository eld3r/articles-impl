﻿using Articles.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Dal.PostgresEfCore.Tests.Unit;

[TestClass]
public class ArticlesTests : DbInitiateTestProfileBase
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
    public async Task AddArticleTest()
    {
        var article = new Article()
        {
            Title = "Статья 1",
            Tags = new List<string>() { "биология", "физика", "наука" }
                .Select(str => new Tag { Name = str })
                .ToList()
        };
        
        await WithNewScopedRepo(async repo =>
        {
            await repo.Add(article);
        });

        await WithNewScopedDbContext(async db =>
        {
            var dbArticle = await db.Articles.FirstOrDefaultAsync(x => x.Id == article.Id);

            dbArticle.ShouldNotBeNull().PrintToConsole();
            dbArticle.Title.ShouldBe(article.Title);
            dbArticle.Tags.ShouldBeEmpty();
            dbArticle.DateCreated.Date.ShouldBe(DateTime.Today);
            
            (await db.Tags.ToListAsync()).ShouldBeEmpty();
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
            var dbArticle = await db.Articles.FirstOrDefaultAsync(x => x.Id == article.Id);

            dbArticle.ShouldNotBeNull().PrintToConsole();
            dbArticle.Title.ShouldBe(title);
            dbArticle.Tags.ShouldBeEmpty();
            dbArticle.DateCreated.Date.ShouldBe(DateTime.Today);
            dbArticle.DateModified.ShouldNotBeNull().Date.ShouldBe(DateTime.Today);
            
            dbArticle.DateCreated.ShouldBeLessThan(dbArticle.DateModified.Value);
            
            (await db.Tags.ToListAsync()).ShouldBeEmpty();
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