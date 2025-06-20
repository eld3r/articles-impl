
using Articles.Dal.PostgresEfCore.Models;
using Microsoft.EntityFrameworkCore;

namespace Articles.Dal.PostgresEfCore;

public class ArticlesDbContext : DbContext
{
    public ArticlesDbContext(DbContextOptions<ArticlesDbContext> options)
        : base(options) { }

    public DbSet<TagEntity> Tags => Set<TagEntity>();
    public DbSet<ArticleEntity> Articles => Set<ArticleEntity>();
    public DbSet<SectionEntity> Sections => Set<SectionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArticlesDbContext).Assembly);
    }
}