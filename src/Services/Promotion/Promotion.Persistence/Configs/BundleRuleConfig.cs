using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class BundleRuleConfig : IEntityTypeConfiguration<BundleRule>
{
    public void Configure(EntityTypeBuilder<BundleRule> builder)
    {
        // Table
        builder.ToTable("bundle_rules");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RuleType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Configuration).HasColumnType("jsonb");

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Bundle)
            .WithMany(x => x.Rules)
            .HasForeignKey(x => x.BundleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.BundleId);
    }
}
