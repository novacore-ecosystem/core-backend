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
        builder.Property(x => x.VersionId).IsRequired();

        builder.Property(x => x.Culture)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(1000);

        // Editor.js-compatible structured JSON document - stored as jsonb, not text, so Postgres
        // can index/query into it later without a migration; the domain still treats it as an
        // opaque validated string (see ContentLocalization.IsValidBody).
        builder.Property(x => x.Body)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => MetadataBase.FromJson<ContentMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Content)
            .WithMany(c => c.Localizations)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Version)
            .WithMany(v => v.Localizations)
            .HasForeignKey(x => x.VersionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => new { x.VersionId, x.Culture }).IsUnique();
        builder.HasIndex(x => new { x.ContentId, x.Culture });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
