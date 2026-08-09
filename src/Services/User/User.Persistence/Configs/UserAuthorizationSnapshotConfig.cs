using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserAuthorizationSnapshotConfig : IEntityTypeConfiguration<UserAuthorizationSnapshot>
{
    public void Configure(EntityTypeBuilder<UserAuthorizationSnapshot> builder)
    {
        // Table
        builder.ToTable("user_authorization_snapshots");

        // Properties
        // Shared primary key (1:1 with User) - no surrogate Id, exactly one snapshot row per user.
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.Permissions)
            .HasConversion(
                x => x.ToArray(),
                x => x.ToList())
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired()
            .HasDefaultValue(0);

        // Indexes
        // Permission-containment checks (`Permissions.Contains(x)`) translate to Postgres `@>`/
        // `= ANY`, which a default btree index can't serve - same approach as
        // UserPermissionSnapshot/UserRole's own text[] Permissions columns.
        builder.HasIndex(x => x.Permissions)
            .HasMethod("gin");

        // Audit & Concurrency
        // Denormalized cache, not user-authored business data - CreatedAt/UpdatedAt only.
        builder.ConfigureAuditFields();
    }
}
