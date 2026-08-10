using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class AccountPermissionConfig : IEntityTypeConfiguration<AccountPermission>
{
    public void Configure(EntityTypeBuilder<AccountPermission> builder)
    {
        // Table
        builder.ToTable("account_permissions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.PermissionKey)
            .HasConversion(x => x.Value, x => PermissionKey.Create(x))
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.SourceRoleId)
            .IsRequired();

        builder.Property(x => x.CachedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Account)
            .WithMany(a => a.Permissions)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.AccountId, x.PermissionKey })
            .IsUnique();

        // Audit & Concurrency
        // Immutable/replace-wholesale cache, not admin-edited business data - CreatedAt/UpdatedAt
        // only, no optimistic-concurrency token needed.
        builder.ConfigureAuditFields();
    }
}
