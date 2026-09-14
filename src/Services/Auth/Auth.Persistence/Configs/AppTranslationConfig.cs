using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.Auth.Domain.Entities.Apps;

using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class AppTranslationConfig : IEntityTypeConfiguration<AppTranslation>
{
    public void Configure(EntityTypeBuilder<AppTranslation> builder)
    {
        // Table
        builder.ToTable("app_translations");

        // Properties
        // Id doubles as the owning App's Id (see AppTranslation.Create) - one row per language,
        // so the primary key must include LanguageCode.
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
        builder.HasOne(x => x.App)
            .WithMany(a => a.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
