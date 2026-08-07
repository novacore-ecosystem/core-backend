using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RewardClaimConfig : IEntityTypeConfiguration<RewardClaim>
{
    public void Configure(EntityTypeBuilder<RewardClaim> builder)
    {
        // Table
        builder.ToTable("reward_claims");

        // Properties
        builder.HasKey(x => x.Id);

        builder.ConfigureAuditFields();

        // Relationships
        // RewardId is a loose reference - no dedicated Reward entity exists in this pass.

        // Indexes
        builder.HasIndex(x => x.RewardId);
        builder.HasIndex(x => x.UserId);
    }
}
