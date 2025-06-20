using Articles.Domain.Entities;

namespace Articles.Dal;

public interface IArticlesRepository
{
    public Task<Article?> GetById(long id);
    public Task Add(Article article);
    public Task Update(Article article);
}