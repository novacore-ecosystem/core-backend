using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationScheduleConfig : IEntityTypeConfiguration<ConversationSchedule>
{
    public void Configure(EntityTypeBuilder<ConversationSchedule> builder)
    {
        // Table
        builder.ToTable("conversation_schedules");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.Type).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(4000);
        builder.Property(x => x.ScheduledAt).IsRequired();
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
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
        builder.HasIndex(x => new { x.Status, x.ScheduledAt });
    }
}
