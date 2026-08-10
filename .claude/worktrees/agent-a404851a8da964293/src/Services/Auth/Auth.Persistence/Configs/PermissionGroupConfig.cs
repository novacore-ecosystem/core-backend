using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class PermissionGroupConfig : IEntityTypeConfiguration<PermissionGroup>
{
    public void Configure(EntityTypeBuilder<PermissionGroup> builder)
    {
        // Table
        builder.ToTable("permission_groups");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => PermissionGroupCode.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasMany(x => x.Definitions)
            .WithOne(d => d.PermissionGroup)
            .HasForeignKey(d => d.PermissionGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Translations)
            .WithOne(t => t.PermissionGroup)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique();
        builder.HasIndex(x => x.SortOrder);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
