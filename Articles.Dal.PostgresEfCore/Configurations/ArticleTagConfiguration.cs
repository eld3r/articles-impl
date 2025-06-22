using Articles.Dal.PostgresEfCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Articles.Dal.PostgresEfCore.Configurations;

public class ArticleTagConfiguration : IEntityTypeConfiguration<ArticleTagEntity>
{
    public void Configure(EntityTypeBuilder<ArticleTagEntity> builder)
    {
        builder.HasKey(a => a.Id);
    }
}