using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentConfig : IEntityTypeConfiguration<ContentEntity>
{
    public void Configure(EntityTypeBuilder<ContentEntity> builder)
    {
        // Table
        builder.ToTable("contents");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug)
            .HasConversion(x => x.Value, x => ContentSlug.Create(x))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.Visibility)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.ContentTypeId)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.UpdatedBy);
        builder.Property(x => x.PublishedAt);
        builder.Property(x => x.ArchivedAt);

        // Relationships
        builder.HasOne(x => x.ContentType)
            .WithMany()
            .HasForeignKey(x => x.ContentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // CurrentVersion/PublishedVersion point into the owned Versions collection (which
        // cascades from this same row) - NoAction avoids a circular immediate-check against
        // the Cascade delete on Versions when a Content row is deleted; Postgres resolves the
        // full cascade set together so the pointer is gone by the time either check matters.
        builder.HasOne(x => x.CurrentVersion)
            .WithMany()
            .HasForeignKey(x => x.CurrentVersionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.PublishedVersion)
            .WithMany()
            .HasForeignKey(x => x.PublishedVersionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Versions/Publications/WorkflowInstances/Relationships/Localizations/TaxonomyAssignments/
        // Audiences/Contributors are all configured from the child entity's own config (single
        // source per relationship).

        // Indexes
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.ContentTypeId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Visibility);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
