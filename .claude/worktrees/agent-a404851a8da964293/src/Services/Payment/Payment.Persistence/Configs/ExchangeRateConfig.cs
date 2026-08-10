using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class ExchangeRateConfig : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        // Table
        builder.ToTable("exchange_rates");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FromCurrency)
            .HasConversion(x => x.Value, x => Currency.Create(x))
            .HasColumnName("from_currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.ToCurrency)
            .HasConversion(x => x.Value, x => Currency.Create(x))
            .HasColumnName("to_currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Rate).HasColumnType("numeric(18,8)").IsRequired();
        builder.Property(x => x.EffectiveAt).IsRequired();

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => new { x.FromCurrency, x.ToCurrency, x.EffectiveAt }).IsUnique();
    }
}
