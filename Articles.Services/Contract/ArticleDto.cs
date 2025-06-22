namespace Articles.Services.DTO;

public class CreateArticleRequest
{
    public string Title { get; set; } = null!;
    public List<string> Tags { get; set; } = [];
}

public class UpdateArticleRequest
{
    public string Title { get; set; } = null!;
    public List<string> Tags { get; set; } = [];
}

public class ArticleDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public List<string> Tags { get; set; } = [];
    public DateTime DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
}

