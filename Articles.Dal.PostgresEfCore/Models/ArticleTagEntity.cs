namespace Articles.Dal.PostgresEfCore.Models;

public class ArticleTagEntity
{
    public long Id { get; set; }
    
    public long ArticleId { get; set; }
    public ArticleEntity Article { get; set; } = null!;
    public long TagId { get; set; }
    public TagEntity? Tag { get; set; } = null!;

    public int Order { get; set; }
}