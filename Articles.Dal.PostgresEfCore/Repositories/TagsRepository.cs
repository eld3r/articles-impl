using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Articles.Dal.PostgresEfCore.Repositories;

public class TagsRepository(ArticlesDbContext dbContext) : ITagRepository
{
    public async Task AddTags(List<Tag> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);
        
        tags.ForEach(tag => tag.Name = tag.Name.ToLower());
        
        //TODO поиск существующих тэгов
        
        var entities = tags.Adapt<List<TagEntity>>();
        var tagMap = tags.Zip(entities).ToList();
        
        dbContext.Tags.AddRange(entities);
        await dbContext.SaveChangesAsync();
        
        foreach (var (tag, entity) in tagMap)
        {
            tag.Id = entity.Id;
        }
    }

    public async Task<Tag?> GetTagById(long id)
    {
        return (await dbContext.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id)).Adapt<Tag>();
    }
}