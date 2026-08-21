using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationRoleTranslationConfig : IEntityTypeConfiguration<ConversationRoleTranslation>
{
    public void Configure(EntityTypeBuilder<ConversationRoleTranslation> builder)
    {
        // Table
        builder.ToTable("conversation_role_translations");

        // Properties
        // Id doubles as the owning ConversationRole's Id (see ConversationRoleTranslation.Create)
        // - one row per language, so the primary key must include LanguageCode.
        builder.HasKey(x => new { x.Id, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        // Relationships
        builder.HasOne(x => x.Role)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
