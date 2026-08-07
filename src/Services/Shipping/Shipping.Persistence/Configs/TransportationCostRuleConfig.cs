using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Shipping.Persistence.Configs;

public sealed class TransportationCostRuleConfig : IEntityTypeConfiguration<TransportationCostRule>
{
    public void Configure(EntityTypeBuilder<TransportationCostRule> builder)
    {
        // Table
        builder.ToTable("transportation_cost_rules");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RuleType).HasConversion<short>().IsRequired();
        builder.Property(x => x.EffectiveFrom).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.BaseAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)")
            .IsRequired();
        builder.Property(x => x.UnitAmount)
            .HasConversion(x => x.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)")
            .IsRequired();
        builder.Property(x => x.MinAmount)
            .HasConversion(x => x!.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.MaxAmount)
            .HasConversion(x => x!.Value, x => Money.Create(x))
            .HasColumnType("numeric(18,2)");

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ProviderId);
        builder.HasIndex(x => new { x.IsActive, x.EffectiveFrom });
    }
}
