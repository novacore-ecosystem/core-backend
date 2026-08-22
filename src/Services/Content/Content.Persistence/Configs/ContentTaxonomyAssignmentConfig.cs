using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentTaxonomyAssignmentConfig : IEntityTypeConfiguration<ContentTaxonomyAssignment>
{
    public void Configure(EntityTypeBuilder<ContentTaxonomyAssignment> builder)
    {
        // Table
        builder.ToTable("content_taxonomy_assignments");

        // Properties
        builder.HasKey(x => new { x.ContentId, x.TaxonomyId });

        // Relationships
        builder.HasOne(x => x.Content)
            .WithMany(c => c.TaxonomyAssignments)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Taxonomy)
            .WithMany()
            .HasForeignKey(x => x.TaxonomyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.TaxonomyId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
