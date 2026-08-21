using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class MessageMentionConfig : IEntityTypeConfiguration<MessageMention>
{
    public void Configure(EntityTypeBuilder<MessageMention> builder)
    {
        // Table
        builder.ToTable("message_mentions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.MentionType).HasConversion<byte>().IsRequired();

        // Span is a genuine 2-column Value Object (StartOffset+Length) with no string form -
        // mapped as an owned type rather than a HasConversion round-trip.
        builder.OwnsOne(x => x.Span, span =>
        {
            span.Property(x => x.StartOffset).HasColumnName("start_offset").IsRequired();
            span.Property(x => x.Length).HasColumnName("length").IsRequired();
        });

        // Relationships
        builder.HasOne(x => x.Message)
            .WithMany(x => x.Mentions)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.MessageId);
        builder.HasIndex(x => x.UserId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
