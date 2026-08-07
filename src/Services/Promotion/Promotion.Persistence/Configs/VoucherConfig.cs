using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class VoucherConfig : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        // Table
        builder.ToTable("vouchers");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => EntityCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.VoucherType).HasConversion<short>().IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.Property(x => x.Currency)
            .HasConversion(x => x.Value, x => Currency.Create(x))
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.Balance)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("balance")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();
        builder.Property(x => x.TimeZone).HasMaxLength(50).IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Wallet)
            .WithMany(x => x.Vouchers)
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        // Issues/Reservations/Redemptions/Transfers/History are all configured from the child
        // entity's own config (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.PromotionId);
        builder.HasIndex(x => x.CampaignId);
        builder.HasIndex(x => x.WalletId);
    }
}
