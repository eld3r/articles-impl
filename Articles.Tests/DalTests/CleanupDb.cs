using Articles.Tests.DalTests.Base;

namespace Articles.Tests.DalTests;

[TestClass]
public class CleanupDb : DbInitiateTestProfileBase
{
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