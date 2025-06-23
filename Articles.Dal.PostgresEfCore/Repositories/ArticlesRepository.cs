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

        await EnrichExistingTags(article);
        
        var articleEntity = article.Adapt<ArticleEntity>();
        articleEntity.DateCreated = DateTime.UtcNow;
        
        await dbContext.AddAsync(articleEntity);
        await dbContext.SaveChangesAsync();
        
        article.Id = articleEntity.Id;
        article.DateCreated = articleEntity.DateCreated;
    }

    public async Task Update(Article article)
    {
        await EnrichExistingTags(article);
        
        var articleEntity = await dbContext.Articles
            .Include(q => q.TagLinks)
            .ThenInclude(q => q.Tag)
            .FirstOrDefaultAsync(x => x.Id == article.Id);

        if (articleEntity is null)
            throw new ItemNotFoundException("Article not found");

        article.Adapt(articleEntity);
        articleEntity.DateModified = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        article.DateModified = articleEntity.DateModified;
    }

    private async Task EnrichExistingTags(Article article)
    {
        var existingTags = await dbContext.Tags.Where(w => article.Tags
                .Select(s => s.Name)
                .Contains(w.Name))
            .AsNoTracking()
            .ToDictionaryAsync(k => k.Name);
        
        foreach (var articleTag in article.Tags)
        {
            if (!existingTags.TryGetValue(articleTag.Name, out var existingTag))
                continue;
            
            articleTag.Id = existingTag.Id;
        };
    }
}