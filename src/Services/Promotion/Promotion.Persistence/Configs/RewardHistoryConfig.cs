using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RewardHistoryConfig : IEntityTypeConfiguration<RewardHistory>
{
    public void Configure(EntityTypeBuilder<RewardHistory> builder)
    {
        // Table
        builder.ToTable("reward_histories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        // RewardId is a loose reference - no dedicated Reward entity exists in this pass.

        // Indexes
        builder.HasIndex(x => x.RewardId);
    }
}
