using Articles.Domain.Entities;
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
    }
}