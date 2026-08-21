using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class MessageConfig : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        // Table
        builder.ToTable("messages");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.SenderUserId);
        builder.Property(x => x.SenderType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.ClientMessageId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.Type).HasConversion<short>().IsRequired();
        builder.Property(x => x.Content).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.Format).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<ChatMetadata>(x))
            .HasColumnType("jsonb");

        builder.ConfigureCommonFields();

        // Relationships
        // Related to Conversation by ConversationId only (no navigation) - unbounded per
        // conversation, see Message.cs.
        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Attachments/References/Reactions/Mentions are configured from their own config.

        // Indexes
        builder.HasIndex(x => new { x.ConversationId, x.Sequence }).IsUnique();
        builder.HasIndex(x => new { x.ConversationId, x.ClientMessageId }).IsUnique();
        builder.HasIndex(x => x.SenderUserId);
        builder.HasIndex(x => x.Status);
    }
}
