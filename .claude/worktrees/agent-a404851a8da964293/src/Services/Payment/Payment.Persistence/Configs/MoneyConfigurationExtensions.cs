using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

/// <summary>Shared OwnsOne mapping for the currency-aware Money Value Object - avoids repeating the same Amount/Currency column pair across every Money-bearing entity's config.</summary>
public static class MoneyConfigurationExtensions
{
    public static void OwnsMoney<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, Money>> propertyExpression,
        string columnPrefix)
        where TEntity : class
    {
        builder.OwnsOne(propertyExpression, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName($"{columnPrefix}_amount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasConversion(c => c.Value, c => Currency.Create(c))
                .HasColumnName($"{columnPrefix}_currency")
                .HasMaxLength(3)
                .IsRequired();
        });
    }
}
