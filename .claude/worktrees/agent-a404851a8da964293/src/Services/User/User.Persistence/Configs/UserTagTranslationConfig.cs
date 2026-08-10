using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserTagTranslationConfig : IEntityTypeConfiguration<UserTagTranslation>
{
    public void Configure(EntityTypeBuilder<UserTagTranslation> builder)
    {
        // Table
        builder.ToTable("user_tag_translations");

        // Properties
        // Id doubles as the owning UserTag's Id (see UserTagTranslation.Create) - one row per
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
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(x => x.UserTag)
            .WithMany(t => t.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
