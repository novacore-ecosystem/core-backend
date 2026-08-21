using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class PollOptionConfig : IEntityTypeConfiguration<PollOption>
{
    public void Configure(EntityTypeBuilder<PollOption> builder)
    {
        // Table
        builder.ToTable("poll_options");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content).HasMaxLength(300).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired().HasDefaultValue(0);

        // Relationships
        builder.HasOne(x => x.Poll)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.PollId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
