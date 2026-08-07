using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class VoucherFreezeConfig : IEntityTypeConfiguration<VoucherFreeze>
{
    public void Configure(EntityTypeBuilder<VoucherFreeze> builder)
    {
        // Table
        builder.ToTable("voucher_freezes");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason).HasMaxLength(500);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Voucher)
            .WithMany()
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.VoucherId);
    }
}
