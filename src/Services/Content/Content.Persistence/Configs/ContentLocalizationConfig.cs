using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentLocalizationConfig : IEntityTypeConfiguration<ContentLocalization>
{
    public void Configure(EntityTypeBuilder<ContentLocalization> builder)
    {
        // Table
        builder.ToTable("content_localizations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentId).IsRequired();

        builder.Property(x => x.Culture)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.VersionId).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Content)
            .WithMany(c => c.Localizations)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Version)
            .WithMany()
            .HasForeignKey(x => x.VersionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.ContentId, x.Culture }).IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
