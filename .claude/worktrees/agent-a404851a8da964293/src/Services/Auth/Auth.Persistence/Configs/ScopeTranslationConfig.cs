using NovaCore.Auth.Domain.Entities.Scopes;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class ScopeTranslationConfig : IEntityTypeConfiguration<ScopeTranslation>
{
    public void Configure(EntityTypeBuilder<ScopeTranslation> builder)
    {
        // Table
        builder.ToTable("scope_translations");

        // Properties
        // Id doubles as the owning Scope's Id (see ScopeTranslation.Create) - one row per
        // language, so the primary key must include LanguageCode.
        builder.HasKey(x => new { x.Id, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(x => x.Scope)
            .WithMany(s => s.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
