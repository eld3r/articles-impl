using Articles.Dal.Exceptions;
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

    private List<ArticleEntity> MapWithEnrichArticles(List<Article> articles) =>
        articles.Select(s =>
                s.Id == 0
                    ? s.Adapt<ArticleEntity>()
                    : _dbContext.Articles.FirstOrDefault(a => a.Id == s.Id) ??
                      throw new ItemNotFoundException("Article that should be found was not found"))
            .ToList();


    public Task<long> AddOrUpdateSection(Section section) =>
        section.Id == 0
            ? AddSection(section)
            : UpdateSection(section);

    private async Task<long> AddSection(Section section)
    {
        var entity = section.Adapt<SectionEntity>();

        entity.Tags = await MapWithEnrichTags(section.Tags);
        entity.Articles = MapWithEnrichArticles(section.Articles);

        await _dbContext.Sections.AddAsync(entity);

        await _dbContext.SaveChangesAsync();
        section.Id = entity.Id;
        return entity.Id;
    }

    private async Task<long> UpdateSection(Section section)
    {
        var entity = await _dbContext.Sections.FirstOrDefaultAsync(f => f.Id == section.Id);
        if (entity == null)
            throw new ItemNotFoundException("Section that should be found was not found");

        entity.Articles = MapWithEnrichArticles(section.Articles);
        await _dbContext.SaveChangesAsync();
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

    private async Task<List<TagEntity>> MapWithEnrichTags(List<Tag> tags)
    {
        var existingTags = await LoadExistingTagsAsync(tags);

        return tags.Select(tag =>
            !existingTags.TryGetValue(tag.Name, out var existingTag) ? tag.Adapt<TagEntity>() : existingTag).ToList();
    }
}