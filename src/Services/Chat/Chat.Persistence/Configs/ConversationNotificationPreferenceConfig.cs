using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationNotificationPreferenceConfig : IEntityTypeConfiguration<ConversationNotificationPreference>
{
    public void Configure(EntityTypeBuilder<ConversationNotificationPreference> builder)
    {
        // Table
        builder.ToTable("conversation_notification_preferences");

        // Properties
        builder.HasKey(x => new { x.ConversationId, x.UserId });

        builder.Property(x => x.Mode).HasConversion<byte>().IsRequired();
        builder.Property(x => x.MutedUntil);

        // Relationships
        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.UserId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
