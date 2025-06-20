using Articles.Domain.Entities;

namespace Articles.Dal;

public interface ITagRepository
{
    Task AddTags(List<Tag> tags);
    Task<Tag?> GetTagById(long id);
}