using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationTaskConfig : IEntityTypeConfiguration<ConversationTask>
{
    public void Configure(EntityTypeBuilder<ConversationTask> builder)
    {
        // Table
        builder.ToTable("conversation_tasks");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Priority).HasConversion<byte>().IsRequired();
        builder.Property(x => x.AssigneeUserId);
        builder.Property(x => x.CreatedByUserId).IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<ChatMetadata>(x))
            .HasColumnType("jsonb");

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.AssigneeUserId);
        builder.HasIndex(x => x.Status);
    }
}
