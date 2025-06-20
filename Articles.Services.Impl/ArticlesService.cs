using Articles.Dal;
using Articles.Domain.Entities;

namespace Articles.Services.Impl;

public class ArticlesService(IArticlesRepository articlesRepository) : IArticlesService
{
    public Article GetById(long id)
    {
        throw new NotImplementedException();
    }

    public long Save(Article article)
    {
        throw new NotImplementedException();
    }

    public void Update(Article article)
    {
        throw new NotImplementedException();
    }
}