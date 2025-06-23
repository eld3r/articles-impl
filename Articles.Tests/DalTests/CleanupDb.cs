using Articles.Tests.DalTests.Base;

namespace Articles.Tests.DalTests;

[TestClass]
[TestCategory("Other")]
public class CleanupDb : DbInitiateTestProfileBase
{
    [Ignore("Используется для дебага")]
    [TestMethod]
    public async Task CleanupDd()
    {
        await WithNewScopedDbContext(async db =>
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        });
    }
}