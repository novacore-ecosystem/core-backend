using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class PollVoteConfig : IEntityTypeConfiguration<PollVote>
{
    public void Configure(EntityTypeBuilder<PollVote> builder)
    {
        // Table
        builder.ToTable("poll_votes");

        // Properties
        builder.HasKey(x => new { x.PollOptionId, x.UserId });

        // PollId is a plain denormalized column (spec section 31's explicit property list) -
        // the true uniqueness constraint is the (PollOptionId, UserId) key above.
        builder.Property(x => x.PollId).IsRequired();

        // Relationships
        builder.HasOne(x => x.PollOption)
            .WithMany()
            .HasForeignKey(x => x.PollOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Poll>()
            .WithMany()
            .HasForeignKey(x => x.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PollId);
        builder.HasIndex(x => x.UserId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
