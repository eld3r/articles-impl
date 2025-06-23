using Articles.Dal;
using Articles.Services.DTO;
using Mapster;

namespace Articles.Services.Impl;

public class SectionsService(ISectionsRepository sectionsRepository): ISectionsService
{
    public async Task<List<SectionDto>> GetAllSections() =>
        (await sectionsRepository.GetAllSections()).Adapt<List<SectionDto>>();

    public async Task<SectionDetailedDto?> GetDetailedSection(long sectionId) =>
        (await sectionsRepository.GetSectionById(sectionId)).Adapt<SectionDetailedDto>();
}