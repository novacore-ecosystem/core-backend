using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class GiftReservationConfig : IEntityTypeConfiguration<GiftReservation>
{
    public void Configure(EntityTypeBuilder<GiftReservation> builder)
    {
        // Table
        builder.ToTable("gift_reservations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReservedQuantity)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .HasColumnName("reserved_quantity")
            .IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.GiftItem)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.GiftItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Claims is configured from GiftClaimConfig's side (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.GiftItemId);
        builder.HasIndex(x => x.UserId);
    }
}
