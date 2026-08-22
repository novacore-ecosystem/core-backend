using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentPublicationConfig : IEntityTypeConfiguration<ContentPublication>
{
    public void Configure(EntityTypeBuilder<ContentPublication> builder)
    {
        // Table
        builder.ToTable("content_publications");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentId).IsRequired();
        builder.Property(x => x.VersionId).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.ScheduledAt);
        builder.Property(x => x.PublishedAt);
        builder.Property(x => x.UnpublishedAt);
        builder.Property(x => x.ExpiresAt);

        // Relationships
        builder.HasOne(x => x.Content)
            .WithMany(c => c.Publications)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Version)
            .WithMany()
            .HasForeignKey(x => x.VersionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ContentId);
        builder.HasIndex(x => x.VersionId);
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
