using Articles.Services.DTO;

namespace Articles.Services;

public interface ISectionsService
{
    Task<List<SectionDto>> GetAllSections();
    Task<SectionDetailedDto?> GetDetailedSection(long sectionId);
}