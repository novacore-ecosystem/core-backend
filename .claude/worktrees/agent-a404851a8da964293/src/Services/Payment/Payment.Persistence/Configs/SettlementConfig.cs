using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class SettlementConfig : IEntityTypeConfiguration<Settlement>
{
    public void Configure(EntityTypeBuilder<Settlement> builder)
    {
        // Table
        builder.ToTable("settlements");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GatewayId).IsRequired();
        builder.Property(x => x.PeriodStart).IsRequired();
        builder.Property(x => x.PeriodEnd).IsRequired();
        builder.Property(x => x.Status).HasConversion<short>().IsRequired();

        builder.OwnsMoney(x => x.GrossAmount, "gross_amount");
        builder.OwnsMoney(x => x.FeeAmount, "fee_amount");
        builder.OwnsMoney(x => x.NetAmount, "net_amount");

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.GatewayId);
        builder.HasIndex(x => new { x.PeriodStart, x.PeriodEnd });
    }
}
