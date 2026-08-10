using NovaCore.Auth.Domain.Entities.Permissions;

using NovaCore.BuildingBlock.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class PermissionGroupTranslationConfig : IEntityTypeConfiguration<PermissionGroupTranslation>
{
    public void Configure(EntityTypeBuilder<PermissionGroupTranslation> builder)
    {
        // Table
        builder.ToTable("permission_group_translations");

        // Properties
        // Id doubles as the owning PermissionGroup's Id (see PermissionGroupTranslation.Create) -
        // one row per language, so the primary key must include LanguageCode.
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
        builder.HasOne(x => x.PermissionGroup)
            .WithMany(g => g.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
