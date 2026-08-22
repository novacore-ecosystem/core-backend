using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Content.Persistence.Configs;

public sealed class ContentContributorConfig : IEntityTypeConfiguration<ContentContributor>
{
    public void Configure(EntityTypeBuilder<ContentContributor> builder)
    {
        // Table
        builder.ToTable("content_contributors");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContentId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<byte>()
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Content)
            .WithMany(c => c.Contributors)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.ContentId, x.UserId });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
