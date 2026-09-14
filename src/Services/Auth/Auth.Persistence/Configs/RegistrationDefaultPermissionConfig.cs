using NovaCore.Auth.Domain.Entities.Registrations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class RegistrationDefaultPermissionConfig : IEntityTypeConfiguration<RegistrationDefaultPermission>
{
    public void Configure(EntityTypeBuilder<RegistrationDefaultPermission> builder)
    {
        // Table
        builder.ToTable("registration_default_permissions");

        // Properties
        builder.HasKey(x => x.Id);

        // Relationships
        // Restrict on both sides - neither an App nor a PermissionDefinition should be deletable
        // while still referenced as a registration default (no owning aggregate here to cascade
        // from, mirroring PermissionGrantConfig's PermissionDefinition FK).
        builder.HasOne(x => x.App)
            .WithMany()
            .HasForeignKey(x => x.AppId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PermissionDefinition)
            .WithMany()
            .HasForeignKey(x => x.PermissionDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        // The logical identity of a default: same permission, same App, same Tenant, only once.
        builder.HasIndex(x => new { x.TenantId, x.AppId, x.PermissionDefinitionId })
            .IsUnique();

        // Primary lookup pattern - "every default permission for this (Tenant, App) pair".
        builder.HasIndex(x => new { x.TenantId, x.AppId });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
