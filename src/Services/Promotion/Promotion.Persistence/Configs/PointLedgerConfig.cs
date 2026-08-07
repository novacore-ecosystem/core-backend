using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PointLedgerConfig : IEntityTypeConfiguration<PointLedger>
{
    public void Configure(EntityTypeBuilder<PointLedger> builder)
    {
        // Table
        builder.ToTable("point_ledgers");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Debit).IsRequired();
        builder.Property(x => x.Credit).IsRequired();
        builder.Property(x => x.Balance).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Transaction)
            .WithMany(x => x.Ledgers)
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.TransactionId);
    }
}
