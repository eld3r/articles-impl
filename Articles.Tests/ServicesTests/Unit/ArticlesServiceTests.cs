using Articles.Dal;
using Articles.Domain.Entities;
using Articles.Services;
using Articles.Services.DTO;
using Articles.Services.Impl;
using Articles.Tests.Extensions;
using DeepEqual.Syntax;
using FakeItEasy;
using Mapster;

namespace Articles.Tests.ServicesTests.Unit;

[TestClass]
[TestCategory("Unit")]
public class ArticlesServiceTests
{
    private static IArticlesRepository _articlesRepository = null!;
    private static ISectionsRepository _sectionsRepository = null!;
    private static ISectionResolveService _sectionResolveService = null!;

    private static readonly Article FakeArticle = new()
    {
        Id = 0,
        DateCreated = DateTime.Now,
        Title = "fakeArticle1",
        Tags =
        [
            new Tag() { Id = 1, Name = "fakeTag1" },
            new Tag() { Id = 2, Name = "fakeTag2" }
        ]
    };

    [ClassInitialize]
    public static void Init(TestContext context)
    {
        _articlesRepository = A.Fake<IArticlesRepository>();
        _sectionResolveService = A.Fake<ISectionResolveService>();
        _sectionsRepository = A.Fake<ISectionsRepository>();

        A.CallTo(() => _sectionsRepository.AddOrUpdateSection(A<Section>._))
            .ReturnsLazily((Section section) => 1);

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

        A.CallTo(() => _sectionResolveService.ResolveSectionForArticleTags(A<Article>._))
            .ReturnsLazily((Article article) =>
            {
                var newSection = new Section()
                {
                    Id = 1,
                    Name = "newSection"
                };
                return Task.FromResult(newSection);
            });
    }

    [TestMethod]
    public async Task GetArticleTest()
    {
        var target = new ArticlesService(_articlesRepository, _sectionResolveService, _sectionsRepository);

        var article = await target.GetById(42);

        article.ShouldNotBeNull().PrintToConsole();
        article.Id.ShouldBe(42);
        article.Title.ShouldBe(FakeArticle.Title);

        article.Tags.ShouldDeepEqual(FakeArticle.Tags.Select(s => s.Name));
    }

    [TestMethod]
    public async Task CreateArticleTest()
    {
        var target = new ArticlesService(_articlesRepository, _sectionResolveService, _sectionsRepository);

        var given = new CreateArticleRequest()
        {
            Title = "createdTitle",
            Tags = ["aaa", "bbb", "ccc"]
        };

        await target.Create(given);

        A.CallTo(() => _articlesRepository.Add(A<Article>.That.Matches(article =>
                    article.Title == given.Title &&
                    article.Tags[0].Name == given.Tags[0] &&
                    article.Tags[1].Name == given.Tags[1] &&
                    article.Tags[2].Name == given.Tags[2] 
                )
            )
        ).MustHaveHappened();
    }
    
    [TestMethod]
    public async Task UpdateArticleTest()
    {
        var target = new ArticlesService(_articlesRepository, _sectionResolveService, _sectionsRepository);

        var given = new UpdateArticleRequest()
        {
            Title = "updatedTitle",
            Tags = ["aaa", "bbb", "ccc"]
        };

        await target.Update(42, given);

        A.CallTo(() => _articlesRepository.Update(A<Article>.That.Matches(article =>
                    article.Title == given.Title &&
                    article.Id == 42 &&
                    article.Tags[0].Name == given.Tags[0] &&
                    article.Tags[1].Name == given.Tags[1] &&
                    article.Tags[2].Name == given.Tags[2] 
                )
            )
        ).MustHaveHappened();
    }
}