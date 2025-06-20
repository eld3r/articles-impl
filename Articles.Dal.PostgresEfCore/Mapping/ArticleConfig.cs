using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Mapster;
namespace Articles.Dal.PostgresEfCore.Mapping;

public class ArticleConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<Article, ArticleEntity>()
            .Ignore(x => x.Tags);
    }
}