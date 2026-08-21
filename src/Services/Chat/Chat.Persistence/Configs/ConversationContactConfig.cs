using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationContactConfig : IEntityTypeConfiguration<ConversationContact>
{
    public void Configure(EntityTypeBuilder<ConversationContact> builder)
    {
        // Table
        builder.ToTable("conversation_contacts");

        // Properties
        builder.HasKey(x => new { x.ConversationId, x.ContactId });

        // Relationships
        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.ContactMappings)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Contact)
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ContactId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
