using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class PermissionDefinitionConfig : IEntityTypeConfiguration<PermissionDefinition>
{
    public void Configure(EntityTypeBuilder<PermissionDefinition> builder)
    {
        // Table
        builder.ToTable("permission_definitions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .HasConversion(x => x.Value, x => PermissionKey.Create(x))
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.PermissionGroupId)
            .IsRequired();

        builder.Property(x => x.IsSystemPermission)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(x => x.PermissionGroup)
            .WithMany(g => g.Definitions)
            .HasForeignKey(x => x.PermissionGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.RolePermissions)
            .WithOne(rp => rp.PermissionDefinition)
            .HasForeignKey(rp => rp.PermissionDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.PermissionDefinition)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Key)
            .IsUnique();
        builder.HasIndex(x => x.PermissionGroupId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
