using Articles.Dal.PostgresEfCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Articles.Dal.PostgresEfCore;

public class CreateUpdateDateInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is null)
            return base.SavingChanges(eventData, result);

        var utcNow = DateTime.UtcNow;
        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Entity is not ArticleEntity article)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    article.DateCreated = utcNow;
                    break;
                case EntityState.Modified:
                    article.DateModified = utcNow;
                    break;
            }
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.FromResult(SavingChanges(eventData, result));
    }
}