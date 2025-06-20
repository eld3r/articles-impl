using Articles.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Articles.Dal.PostgresEfCore.Repositories;

public class SectionsRepository(ArticlesDbContext dbContext) : ISectionsRepository
{
    public async Task<List<Section>> GetSections()
    {
        var result = (await dbContext
            .Sections
            .Include(q => q.Tags)
            .OrderByDescending(s => s.Tags.Count())
            .ToListAsync());
        return result
            .Adapt<List<Section>>();
    }

    public async Task<List<Article>> GetArticlesBySection(long sectionId)
    {
        throw new NotImplementedException();
    }
}