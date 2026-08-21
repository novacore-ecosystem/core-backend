using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationPermissionTranslationConfig : IEntityTypeConfiguration<ConversationPermissionTranslation>
{
    public void Configure(EntityTypeBuilder<ConversationPermissionTranslation> builder)
    {
        // Table
        builder.ToTable("conversation_permission_translations");

        // Properties
        // Id doubles as the owning ConversationPermission's Id (see
        // ConversationPermissionTranslation.Create) - one row per language, so the primary key
        // must include LanguageCode.
        builder.HasKey(x => new { x.Id, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        // Relationships
        builder.HasOne(x => x.Permission)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
