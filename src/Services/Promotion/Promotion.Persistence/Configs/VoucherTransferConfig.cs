using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class VoucherTransferConfig : IEntityTypeConfiguration<VoucherTransfer>
{
    public void Configure(EntityTypeBuilder<VoucherTransfer> builder)
    {
        // Table
        builder.ToTable("voucher_transfers");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnName("amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Voucher)
            .WithMany(x => x.Transfers)
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.VoucherId);
        builder.HasIndex(x => x.FromUserId);
        builder.HasIndex(x => x.ToUserId);
    }
}
