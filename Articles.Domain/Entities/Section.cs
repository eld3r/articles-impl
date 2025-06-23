using System.Collections.Generic;

namespace Articles.Domain.Entities;

public class Section
{
    public long Id { get; set; }
    public string Name { get; set; }
    
    public int ArticlesCount { get; set; }
    
    public List<Tag> Tags { get; set; } = new();
    
    public List<Article> Articles { get; set; } = new();
}