using Articles.Domain.Entities;

namespace Articles.Dal;

public interface ITagsRepository
{
    Task AddTags(List<Tag> tags);
    Task<Tag?> GetTagById(long id);
    Task<Dictionary<string, Tag>> GetExistingTags(List<Tag> articleTags);
}