using Articles.Domain.Entities;

namespace Articles.Services;

public interface ISectionResolveService
{
    Task ResolveSectionForArticle(Article article);
}