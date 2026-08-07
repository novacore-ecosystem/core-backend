using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class GiftClaimConfig : IEntityTypeConfiguration<GiftClaim>
{
    public void Configure(EntityTypeBuilder<GiftClaim> builder)
    {
        // Table
        builder.ToTable("gift_claims");

        // Properties
        builder.HasKey(x => x.Id);

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Reservation)
            .WithMany(x => x.Claims)
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ReservationId);
    }
}
