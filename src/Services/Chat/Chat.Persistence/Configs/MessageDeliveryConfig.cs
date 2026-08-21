using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class MessageDeliveryConfig : IEntityTypeConfiguration<MessageDelivery>
{
    public void Configure(EntityTypeBuilder<MessageDelivery> builder)
    {
        // Table
        builder.ToTable("message_deliveries");

        // Properties
        builder.HasKey(x => new { x.MessageId, x.UserId });

        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();

        // Relationships - independently constructible, no navigation from Message (see MessageDelivery.cs).
        builder.HasOne<Message>()
            .WithMany()
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
