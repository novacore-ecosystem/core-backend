using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserTagConfig : IEntityTypeConfiguration<UserTag>
{
    public void Configure(EntityTypeBuilder<UserTag> builder)
    {
        // Table
        builder.ToTable("user_tags");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasConversion(x => x.Value, x => TagCode.Create(x))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Color)
            .HasMaxLength(20);

        builder.Property(x => x.Scope)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.IsSystem)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasMany(x => x.Translations)
            .WithOne(t => t.UserTag)
            .HasForeignKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Name)
            .IsUnique();
        builder.HasIndex(x => x.Scope);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
