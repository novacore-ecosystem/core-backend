using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentRelationshipConfig : IEntityTypeConfiguration<ContentRelationship>
{
    public void Configure(EntityTypeBuilder<ContentRelationship> builder)
    {
        // Table
        builder.ToTable("content_relationships");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceContentId).IsRequired();
        builder.Property(x => x.TargetType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TargetId).IsRequired();

        builder.Property(x => x.RelationshipType)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.Metadata).HasColumnType("jsonb");

        // Relationships
        builder.HasOne(x => x.SourceContent)
            .WithMany(c => c.Relationships)
            .HasForeignKey(x => x.SourceContentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.SourceContentId);
        builder.HasIndex(x => new { x.TargetType, x.TargetId });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
