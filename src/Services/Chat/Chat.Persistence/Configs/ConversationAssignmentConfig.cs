using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationAssignmentConfig : IEntityTypeConfiguration<ConversationAssignment>
{
    public void Configure(EntityTypeBuilder<ConversationAssignment> builder)
    {
        // Table
        builder.ToTable("conversation_assignments");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.AssigneeUserId).IsRequired();
        builder.Property(x => x.AssignedAt).IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<ChatMetadata>(x))
            .HasColumnType("jsonb");

        // Relationships - independently constructible by ConversationId (see ConversationAssignment.cs), no navigation.
        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.AssigneeUserId);
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
