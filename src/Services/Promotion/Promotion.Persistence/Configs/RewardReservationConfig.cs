using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class RewardReservationConfig : IEntityTypeConfiguration<RewardReservation>
{
    public void Configure(EntityTypeBuilder<RewardReservation> builder)
    {
        // Table
        builder.ToTable("reward_reservations");

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
