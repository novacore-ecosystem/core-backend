using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentTaxonomyConfig : IEntityTypeConfiguration<ContentTaxonomy>
{
    public void Configure(EntityTypeBuilder<ContentTaxonomy> builder)
    {
        // Table
        builder.ToTable("content_taxonomies");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .HasConversion(x => x.Value, x => ContentKey.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        // Relationships - self-referencing hierarchy. Restrict prevents deleting a node while
        // it still has children, forcing an explicit reparent/delete of the subtree first.
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Key).IsUnique();
        builder.HasIndex(x => x.ParentId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
