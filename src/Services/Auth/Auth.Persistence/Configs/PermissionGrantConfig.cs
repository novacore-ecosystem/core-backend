using NovaCore.Auth.Domain.Entities.Permissions;

using NovaCore.BuildingBlock.SharedKernel.Authorization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class PermissionGrantConfig : IEntityTypeConfiguration<PermissionGrant>
{
    public void Configure(EntityTypeBuilder<PermissionGrant> builder)
    {
        // Table
        builder.ToTable("permission_grants");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PermissionDefinitionId)
            .IsRequired();

        builder.Property(x => x.ProviderName)
            .HasConversion(x => x.ToName(), x => PermissionProviderNameExtensions.ParseName(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProviderKey)
            .HasMaxLength(100)
            .IsRequired();

        // Relationships
        // Restrict, not Cascade - a PermissionDefinition still granted should not be deletable.
        builder.HasOne(x => x.PermissionDefinition)
            .WithMany(p => p.Grants)
            .HasForeignKey(x => x.PermissionDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        // The logical identity of a grant: same permission, same provider/context, same tenant
        // can only be granted once - enforced at the DB level, not just application checks.
        builder.HasIndex(x => new { x.TenantId, x.PermissionDefinitionId, x.ProviderName, x.ProviderKey })
            .IsUnique();

        // Primary lookup pattern - "every grant for this provider instance" (e.g. a Role's
        // permission set).
        builder.HasIndex(x => new { x.TenantId, x.ProviderName, x.ProviderKey });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
