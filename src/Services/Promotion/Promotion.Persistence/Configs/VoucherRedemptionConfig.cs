using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class VoucherRedemptionConfig : IEntityTypeConfiguration<VoucherRedemption>
{
    public void Configure(EntityTypeBuilder<VoucherRedemption> builder)
    {
        // Table
        builder.ToTable("voucher_redemptions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RedeemedAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("redeemed_amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Voucher)
            .WithMany(x => x.Redemptions)
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.VoucherId);
        builder.HasIndex(x => x.OrderId);
    }
}
