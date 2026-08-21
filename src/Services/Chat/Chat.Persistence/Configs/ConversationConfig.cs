using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationConfig : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        // Table
        builder.ToTable("conversations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Lifecycle).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Avatar).HasMaxLength(500);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.Priority).HasConversion<byte>().IsRequired();
        builder.Property(x => x.LastActivityAt).IsRequired();
        builder.Property(x => x.LastMessageSequence).IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<ChatMetadata>(x))
            .HasColumnType("jsonb");

        builder.ConfigureCommonFields();

        // Relationships
        // ContactMappings/Participants/TagMappings/Notes/PinnedMessages are all configured from
        // the child entity's own config (single source per relationship).

        // Indexes
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.LastActivityAt);
    }
}
