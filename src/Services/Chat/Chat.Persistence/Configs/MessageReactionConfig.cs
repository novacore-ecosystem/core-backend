using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class MessageReactionConfig : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        // Table
        builder.ToTable("message_reactions");

        // Properties
        builder.HasKey(x => new { x.MessageId, x.UserId });

        builder.Property(x => x.ReactionType).HasConversion<byte>().IsRequired();

        // Relationships
        builder.HasOne(x => x.Message)
            .WithMany(x => x.Reactions)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.UserId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
