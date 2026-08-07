using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class VoucherReservationConfig : IEntityTypeConfiguration<VoucherReservation>
{
    public void Configure(EntityTypeBuilder<VoucherReservation> builder)
    {
        // Table
        builder.ToTable("voucher_reservations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReservedAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("reserved_amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Voucher)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.VoucherId);
        builder.HasIndex(x => x.OrderId);
    }
}
