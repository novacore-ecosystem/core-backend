using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.BuildingBlock.Persistence.Ef.Inbox;

public sealed class InboxRetryHistoryConfiguration : IEntityTypeConfiguration<InboxRetryHistory>
{
    public void Configure(EntityTypeBuilder<InboxRetryHistory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InboxMessageId).IsRequired();
        builder.Property(x => x.MessageId).IsRequired();
        builder.Property(x => x.ConsumerName).IsRequired();
        builder.Property(x => x.Topic).IsRequired();
        builder.Property(x => x.RetryNumber).IsRequired();
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.FinishedAt).IsRequired(false);
        builder.Property(x => x.DurationMs).IsRequired(false);
        builder.Property(x => x.Operator).IsRequired(false);
        builder.Property(x => x.Result).IsRequired().HasConversion<string>();
        builder.Property(x => x.Exception).IsRequired(false);

        // Lists a row's retry history most-recent-first (GetRetryHistoryAsync).
        builder.HasIndex(x => new { x.InboxMessageId, x.StartedAt })
            .HasDatabaseName("idx_inbox_retry_history_message_started_at");

        // Finds the single open (FinishedAt == null) entry closed out by CompleteAttemptAsync/FailAttemptAsync.
        builder.HasIndex(x => new { x.InboxMessageId, x.FinishedAt })
            .HasDatabaseName("idx_inbox_retry_history_message_open");
    }
}
