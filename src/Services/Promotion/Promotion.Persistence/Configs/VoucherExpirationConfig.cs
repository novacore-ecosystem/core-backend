using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class VoucherExpirationConfig : IEntityTypeConfiguration<VoucherExpiration>
{
    public void Configure(EntityTypeBuilder<VoucherExpiration> builder)
    {
        // Table
        builder.ToTable("voucher_expirations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExpiredAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("expired_amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Voucher)
            .WithMany()
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.VoucherId);
    }
}
