using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationTransferRequestConfig : IEntityTypeConfiguration<ConversationTransferRequest>
{
    public void Configure(EntityTypeBuilder<ConversationTransferRequest> builder)
    {
        // Table
        builder.ToTable("conversation_transfer_requests");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.FromUserId).IsRequired();
        builder.Property(x => x.ToUserId).IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.RequestedAt).IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<ChatMetadata>(x))
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.ToUserId);
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
