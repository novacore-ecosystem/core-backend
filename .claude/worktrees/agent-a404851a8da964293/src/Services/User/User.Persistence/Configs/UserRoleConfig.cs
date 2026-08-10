using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserRoleConfig : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // Table
        builder.ToTable("user_roles");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .HasConversion(x => x.Value, x => RoleKey.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(RoleStatus.Active);

        builder.Property(x => x.Permissions)
            .HasConversion(
                x => x.Values.ToArray(),
                x => PermissionCollection.Create(x))
            .HasColumnType("text[]")
            .IsRequired();

        // Relationships
        builder.HasMany(x => x.Translations)
            .WithOne(t => t.Role)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Key)
            .IsUnique();
        builder.HasIndex(x => x.Status);
        // Permission-containment checks (`Permissions.Contains(x)`) translate to Postgres `@>`/
        // `= ANY`, which a default btree index can't serve.
        builder.HasIndex(x => x.Permissions)
            .HasMethod("gin");

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
