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
        var articleEntity = article.Adapt<ArticleEntity>();
        await dbContext.AddAsync(articleEntity);
        await dbContext.SaveChangesAsync();
        article.Id = articleEntity.Id;
        article.DateCreated = articleEntity.DateCreated;
    }

    public async Task Update(Article article)
    {
        var entity = await dbContext.Articles.FirstOrDefaultAsync(x => x.Id == article.Id);
        if (entity is null)
            //TODO придумать эксепшон
            throw new Exception("Article not found");

        article.Adapt(entity);
        await dbContext.SaveChangesAsync();
        article.DateModified = entity.DateModified;
    }
}