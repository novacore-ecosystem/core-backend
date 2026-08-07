using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class VoucherWalletConfig : IEntityTypeConfiguration<VoucherWallet>
{
    public void Configure(EntityTypeBuilder<VoucherWallet> builder)
    {
        // Table
        builder.ToTable("voucher_wallets");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalBalance)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("total_balance")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.AvailableBalance)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("available_balance")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.ReservedBalance)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("reserved_balance")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.ConfigureCommonFields();

        // Relationships configured from VoucherConfig (Vouchers collection).

        // Indexes
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
