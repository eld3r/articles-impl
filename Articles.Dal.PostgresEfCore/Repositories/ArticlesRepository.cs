using Articles.Dal.Exceptions;
using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Articles.Dal.PostgresEfCore.Repositories;

public class ArticlesRepository(ArticlesDbContext dbContext) : BaseRepository(dbContext), IArticlesRepository
{
    private readonly ArticlesDbContext _dbContext = dbContext;

    public async Task<Article?> GetById(long id) =>
        (await _dbContext.Articles
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
        articleEntity.DateCreated = DateTime.UtcNow;

        articleEntity.TagLinks = await EnrichExistingTags(article.Tags, articleEntity);
        await _dbContext.AddAsync(articleEntity);
        await _dbContext.SaveChangesAsync();
        
        article.Id = articleEntity.Id;
        article.DateCreated = articleEntity.DateCreated;
    }

    public async Task Update(Article article)
    {
        var articleEntity = await _dbContext.Articles
            .Include(q => q.TagLinks)
            .ThenInclude(q => q.Tag)
            .FirstOrDefaultAsync(x => x.Id == article.Id);

        if (articleEntity is null)
            throw new ItemNotFoundException("Article not found");

        article.Adapt(articleEntity);
        articleEntity.TagLinks = await EnrichExistingTags(article.Tags, articleEntity);
        articleEntity.DateModified = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        article.DateModified = articleEntity.DateModified;
    }
    
    private async Task<List<ArticleTagEntity>> EnrichExistingTags(List<Tag> tags, ArticleEntity articleEntity)
    {
        var existingTags = await LoadExistingTagsAsync(tags);
        
        var result = new List<ArticleTagEntity>();
        
        int index = 0;
        foreach (var articleTag in tags)
        {
            var resultTagEntity = new ArticleTagEntity() { };
            if (existingTags.TryGetValue(articleTag.Name, out var existingTag))
            {
                resultTagEntity.Tag = existingTag;
                resultTagEntity.TagId = existingTag.Id;
                resultTagEntity.Order = index;
            }
            else
            {
                resultTagEntity.Tag = articleTag.Adapt<TagEntity>();
                resultTagEntity.Order = index;
            }
            
            result.Add(resultTagEntity);
            index++;
        };
        return result;
    }
}