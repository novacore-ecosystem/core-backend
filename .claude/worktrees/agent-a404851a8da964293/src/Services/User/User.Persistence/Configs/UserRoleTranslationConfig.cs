using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserRoleTranslationConfig : IEntityTypeConfiguration<UserRoleTranslation>
{
    public void Configure(EntityTypeBuilder<UserRoleTranslation> builder)
    {
        // Table
        builder.ToTable("user_role_translations");

        // Properties
        // Id doubles as the owning UserRole's Id (see UserRoleTranslation.Create) - one row per
        // language, so the primary key must include LanguageCode.
        builder.HasKey(x => new { x.Id, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(x => x.Role)
            .WithMany(r => r.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
