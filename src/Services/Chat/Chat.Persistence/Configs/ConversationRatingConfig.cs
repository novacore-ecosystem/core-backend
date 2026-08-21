using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationRatingConfig : IEntityTypeConfiguration<ConversationRating>
{
    public void Configure(EntityTypeBuilder<ConversationRating> builder)
    {
        // Table
        builder.ToTable("conversation_ratings");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConversationId).IsRequired();
        builder.Property(x => x.RatedByUserId).IsRequired();
        builder.Property(x => x.Stars).IsRequired();
        builder.Property(x => x.Review).HasMaxLength(2000);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        // At most one rating per conversation - row existence is the "was it submitted" flag.
        builder.HasIndex(x => x.ConversationId).IsUnique();
    }
}
