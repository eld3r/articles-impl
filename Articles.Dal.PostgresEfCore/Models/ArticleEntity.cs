namespace Articles.Dal.PostgresEfCore.Models;

public class ArticleEntity
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public DateTime DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
    public ICollection<ArticleTagEntity> TagLinks { get; set; } = new List<ArticleTagEntity>();
    public SectionEntity? Section { get; set; } = null!;
}