using Articles.Dal.PostgresEfCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Articles.Dal.PostgresEfCore.Configurations;

public class SectionConfiguration: IEntityTypeConfiguration<SectionEntity>
{
    public void Configure(EntityTypeBuilder<SectionEntity> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(1024);
    }
}