using Articles.Dal.Exceptions;
using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Articles.Dal.PostgresEfCore.Repositories;

public class ArticlesRepository(ArticlesDbContext dbContext) : IArticlesRepository
{
    public async Task<Article?> GetById(long id) =>
        (await dbContext.Articles
            .Include(q => q.TagLinks)
            .ThenInclude(q => q.Tag)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
        )
        .Adapt<Article>();

    public async Task Add(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        var articleEntity = article.Adapt<ArticleEntity>();

        await dbContext.AddAsync(articleEntity);
        await dbContext.SaveChangesAsync();
        article.Id = articleEntity.Id;
        article.DateCreated = articleEntity.DateCreated;
    }

    public async Task Update(Article article)
    {
        var articleEntity = await dbContext.Articles
            .Include(q => q.TagLinks)
            .ThenInclude(q => q.Tag)
            .FirstOrDefaultAsync(x => x.Id == article.Id);

        if (articleEntity is null)
            throw new ItemNotFoundException("Article not found");

        article.Adapt(articleEntity);
        
        await dbContext.SaveChangesAsync();
        article.DateModified = articleEntity.DateModified;
    }
}