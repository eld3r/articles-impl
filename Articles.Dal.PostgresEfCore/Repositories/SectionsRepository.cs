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

        entity.Tags = await MapWithEnrich(section.Tags);

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

    public async Task<Section?> FindSectionByTags(List<Tag> tags)
    {
        var result = await (from section in dbContext.Sections.Include(q => q.Tags)
            where
                !section.Tags.Select(s => s.Name).Except(tags.Select(s => s.Name)).Any()
                && !tags.Select(s => s.Name).Except(section.Tags.Select(s => s.Name)).Any()
            select section).AsNoTracking().FirstOrDefaultAsync();

        return result.Adapt<Section>();
    }

    private async Task<List<TagEntity>> MapWithEnrich(List<Tag> tags)
    {
        var existingTags = await dbContext.Tags.Where(w => tags
                .Select(s => s.Name)
                .Contains(w.Name))
            .ToDictionaryAsync(k => k.Name);

        return tags.Select(tag => !existingTags.TryGetValue(tag.Name, out var existingTag) ? tag.Adapt<TagEntity>() : existingTag).ToList();
    }
}