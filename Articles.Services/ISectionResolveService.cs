using Articles.Domain.Entities;

namespace Articles.Services;

public interface ISectionResolveService
{
    Task<Section> ResolveSectionForArticleTags(Article tags);
}