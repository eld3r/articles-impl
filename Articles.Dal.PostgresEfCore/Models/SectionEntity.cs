namespace Articles.Dal.PostgresEfCore.Models;

public class SectionEntity
{
    public long Id { get; set; }
    public string Name { get; set; }
    public ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
}