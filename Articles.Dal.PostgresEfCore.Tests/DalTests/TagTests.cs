using Articles.Dal.PostgresEfCore.Models;
using Articles.Dal.PostgresEfCore.Tests.Unit.Base;
using Articles.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Articles.Dal.PostgresEfCore.Tests.Unit;

[TestClass]
public class TagTests : DbInitiateTestProfileBase
{
    [TestInitialize]
    public async Task TestInit()
    {
        var dbContext = ServiceProvider.GetRequiredService<ArticlesDbContext>();
        dbContext.Tags.RemoveRange(dbContext.Tags);
        await dbContext.SaveChangesAsync();
    }
    
    private static async Task TagRepoInNewScope(Func<ITagsRepository, Task> action)
    {
        await WithNewScope(action);
    }
    
    [TestMethod]
    public async Task AddTagTest()
    {
        var given = new List<Tag>() { new() { Name = "test" }, new() { Name = "test2" } };
        await TagRepoInNewScope(async target =>
        {
            await target.AddTags(given);
        });

        await WithNewScopedDbContext(async dbContext =>
        {
            var result = await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == given[1].Id);

            result.ShouldNotBeNull();
            result.Name.ShouldBe(given[1].Name);
            result.Id.ShouldBe(given[1].Id);
            result.PrintToConsole();
        });
    }

    [TestMethod]
    public async Task GetTagByIdTest()
    {
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ArticlesDbContext>();
        var given = new TagEntity() { Name = "test" };
        await dbContext.Tags.AddAsync(given);
        await dbContext.SaveChangesAsync();
        
        using var anotherScope = ServiceProvider.CreateScope();
        var target = anotherScope.ServiceProvider.GetRequiredService<ITagsRepository>();
        var result = await target.GetTagById(given.Id);
        
        result.ShouldNotBeNull();
        result.Name.ShouldBe(given.Name);
        result.Id.ShouldBe(given.Id);
        result.PrintToConsole();
    }

    [TestMethod]
    public async Task NullArgTest()
    {
        using var scope = ServiceProvider.CreateScope();
        var target = scope.ServiceProvider.GetRequiredService<ITagsRepository>();

        await Should.ThrowAsync<ArgumentNullException>(()=> target.AddTags(null!));
    }
    
    [TestMethod]
    public async Task TooLooongTagTest()
    {
        using var scope = ServiceProvider.CreateScope();
        var target = scope.ServiceProvider.GetRequiredService<ITagsRepository>();

        var tooLongTag = new Tag()
        {
            Name = new string(Enumerable
                .Range(1, 257)
                .Select(_ => 'a')
                .ToArray())
        }.PrintToConsole();

        tooLongTag.Name.Length.PrintToConsole("Length of tag");

        await Should.ThrowAsync<DbUpdateException>(()=> target.AddTags([tooLongTag]));
    }
    
    [TestMethod]
    public async Task TooLooongTagTestNotInsertsAll()
    {
        using var scope = ServiceProvider.CreateScope();
        var target = scope.ServiceProvider.GetRequiredService<ITagsRepository>();

        var tooLongTag = new Tag()
        {
            Name = new string(Enumerable
                .Range(1, 257)
                .Select(_ => 'a')
                .ToArray())
        }.PrintToConsole();

        tooLongTag.Name.Length.PrintToConsole("Length of tag");

        await Should.ThrowAsync<DbUpdateException>(()=> target.AddTags([tooLongTag, new Tag(){Name = "test"}]));

        await WithNewScopedDbContext(async db =>
        {
            var tags = await db.Tags.ToListAsync();
            tags.ShouldBeEmpty();
        });
    }
    
    [TestMethod]
    public async Task GetNotExistingTagTest()
    {
        using var scope = ServiceProvider.CreateScope();
        var target = scope.ServiceProvider.GetRequiredService<ITagsRepository>();

        var result = await target.GetTagById(-1);
        
        result.ShouldBeNull();
    }
    
    [TestMethod]
    public async Task CaseIgnoreCoupledTest()
    {
        using var scope = ServiceProvider.CreateScope();
        var target = scope.ServiceProvider.GetRequiredService<ITagsRepository>();

        await target.AddTags([new Tag() { Name = "test" }, new Tag() { Name = "TEST" }]);
        
        using var anotherScope = ServiceProvider.CreateScope();
        var allTags = await anotherScope.ServiceProvider.GetRequiredService<ArticlesDbContext>().Tags.ToListAsync();
        allTags.ShouldHaveSingleItem();
    }
    
    [TestMethod]
    public async Task CaseIgnoreSeparateTest()
    {
        using var scope = ServiceProvider.CreateScope();
        var target = scope.ServiceProvider.GetRequiredService<ITagsRepository>();

        var tag = new Tag() { Name = "test" };
        await target.AddTags([tag]);

        await WithNewScope<ITagsRepository>(async repository =>
        {
            var anotherTag = new Tag() { Name = "TEST" };
            await repository.AddTags([anotherTag]);
        });

        await WithNewScopedDbContext(async dbContext =>
        {
            var allTags = await dbContext.Tags.ToListAsync();
            allTags.ShouldHaveSingleItem();
        });
    }
}