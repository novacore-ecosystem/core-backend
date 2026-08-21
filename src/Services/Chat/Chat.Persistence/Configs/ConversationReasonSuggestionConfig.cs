using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationReasonSuggestionConfig : IEntityTypeConfiguration<ConversationReasonSuggestion>
{
    public void Configure(EntityTypeBuilder<ConversationReasonSuggestion> builder)
    {
        // Table
        builder.ToTable("conversation_reason_suggestions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasConversion(x => x.Value, x => EntityCode.Create(x))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.SortOrder).IsRequired().HasDefaultValue(0);

        builder.ConfigureCommonFields();

        // Indexes
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}
