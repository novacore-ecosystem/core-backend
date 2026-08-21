using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationParticipantConfig : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        // Table
        builder.ToTable("conversation_participants");

        // Properties
        builder.HasKey(x => new { x.ConversationId, x.UserId });

        builder.Property(x => x.Role).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.JoinedAt).IsRequired();
        builder.Property(x => x.IsMuted).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.LastReadSequence).IsRequired().HasDefaultValue(0L);

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<ChatMetadata>(x))
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
