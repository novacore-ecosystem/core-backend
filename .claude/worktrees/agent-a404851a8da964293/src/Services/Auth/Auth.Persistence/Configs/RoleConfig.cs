using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Table
        builder.ToTable("roles");

        // Properties
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.NormalizedName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.Code)
            .HasConversion(x => x.Value, x => RoleCode.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.IsSystemRole)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasMany(r => r.UserRoles)
            .WithOne(ar => ar.Role)
            .HasForeignKey(ar => ar.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Permissions)
            .WithOne(p => p.Role)
            .HasForeignKey(p => p.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Translations)
            .WithOne(t => t.Role)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.Code)
            .IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
