using System.ComponentModel.DataAnnotations;
using Articles.Services.Attributes;

namespace Articles.Services.Contract;

public class CreateArticleRequest
{
    [Required]
    public string Title { get; set; } = null!;

    [MaxLength(256)]
    [EachTagMaxLenght(256)]
    public string[] Tags { get; set; } = [];
}

public class UpdateArticleRequest
{
    [Required]
    public string Title { get; set; } = null!;
    
    [MaxLength(256)]
    [EachTagMaxLenght(256)]
    public string[] Tags { get; set; } = [];
}

public class ArticleDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public List<string> Tags { get; set; } = [];

    public DateTime DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
}