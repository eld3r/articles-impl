using Articles.Domain.Entities;

namespace Articles.Services;

public interface IArticlesService
{
    public Article GetById(long id);
    public long Save(Article article);
    public void Update(Article article);
}