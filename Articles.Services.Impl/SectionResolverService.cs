using Articles.Dal;
using Articles.Domain.Entities;

namespace Articles.Services.Impl;

public class SectionResolverService(ISectionsRepository sectionsRepository) : ISectionResolveService
{
    public async Task<Section> ResolveSectionForArticleTags(Article article)
    {
        article.Tags.ForEach(t => t.Name = t.Name.ToLower());
        
        var section = await sectionsRepository.FindSectionByTags(article.Tags);

        if (section != null)
        {
            section.Articles.Add(article);
            return section;
        }
        
        var orderedTags = article.Tags.OrderBy(s => s.Name).ToList();
        section = new Section()
        {
            Name = string.Join(',', orderedTags.Select(s => s.Name)),
            Tags = orderedTags,
            Articles = new List<Article>() { article }
        };
        
        return section;
    }
}