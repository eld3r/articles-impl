using Articles.Domain.Entities;

namespace Articles.Dal;

public interface ITagsRepository
{
    Task<Dictionary<string, Tag>> GetExistingTags(List<Tag> articleTags);
}