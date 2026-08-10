using NovaCore.Auth.Domain.Entities.Roles;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class RolePermissionConfig : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        // Table
        builder.ToTable("role_permissions");

        // Properties
        // Pure mapping entity - the pairing itself is the identity, no surrogate Id.
        builder.HasKey(x => new { x.RoleId, x.PermissionDefinitionId });

        // Relationships
        builder.HasOne(x => x.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade - a PermissionDefinition still granted by a Role should not be
        // deletable.
        builder.HasOne(x => x.PermissionDefinition)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(x => x.PermissionDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.PermissionDefinitionId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
