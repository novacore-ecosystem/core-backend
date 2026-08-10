using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class ReconciliationConfig : IEntityTypeConfiguration<Reconciliation>
{
    public void Configure(EntityTypeBuilder<Reconciliation> builder)
    {
        // Table
        builder.ToTable("reconciliations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<short>().IsRequired();
        builder.Property(x => x.DiscrepancyAmount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne<Settlement>()
            .WithMany()
            .HasForeignKey(x => x.SettlementId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.SettlementId);
    }
}
