using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class VoucherHistoryConfig : IEntityTypeConfiguration<VoucherHistory>
{
    public void Configure(EntityTypeBuilder<VoucherHistory> builder)
    {
        // Table
        builder.ToTable("voucher_histories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Voucher)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.VoucherId);
    }
}
