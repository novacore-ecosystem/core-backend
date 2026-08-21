using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class MessageReferenceConfig : IEntityTypeConfiguration<MessageReference>
{
    public void Configure(EntityTypeBuilder<MessageReference> builder)
    {
        // Table
        builder.ToTable("message_references");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferencedMessageId).IsRequired();
        builder.Property(x => x.Type).HasConversion<byte>().IsRequired();

        builder.Property(x => x.Metadata)
            .HasConversion(
                x => x == null ? null : x.ToJson(),
                x => x == null ? null : MetadataBase.FromJson<ChatMetadata>(x))
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne(x => x.Message)
            .WithMany(x => x.References)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // ReferencedMessageId is a plain id, resolved by the Application layer - no navigation
        // (spec section 24). Restrict avoids a cascade cycle through the same Messages table.
        builder.HasOne<Message>()
            .WithMany()
            .HasForeignKey(x => x.ReferencedMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.MessageId);
        builder.HasIndex(x => x.ReferencedMessageId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
