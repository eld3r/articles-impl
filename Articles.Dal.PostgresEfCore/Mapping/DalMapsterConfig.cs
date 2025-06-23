using Articles.Dal.PostgresEfCore.Models;
using Articles.Domain.Entities;
using Mapster;

namespace Articles.Dal.PostgresEfCore.Mapping;

public class DalMapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<Tag, TagEntity>()
            .Map(s => s.Name, d => d.Name.ToLower())
            ;
        
        config.ForType<Section, SectionEntity>()
            .Ignore(s => s.Tags)
            ;

        config.ForType<Article, ArticleEntity>()
            .Ignore(a => a.TagLinks)
            .Ignore(a => a.DateCreated)
            ;

        config.ForType<ArticleEntity, Article>()
            .Map(d => d.Tags,
                s => s.TagLinks.Select(tagLink => new Tag()
                {
                    Id = tagLink.TagId,
                    Name = tagLink.Tag == null ? string.Empty : tagLink.Tag.Name
                }))
            ;
    }
}