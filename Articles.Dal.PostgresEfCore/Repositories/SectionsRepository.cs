using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Articles.Dal.PostgresEfCore.Repositories;

public class SectionsRepository(ArticlesDbContext dbContext) : ISectionsRepository
{
    public async Task<List<Section>> GetAllSections()
    {
        var result = await dbContext
            .Sections
            .OrderByDescending(s => s.Articles.Count)
            .ToListAsync();
        
        return result.Adapt<List<Section>>();
    }

    public async Task<Section> GetSectionById(long sectionId)
    {
        var result = await dbContext
            .Sections
            .Include(s => s.Articles)
            .FirstOrDefaultAsync(s => s.Id == sectionId);
        
        return result.Adapt<Section>();
    }

    public async Task<long> AddSection(Section section)
    {
        var entity = section.Adapt<SectionEntity>();
        await dbContext.Sections.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        section.Id = entity.Id;
        return entity.Id;
    }

    public async Task RemoveSection(long id)
    {
        var entity = await dbContext.Sections.FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null)
            throw new InvalidOperationException($"Section with id {id} was not found");

        dbContext.Remove(entity);
        await dbContext.SaveChangesAsync();
    }
}