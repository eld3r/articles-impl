namespace Articles.Dal.PostgresEfCore.Models;

public class TagEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<SectionEntity> Sections { get; set; } = new List<SectionEntity>();
    public ICollection<ArticleTagEntity> Articles { get; set; } = new List<ArticleTagEntity>();
}