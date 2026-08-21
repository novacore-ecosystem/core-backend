using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationResponsibilityHistoryConfig : IEntityTypeConfiguration<ConversationResponsibilityHistory>
{
    public void Configure(EntityTypeBuilder<ConversationResponsibilityHistory> builder)
    {
        // Table
        builder.ToTable("conversation_responsibility_histories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ResponsibilityType).HasConversion<byte>().IsRequired();
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.Source).HasConversion<byte>().IsRequired();

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
        builder.HasIndex(x => x.UserId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
