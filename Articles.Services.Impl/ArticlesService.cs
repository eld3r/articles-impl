using Articles.Dal;
using Articles.Dal.Exceptions;
using Articles.Domain.Entities;
using Articles.Services.DTO;
using Mapster;

namespace Articles.Services.Impl;

public class ArticlesService(IArticlesRepository articlesRepository, ITagsRepository tagsRepository) : IArticlesService
{
    public async Task<ArticleDto?> GetById(long id)
    {
        return (await articlesRepository.GetById(id)).Adapt<ArticleDto>();
    }

    private async Task EnrichExistingTags(Article article)
    {
        //обработать ситуацию с имеющимися тэгами
        //TODO учесть порядок тэгов
        var existingTags = await tagsRepository.GetExistingTags(article.Tags);
        
        foreach (var articleTag in article.Tags)
        {
            if (!existingTags.TryGetValue(articleTag.Name, out var existingTag))
                continue;
            
            articleTag.Id = existingTag.Id;
        };
    }

    private void DistinctTags(Article article)
    {
        article.Tags = article.Tags.Distinct(new TagEqualityComparer()).ToList();
    }

    public async Task<ArticleDto> Create(CreateArticleRequest createArticleRequest)
    {
        var article = createArticleRequest.Adapt<Article>();
        
        DistinctTags(article);
        await EnrichExistingTags(article);
        
        await articlesRepository.Add(article);
        return article.Adapt<ArticleDto>();
    }

    public async Task<bool> Update(long id, UpdateArticleRequest updateArticleRequest)
    {
        var article = updateArticleRequest.Adapt<Article>();
        article.Id = id;
        
        DistinctTags(article);
        await EnrichExistingTags(article);
        
        try
        {
            await articlesRepository.Update(article);
        }
        catch (ItemNotFoundException e)
        {
            Console.WriteLine(e);
            return false;
        }
        
        return true;
    }
}