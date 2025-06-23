using Articles.Dal;
using Articles.Domain.Entities;

namespace Articles.Services.Impl;

public class SectionResolverService(ISectionsRepository sectionsRepository) : ISectionResolveService
{
    public async Task<Section> ResolveSectionForArticleTags(List<Tag> tags)
    {
        tags.ForEach(t => t.Name = t.Name.ToLower());
        
        var section = await sectionsRepository.FindSectionByTags(tags);

        if (section != null)
            return section;

        var orderedTags = tags.OrderBy(s => s.Name).ToList();
        section = new Section()
        {
            Name = string.Join(',', orderedTags.Select(s => s.Name)),
            Tags = orderedTags
        };
        
        return section;
    }
}