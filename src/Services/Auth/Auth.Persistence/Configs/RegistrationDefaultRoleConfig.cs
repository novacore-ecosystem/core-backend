using NovaCore.Auth.Domain.Entities.Registrations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class RegistrationDefaultRoleConfig : IEntityTypeConfiguration<RegistrationDefaultRole>
{
    public void Configure(EntityTypeBuilder<RegistrationDefaultRole> builder)
    {
        // Table
        builder.ToTable("registration_default_roles");

        // Properties
        builder.HasKey(x => x.Id);

        // Relationships
        // Restrict on both sides - neither an App nor a Role should be deletable while still
        // referenced as a registration default (no owning aggregate here to cascade from,
        // mirroring PositionRoleConfig's Role FK / PermissionGrantConfig's PermissionDefinition FK).
        builder.HasOne(x => x.App)
            .WithMany()
            .HasForeignKey(x => x.AppId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        // The logical identity of a default: same Role, same App, same Tenant, only once.
        builder.HasIndex(x => new { x.TenantId, x.AppId, x.RoleId })
            .IsUnique();

        // Primary lookup pattern - "every default Role for this (Tenant, App) pair".
        builder.HasIndex(x => new { x.TenantId, x.AppId });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
