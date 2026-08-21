using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class StickerTranslationConfig : IEntityTypeConfiguration<StickerTranslation>
{
    public void Configure(EntityTypeBuilder<StickerTranslation> builder)
    {
        // Table
        builder.ToTable("sticker_translations");

        // Properties
        // Id doubles as the owning Sticker's Id (see StickerTranslation.Create) - one row per
        // language, so the primary key must include LanguageCode.
        builder.HasKey(x => new { x.Id, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

        // Relationships
        builder.HasOne(x => x.Sticker)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
