using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentVersionConfig : IEntityTypeConfiguration<ContentVersion>
{
    public void Configure(EntityTypeBuilder<ContentVersion> builder)
    {
        // Table
        builder.ToTable("content_versions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentId).IsRequired();
        builder.Property(x => x.VersionNumber).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(1000);
        builder.Property(x => x.Body).HasColumnType("text");

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x.ToJson(),
                x => MetadataBase.FromJson<ContentMetadata>(x))
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedBy).IsRequired();

        // Relationships
        builder.HasOne(x => x.Content)
            .WithMany(c => c.Versions)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.ContentId, x.VersionNumber }).IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
