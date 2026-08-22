using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentAudienceConfig : IEntityTypeConfiguration<ContentAudience>
{
    public void Configure(EntityTypeBuilder<ContentAudience> builder)
    {
        // Table
        builder.ToTable("content_audiences");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentId).IsRequired();

        builder.Property(x => x.AudienceType)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.AudienceReferenceId);

        // Relationships
        builder.HasOne(x => x.Content)
            .WithMany(c => c.Audiences)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ContentId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
