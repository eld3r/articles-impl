using Articles.Services.Contract;

namespace Articles.Services;

public interface IArticlesService
{
    public Task<ArticleDto?> GetById(long id);
    public Task<ArticleDto> Create(CreateArticleRequest article);
    public Task<bool> Update(long id, UpdateArticleRequest article);
}