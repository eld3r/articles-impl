namespace Articles.Services.DTO;

public class SectionDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public List<string> Tags { get; set; } = new();
    public int ArticlesCount { get; set; } //может и не нужно
}

public class SectionDetailedDto : SectionDto
{
    public List<ArticleDto> Articles { get; set; } = new();
}