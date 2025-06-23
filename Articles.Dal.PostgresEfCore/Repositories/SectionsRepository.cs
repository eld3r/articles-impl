using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain;
using Articles.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Articles.Dal.PostgresEfCore.Repositories;

public class SectionsRepository(ArticlesDbContext dbContext) : BaseRepository(dbContext), ISectionsRepository
{
    private readonly ArticlesDbContext _dbContext = dbContext;

    public async Task<List<Section>> GetAllSections()
    {
        var result = await _dbContext
            .Sections
            .Include(q => q.Tags)
            .AsNoTracking()
            .OrderByDescending(s => s.Articles.Count)
            .Select(s => ValueTuple.Create(s, s.Articles.Count))
            .ToListAsync();

        return result.Adapt<List<Section>>();
    }

    public async Task<Section> GetSectionById(long sectionId)
    {
        var result = await _dbContext
            .Sections
            .Include(s => s.Articles)
            .Include(s => s.Tags)
            .FirstOrDefaultAsync(s => s.Id == sectionId);

        return result.Adapt<Section>();
    }

    public async Task<long> AddSection(Section section)
    {
        //Обновление раздела не предполагается, можно бонусом где-то выше провалидировать тэги и название, 
        //но пока не видится возможность такой коллизии
        if (section.Id > 0)
            return section.Id;

        var entity = section.Adapt<SectionEntity>();

        entity.Tags = await MapWithEnrich(section.Tags);

        entity.Articles = section.Articles.Select(s =>
            s.Id == 0
                ? s.Adapt<ArticleEntity>()
                : _dbContext.Articles.FirstOrDefault(a => a.Id == s.Id) ?? s.Adapt<ArticleEntity>()).ToList();
        ;

        await _dbContext.Sections.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        section.Id = entity.Id;
        return entity.Id;
    }

    public async Task<Section?> FindSectionByTags(List<Tag> tags)
    {
        tags = tags.Distinct(new TagEqualityComparer()).ToList();
        tags.ForEach(t => t.Name = t.Name.ToLower());

        var result = await (from section in _dbContext.Sections
                .Include(q => q.Tags)
                .Include(q => q.Articles)
            where
                section.Tags.Count == tags.Count &&
                !section.Tags.Select(s => s.Name).Except(tags.Select(s => s.Name)).Any()
                && !tags.Select(s => s.Name).Except(section.Tags.Select(s => s.Name)).Any()
            select section).AsNoTracking().FirstOrDefaultAsync();

        return result.Adapt<Section>();
    }

    private async Task<List<TagEntity>> MapWithEnrich(List<Tag> tags)
    {
        var existingTags = await LoadExistingTagsAsync(tags);

        return tags.Select(tag =>
            !existingTags.TryGetValue(tag.Name, out var existingTag) ? tag.Adapt<TagEntity>() : existingTag).ToList();
    }
}