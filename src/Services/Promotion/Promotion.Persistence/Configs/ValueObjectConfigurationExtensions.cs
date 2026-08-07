using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.Promotion.Domain.ValueObjects;

namespace NovaCore.Promotion.Persistence.Configs;

/// <summary>Shared OwnsOne mapping for the multi-column Period Value Object - avoids repeating the same StartTime/EndTime/TimeZone column trio across every Period-bearing entity's config. Single-column Value Objects (EntityCode, Currency, Quantity, Money, PromotionPriorityValue) are mapped inline via HasConversion in each entity's own config instead, matching this project's OrderNumber/PhoneNumber precedent.</summary>
public static class ValueObjectConfigurationExtensions
{
    public static void OwnsPeriod<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, Period?>> propertyExpression,
        string columnPrefix)
        where TEntity : class
    {
        builder.OwnsOne(propertyExpression, period =>
        {
            period.Property(p => p.StartTime)
                .HasColumnName($"{columnPrefix}_start_time")
                .IsRequired();

            period.Property(p => p.EndTime)
                .HasColumnName($"{columnPrefix}_end_time")
                .IsRequired();

            period.Property(p => p.TimeZone)
                .HasColumnName($"{columnPrefix}_time_zone")
                .HasMaxLength(50);
        });
    }
}
