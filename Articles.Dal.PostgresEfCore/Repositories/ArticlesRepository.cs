using Articles.Dal.PostgresEfCore.Equality;
using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Articles.Dal.PostgresEfCore.Repositories;

public class ArticlesRepository(ArticlesDbContext dbContext) : IArticlesRepository
{
    public async Task<Article?> GetById(long id) =>
        (await dbContext.Articles.Include(q => q.Tags).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id))
        .Adapt<Article>();

    public async Task Add(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        var articleEntity = article.Adapt<ArticleEntity>();

        foreach (var tag in articleEntity.Tags.Where(t=>t.Id != 0))
        {
            dbContext.Attach(tag);
        }

        await dbContext.AddAsync(articleEntity);
        await dbContext.SaveChangesAsync();
        article.Id = articleEntity.Id;
        article.DateCreated = articleEntity.DateCreated;
    }

    public async Task Update(Article article)
    {
        var articleEntity = await dbContext.Articles
            .Include(q => q.Tags)
            .FirstOrDefaultAsync(x => x.Id == article.Id);
        
        if (articleEntity is null)
            //TODO придумать эксепшон
            throw new Exception("Article not found");

        var finalTags = articleEntity.Tags
            .Where(articleEntityTag => article.Tags.Any(t => t.Name == articleEntityTag.Name))
            .Union(
                from articleTag in article.Tags
                where articleEntity.Tags.All(t => t.Name != articleTag.Name)
                select articleTag.Adapt<TagEntity>()
                ).ToList();

        articleEntity.Title = article.Title;
        articleEntity.Tags = finalTags;
        
        await dbContext.SaveChangesAsync();
        article.DateModified = articleEntity.DateModified;
    }
}