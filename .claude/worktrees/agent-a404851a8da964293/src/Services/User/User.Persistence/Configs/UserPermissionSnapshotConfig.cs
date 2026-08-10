using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserPermissionSnapshotConfig : IEntityTypeConfiguration<UserPermissionSnapshot>
{
    public void Configure(EntityTypeBuilder<UserPermissionSnapshot> builder)
    {
        // Table
        builder.ToTable("user_permission_snapshots");

        // Properties
        // Shared primary key (1:1 with User) - no surrogate Id, exactly one snapshot row per user.
        builder.HasKey(x => x.UserId);

        // Same array + GIN-index approach as UserRole.Permissions - both wrap the same
        // PermissionCollection Value Object.
        builder.Property(x => x.Permissions)
            .HasConversion(
                x => x.Values.ToArray(),
                x => PermissionCollection.Create(x))
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired()
            .HasDefaultValue(0);

        // Indexes
        // Permission-containment checks (`Permissions.Contains(x)`) translate to Postgres `@>`/
        // `= ANY`, which a default btree index can't serve.
        builder.HasIndex(x => x.Permissions)
            .HasMethod("gin");

        // Audit & Concurrency
        // Denormalized cache, not user-authored business data - CreatedAt/UpdatedAt only.
        builder.ConfigureAuditFields();
    }
}
