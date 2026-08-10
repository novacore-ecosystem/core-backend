using NovaCore.Auth.Domain.Entities.Positions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class PositionRoleConfig : IEntityTypeConfiguration<PositionRole>
{
    public void Configure(EntityTypeBuilder<PositionRole> builder)
    {
        // Table
        builder.ToTable("position_roles");

        // Properties
        // Pure mapping entity - the pairing itself is the identity, no surrogate Id.
        builder.HasKey(x => new { x.PositionId, x.RoleId });

        // Relationships
        builder.HasOne(x => x.Position)
            .WithMany(p => p.Roles)
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade - a Role still carried by a Position should not be deletable.
        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.RoleId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
