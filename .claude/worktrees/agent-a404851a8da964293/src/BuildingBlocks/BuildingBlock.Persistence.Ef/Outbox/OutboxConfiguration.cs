using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.BuildingBlock.Persistence.Ef.Outbox;

public sealed class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).IsRequired();
        builder.Property(x => x.Topic).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.CorrelationId).IsRequired();
        builder.Property(x => x.ActorId).IsRequired(false);
        builder.Property(x => x.ActorType).IsRequired().HasDefaultValue("system");
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ProcessedAt).IsRequired(false);
        builder.Property(x => x.Error).IsRequired(false);
        builder.Property(x => x.RetryCount).IsRequired();

        builder.HasIndex(x => x.ProcessedAt)
            .HasDatabaseName("idx_outbox_processed_at");

        // Covers the relay's hot poll: WHERE ProcessedAt IS NULL ORDER BY CreatedAt. Partial so the
        // index stays small as processed rows accumulate (the ProcessedAt index above already
        // serves the IS NULL predicate, but not the CreatedAt sort).
        builder.HasIndex(x => x.CreatedAt)
            .HasFilter("\"processed_at\" IS NULL")
            .HasDatabaseName("idx_outbox_unprocessed_created_at");
    }
}
