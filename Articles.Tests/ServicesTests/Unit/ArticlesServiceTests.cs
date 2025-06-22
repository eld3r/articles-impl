using Articles.Dal;
using Articles.Domain.Entities;
using Articles.Services.DTO;
using Articles.Services.Impl;
using Articles.Tests.Extensions;
using Articles.Tests.ServicesTests.Base;
using DeepEqual.Syntax;
using FakeItEasy;
using Mapster;

namespace Articles.Tests.ServicesTests;

[TestClass]
public class ArticlesServiceTests : ServicesTestsBase
{
    private static IArticlesRepository _articlesRepository = null!;
    private static ITagsRepository _tagsRepository = null!;

    private static readonly Article FakeArticle = new()
    {
        Id = 0,
        DateCreated = DateTime.Now,
        Title = "fakeArticle1",
        Tags = new List<Tag>()
        {
            new Tag() { Id = 1, Name = "fakeTag1" },
            new Tag() { Id = 2, Name = "fakeTag2" }
        }
    };

    private const string ExistingTagName = "bbb";
    private const long ExistingTagId = 100;

    [ClassInitialize]
    public static void Init(TestContext context)
    {
        _articlesRepository = A.Fake<IArticlesRepository>();
        _tagsRepository = A.Fake<ITagsRepository>();

        A.CallTo(() => _articlesRepository.GetById(A<long>._))
            .ReturnsLazily((long id) =>
            {
                var fakeResult = FakeArticle.Adapt<Article>();
                fakeResult.Id = id;
                return fakeResult;
            });

        A.CallTo(() => _articlesRepository.Add(A<Article>._))
            .ReturnsLazily((Article article) =>
            {
                var newArticle = new Article()
                {
                    Id = article.Id,
                    Title = article.Title,
                    Tags = article.Tags,
                    DateCreated = DateTime.Now,
                    DateModified = null
                };

                return Task.FromResult(newArticle);
            });
        
        A.CallTo(() => _articlesRepository.Update(A<Article>._))
            .ReturnsLazily((Article article) =>
            {
                var newArticle = new Article()
                {
                    Id = article.Id,
                    Title = article.Title,
                    Tags = article.Tags,
                    DateCreated = DateTime.Now,
                    DateModified = null
                };

                return Task.FromResult(newArticle);
            });

        A.CallTo(() => _tagsRepository.GetExistingTags(A<List<Tag>>._))
            .ReturnsLazily((List<Tag> tags) => new Dictionary<string, Tag>()
                { { ExistingTagName, new Tag() { Id = ExistingTagId, Name = ExistingTagName } } });
    }

    [TestMethod]
    public async Task GetArticleTest()
    {
        var target = new ArticlesService(_articlesRepository, _tagsRepository);

        var article = await target.GetById(42);

        article.ShouldNotBeNull().PrintToConsole();
        article.Id.ShouldBe(42);
        article.Title.ShouldBe(FakeArticle.Title);

        article.Tags.ShouldDeepEqual(FakeArticle.Tags.Select(s => s.Name));
    }

    [TestMethod]
    public async Task CreateArticleTest()
    {
        var target = new ArticlesService(_articlesRepository, _tagsRepository);

        var given = new CreateArticleRequest()
        {
            Title = "createdTitle",
            Tags = ["aaa", ExistingTagName, "ccc"]
        };

        await target.Create(given);

        A.CallTo(() => _articlesRepository.Add(A<Article>.That.Matches(article =>
                    article.Title == given.Title &&
                    article.Tags[0].Name == given.Tags[0] &&
                    article.Tags[1].Name == given.Tags[1] &&
                    article.Tags[2].Name == given.Tags[2] &&
                    article.Tags[1].Id == ExistingTagId
                )
            )
        ).MustHaveHappened();
    }
    
    [TestMethod]
    public async Task UpdateArticleTest()
    {
        var target = new ArticlesService(_articlesRepository, _tagsRepository);

        var given = new UpdateArticleRequest()
        {
            Id = 42,
            Title = "updatedTitle",
            Tags = ["aaa", ExistingTagName, "ccc"]
        };

        await target.Update(given);

        A.CallTo(() => _articlesRepository.Update(A<Article>.That.Matches(article =>
                    article.Title == given.Title &&
                    article.Id == given.Id &&
                    article.Tags[0].Name == given.Tags[0] &&
                    article.Tags[1].Name == given.Tags[1] &&
                    article.Tags[2].Name == given.Tags[2] &&
                    article.Tags[1].Id == ExistingTagId
                )
            )
        ).MustHaveHappened();
    }
}