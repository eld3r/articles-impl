namespace Articles.Services.DTO;

public class CreateArticleRequest
{
    public string Title { get; set; } = null!;
    public List<string> Tags { get; set; } = [];
}

public class UpdateArticleRequest
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public List<string> Tags { get; set; } = [];
}

public class ArticleDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public List<string> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

