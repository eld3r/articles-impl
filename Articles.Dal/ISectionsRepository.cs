using Articles.Domain.Entities;

namespace Articles.Dal;

public interface ISectionsRepository
{
    Task<List<Section>> GetSections();
    Task<List<Article>> GetArticlesBySection(long sectionId);
}