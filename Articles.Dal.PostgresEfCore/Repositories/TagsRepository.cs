using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Articles.Dal.PostgresEfCore.Repositories;

public class TagsRepository(ArticlesDbContext dbContext) : ITagsRepository
{
    public async Task<Dictionary<string, Tag>> GetExistingTags(List<Tag> articleTags)
    {
        var existingTags = await dbContext.Tags.Where(w => articleTags
                .Select(s => s.Name)
                .Contains(w.Name))
            .AsNoTracking()
            .ToDictionaryAsync(k => k.Name);

        return existingTags.Adapt<Dictionary<string, Tag>>();
    }
}