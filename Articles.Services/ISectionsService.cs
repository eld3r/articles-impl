using Articles.Domain.Entities;

namespace Articles.Services;

public interface ISectionsService
{
    List<Section> GetSections();
    List<Article> GetArticlesBySection(long sectionId);
}