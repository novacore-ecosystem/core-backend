using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationTagConfig : IEntityTypeConfiguration<ConversationTag>
{
    public void Configure(EntityTypeBuilder<ConversationTag> builder)
    {
        // Table
        builder.ToTable("conversation_tags");

        // Properties
        builder.HasKey(x => new { x.ConversationId, x.TagId });

        // Relationships
        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.TagMappings)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.TagId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
