using Articles.Dal;
using Articles.Dal.Exceptions;
using Articles.Domain;
using Articles.Domain.Entities;
using Articles.Services.DTO;
using Mapster;

namespace Articles.Services.Impl;

public class ArticlesService(
    IArticlesRepository articlesRepository,
    ISectionResolveService sectionResolveService,
    ISectionsRepository sectionsRepository) : IArticlesService
{
    public async Task<ArticleDto?> GetById(long id)
    {
        return (await articlesRepository.GetById(id)).Adapt<ArticleDto>();
    }

    private void DistinctTags(Article article)
    {
        article.Tags = article.Tags.Distinct(new TagEqualityComparer()).ToList();
    }

    public async Task<ArticleDto> Create(CreateArticleRequest createArticleRequest)
    {
        var article = createArticleRequest.Adapt<Article>();

        DistinctTags(article);
        article.Section = await sectionResolveService.ResolveSectionForArticleTags(article);
        
        await articlesRepository.Add(article);
        await sectionsRepository.AddSection(article.Section);

        return article.Adapt<ArticleDto>();
    }

    public async Task<bool> Update(long id, UpdateArticleRequest updateArticleRequest)
    {
        var article = updateArticleRequest.Adapt<Article>();
        article.Id = id;

        DistinctTags(article);
        article.Section = await sectionResolveService.ResolveSectionForArticleTags(article);
        
        try
        {
            await articlesRepository.Update(article);
            await sectionsRepository.AddSection(article.Section);
        }
        catch (ItemNotFoundException e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }
}