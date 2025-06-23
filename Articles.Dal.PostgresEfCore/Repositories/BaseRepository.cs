using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Articles.Dal.PostgresEfCore.Repositories;

public class BaseRepository(ArticlesDbContext dbContext)
{
    protected async Task<Dictionary<string, TagEntity>> LoadExistingTagsAsync(List<Tag> tags) =>
        await dbContext.Tags.Where(w => tags
                .Select(s => s.Name)
                .Contains(w.Name))
            .ToDictionaryAsync(k => k.Name);
}