using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationReadStateConfig : IEntityTypeConfiguration<ConversationReadState>
{
    public void Configure(EntityTypeBuilder<ConversationReadState> builder)
    {
        // Table
        builder.ToTable("conversation_read_states");

        // Properties
        builder.HasKey(x => new { x.ConversationId, x.UserId });

        builder.Property(x => x.LastReadSequence).IsRequired().HasDefaultValue(0L);

        // Relationships - independently constructible, not owned by Conversation (see ConversationReadState.cs).
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
