using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationReasonSuggestionTranslationConfig : IEntityTypeConfiguration<ConversationReasonSuggestionTranslation>
{
    public void Configure(EntityTypeBuilder<ConversationReasonSuggestionTranslation> builder)
    {
        // Table
        builder.ToTable("conversation_reason_suggestion_translations");

        // Properties
        // Id doubles as the owning suggestion's Id (see ConversationReasonSuggestionTranslation.Create)
        // - one row per language, so the primary key must include LanguageCode.
        builder.HasKey(x => new { x.Id, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Text).HasMaxLength(200).IsRequired();

        // Relationships
        builder.HasOne(x => x.ConversationReasonSuggestion)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
