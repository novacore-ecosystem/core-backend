using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PromotionConfig : IEntityTypeConfiguration<PromotionEntity>
{
    public void Configure(EntityTypeBuilder<PromotionEntity> builder)
    {
        // Table
        builder.ToTable("promotions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => EntityCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.Type).HasConversion<short>().IsRequired();
        builder.Property(x => x.Priority).IsRequired();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();

        builder.Property(x => x.Currency)
            .HasConversion(x => x.Value, x => Currency.Create(x))
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.TimeZone).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.IsApproved).IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<PromotionMetadata>(x))
            .HasColumnType("jsonb");

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Promotions)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        // No reverse collection on ApprovalWorkflow (it's a general-purpose aggregate, not owned
        // by Promotion) - same shape as the Campaign relationship above.
        builder.HasOne(x => x.ApprovalWorkflow)
            .WithMany()
            .HasForeignKey(x => x.ApprovalWorkflowId)
            .OnDelete(DeleteBehavior.Restrict);

        // Versions/Rules/Targets/Benefits/Constraints/UsageLimits/ExecutionPolicy/StackingPolicy
        // are all configured from the child entity's own config (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.CampaignId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Status, x.StartTime });
        builder.HasIndex(x => new { x.Type, x.Status });
        builder.HasIndex(x => x.Priority);
        builder.HasIndex(x => x.ApprovalWorkflowId);
    }
}
