using Articles.Domain.Entities;

namespace Articles.Dal;

public interface ISectionsRepository
{
    Task<List<Section>> GetAllSections();
    Task<Section> GetSectionById(long sectionId);
    Task<long> AddOrUpdateSection(Section section);
    Task<Section?> FindSectionByTags(List<Tag> tags);
}