using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationNoteConfig : IEntityTypeConfiguration<ConversationNote>
{
    public void Configure(EntityTypeBuilder<ConversationNote> builder)
    {
        // Table
        builder.ToTable("conversation_notes");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(4000);
        builder.Property(x => x.Type).HasConversion<byte>().IsRequired();
        builder.Property(x => x.SortOrder).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.IsPinned).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedByUserId).IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<ChatMetadata>(x))
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.Notes)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ConversationId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
