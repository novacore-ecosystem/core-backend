using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PointTransactionConfig : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> builder)
    {
        // Table
        builder.ToTable("point_transactions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasConversion<short>().IsRequired();
        builder.Property(x => x.Source).HasMaxLength(100);
        builder.Property(x => x.Points).IsRequired();
        builder.Property(x => x.BalanceAfter).IsRequired();

        builder.ConfigureAuditFields();

        // Relationships
        builder.HasOne(x => x.Account)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ledgers is configured from PointLedgerConfig's side (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.ReferenceId);
        builder.HasIndex(x => x.Type);
    }
}
