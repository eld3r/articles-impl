﻿using Articles.Dal;
using Articles.Dal.Exceptions;
using Articles.Domain;
using Articles.Domain.Entities;
using Articles.Services.Contract;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Articles.Services.Impl;

public class ArticlesService(
    IArticlesRepository articlesRepository,
    ISectionResolveService sectionResolveService,
    ISectionsRepository sectionsRepository,
    ILogger<ArticlesService> logger) : IArticlesService
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
        await sectionsRepository.AddOrUpdateSection(article.Section);

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
            await sectionsRepository.AddOrUpdateSection(article.Section);
        }
        catch (ItemNotFoundException e)
        {
            logger.LogError(e, "Article could not be updated");
            return false;
        }

        return true;
    }
}