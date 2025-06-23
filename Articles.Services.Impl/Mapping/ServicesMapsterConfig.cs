using Articles.Domain.Entities;
using Articles.Services.DTO;
using Mapster;

namespace Articles.Services.Impl.Mapping;

public class ServicesMapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<string, Tag>()
            .MapWith(s => new Tag() { Name = s.ToLower() });

        config.ForType<Tag, string>()
            .MapWith(s => $"{s.Name}");

        config.ForType<Section, SectionDetailedDto>()
            .Map(d => d.Articles,
                s => s.Articles.OrderByDescending(d => d.DateModified ?? d.DateCreated));
    }
}