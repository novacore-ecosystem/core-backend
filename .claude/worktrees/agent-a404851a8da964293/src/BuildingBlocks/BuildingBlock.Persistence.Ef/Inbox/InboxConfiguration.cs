using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.BuildingBlock.Persistence.Ef.Inbox;

public sealed class InboxConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.ConsumerName).IsRequired();
        builder.Property(x => x.Topic).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.HeadersJson).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>();
        builder.Property(x => x.RetryCount).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ProcessedAt).IsRequired(false);
        builder.Property(x => x.NextRetryAt).IsRequired(false);
        builder.Property(x => x.LastRetryAt).IsRequired(false);
        builder.Property(x => x.LastError).IsRequired(false);

        builder.HasIndex(x => new { x.MessageId, x.ConsumerName })
            .IsUnique()
            .HasDatabaseName("idx_inbox_message_consumer_unique");

        builder.HasIndex(x => x.ProcessedAt)
            .HasDatabaseName("idx_inbox_processed_at");

        // Covers the InboxRetryHostedService poll: WHERE Status = Retrying AND NextRetryAt <= now.
        builder.HasIndex(x => new { x.Status, x.NextRetryAt })
            .HasDatabaseName("idx_inbox_status_next_retry_at");

        // Covers the Dead Letter Queue admin list/search default view: WHERE Status = DeadLetter
        // ORDER BY CreatedAt (see DeadLetterCriteriaDefinition/EfDeadLetterQueryService).
        builder.HasIndex(x => new { x.Status, x.CreatedAt })
            .HasDatabaseName("idx_inbox_status_created_at");
    }
}
