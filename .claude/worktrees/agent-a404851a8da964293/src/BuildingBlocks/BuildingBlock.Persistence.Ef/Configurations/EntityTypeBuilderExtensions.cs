using NovaCore.BuildingBlock.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.BuildingBlock.Persistence.Ef.Configurations;

/// <summary>
/// Reusable EF Core configuration extensions for common entity fields.
/// These methods apply database persistence metadata (audit timestamps, concurrency tokens)
/// that are useful for nearly every table, regardless of business audit logging concerns.
/// </summary>
public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// Configures audit metadata fields: CreatedAt and UpdatedAt.
    /// These fields track when records are created and modified at the database level,
    /// separate from business audit logging via the Audit Service.
    /// </summary>
    public static EntityTypeBuilder<T> ConfigureAuditFields<T>(this EntityTypeBuilder<T> builder)
        where T : class, IEntity
    {
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        return builder;
    }

    /// <summary>
    /// Configures optimistic concurrency control using PostgreSQL's xmin row version system.
    /// Prevents lost writes when multiple processes attempt concurrent modifications.
    /// </summary>
    public static EntityTypeBuilder<T> ConfigureConcurrency<T>(this EntityTypeBuilder<T> builder)
        where T : class
    {
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();

        return builder;
    }

    /// <summary>
    /// Convenience method that configures both audit fields and concurrency in one call.
    /// Use when an entity requires both database audit metadata and optimistic concurrency.
    /// For immutable entities (like append-only logs), call ConfigureAuditFields() only.
    /// </summary>
    public static EntityTypeBuilder<T> ConfigureCommonFields<T>(this EntityTypeBuilder<T> builder)
        where T : class, IEntity
    {
        return builder
            .ConfigureAuditFields()
            .ConfigureConcurrency();
    }
}
