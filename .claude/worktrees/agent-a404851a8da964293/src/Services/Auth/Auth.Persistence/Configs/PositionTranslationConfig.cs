using NovaCore.Auth.Domain.Entities.Positions;

using NovaCore.BuildingBlock.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class PositionTranslationConfig : IEntityTypeConfiguration<PositionTranslation>
{
    public void Configure(EntityTypeBuilder<PositionTranslation> builder)
    {
        // Table
        builder.ToTable("position_translations");

        // Properties
        // Id doubles as the owning Position's Id (see PositionTranslation.Create) - one row per
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
        builder.HasOne(x => x.Position)
            .WithMany(p => p.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
