using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PointPolicyConfig : IEntityTypeConfiguration<PointPolicy>
{
    public void Configure(EntityTypeBuilder<PointPolicy> builder)
    {
        // Table
        builder.ToTable("point_policies");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PolicyType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Configuration).HasColumnType("jsonb");

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.PointPolicies)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProgramId);
    }
}
