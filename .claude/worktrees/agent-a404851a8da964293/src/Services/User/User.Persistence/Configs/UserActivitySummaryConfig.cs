using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserActivitySummaryConfig : IEntityTypeConfiguration<UserActivitySummary>
{
    public void Configure(EntityTypeBuilder<UserActivitySummary> builder)
    {
        // Table
        builder.ToTable("user_activity_summaries");

        // Properties
        // Shared primary key (1:1 with User) - no surrogate Id, exactly one summary row per user.
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.LastLoginAt);
        builder.Property(x => x.LastOrderAt);
        builder.Property(x => x.LastPurchaseAt);

        builder.Property(x => x.TotalLoginCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.TotalOrderCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.TotalSpentAmount)
            .HasColumnType("numeric(18,2)")
            .IsRequired()
            .HasDefaultValue(0m);

        builder.Property(x => x.FavoriteCategory);

        // Indexes
        builder.HasIndex(x => x.FavoriteCategory);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
