using Articles.Dal.PostgresEfCore.Models;
using Bogus;

namespace Articles.Tests;

public static class FakeDataFactory
{
    public static List<SectionEntity> CreateFakeSections(int count = 3)
    {
        var tagFaker = new Faker<TagEntity>()
            .RuleFor(t => t.Id, _ => 0)
            .RuleFor(t => t.Name, f => f.Random.String2(10));

        var articleFaker = new Faker<ArticleEntity>()
            .RuleFor(a => a.Id, _ => 0)
            .RuleFor(a => a.Title, f => f.Lorem.Sentence())
            .RuleFor(a => a.DateCreated, f => f.Date.Past().ToUniversalTime())
            .RuleFor(a => a.DateModified, f => f.Date.Past().ToUniversalTime());

        var sectionFaker = new Faker<SectionEntity>()
            .RuleFor(s => s.Id, _ => 0)
            .RuleFor(s => s.Name, f => f.Commerce.Department())
            .RuleFor(s => s.Articles, f => articleFaker.Generate(f.Random.Int(2, 5)))
            .RuleFor(s => s.Tags, f => tagFaker.Generate(f.Random.Int(3, 6)));

        return sectionFaker.Generate(count);
    }
}