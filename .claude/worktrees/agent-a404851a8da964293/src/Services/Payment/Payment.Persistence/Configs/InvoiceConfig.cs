using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class InvoiceConfig : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        // Table
        builder.ToTable("invoices");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceType).HasConversion<short>().IsRequired();
        builder.Property(x => x.ReferenceId).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.OwnsMoney(x => x.Amount, "amount");

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne<BillingProfile>()
            .WithMany()
            .HasForeignKey(x => x.BillingProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
        builder.HasIndex(x => x.Status);
    }
}
