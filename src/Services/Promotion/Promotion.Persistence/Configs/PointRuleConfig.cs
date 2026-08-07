using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PointRuleConfig : IEntityTypeConfiguration<PointRule>
{
    public void Configure(EntityTypeBuilder<PointRule> builder)
    {
        // Table
        builder.ToTable("point_rules");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RuleType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Priority).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.PointRules)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProgramId);
    }
}
