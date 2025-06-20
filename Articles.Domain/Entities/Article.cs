using System;
using System.Collections.Generic;

namespace Articles.Domain.Entities;

public class Article
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public DateTime DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
    public List<Tag> Tags { get; set; } = [];
}