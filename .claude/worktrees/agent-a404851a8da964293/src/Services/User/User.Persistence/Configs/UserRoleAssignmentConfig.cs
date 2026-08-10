using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserRoleAssignmentConfig : IEntityTypeConfiguration<UserRoleAssignment>
{
    public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
    {
        // Table
        builder.ToTable("user_role_assignments");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.RoleId)
            .IsRequired();

        builder.Property(x => x.AssignedAt)
            .IsRequired();

        builder.Property(x => x.AssignedBy);
        builder.Property(x => x.ExpiredAt);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(UserRoleAssignmentStatus.Active);

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.RoleAssignments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade - a UserRole with grant history should not be deletable while
        // that history still references it.
        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.UserId);
        // Serves User.AssignRole's "already effective" check and permission-snapshot rebuilds.
        builder.HasIndex(x => new { x.UserId, x.RoleId, x.Status });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
