using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UrlMappingConfiguration : IEntityTypeConfiguration<UrlMapping>
{
    public void Configure(EntityTypeBuilder<UrlMapping> builder)
    {
        builder.ToTable("url_mappings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LongUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.ShortCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(x => x.ShortCode)
            .IsUnique();

        builder.HasIndex(x => x.LongUrl)
            .IsUnique();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ClickCount)
            .HasDefaultValue(0);
    }
}