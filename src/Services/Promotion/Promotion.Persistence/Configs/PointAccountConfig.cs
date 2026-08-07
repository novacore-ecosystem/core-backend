using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class PointAccountConfig : IEntityTypeConfiguration<PointAccount>
{
    public void Configure(EntityTypeBuilder<PointAccount> builder)
    {
        // Table
        builder.ToTable("point_accounts");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AvailablePoints)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .HasColumnName("available_points")
            .IsRequired();

        builder.Property(x => x.PendingPoints)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .HasColumnName("pending_points")
            .IsRequired();

        builder.Property(x => x.ExpiredPoints)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .HasColumnName("expired_points")
            .IsRequired();

        builder.Property(x => x.LifetimeEarned)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .HasColumnName("lifetime_earned")
            .IsRequired();

        builder.Property(x => x.LifetimeSpent)
            .HasConversion(x => x.Value, x => Quantity.Create(x))
            .HasColumnName("lifetime_spent")
            .IsRequired();

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.Accounts)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // Transactions/Expirations/Adjustments/History are all configured from the child entity's
        // own config (single source per relationship).

        // Indexes
        builder.HasIndex(x => new { x.UserId, x.ProgramId }).IsUnique();
    }
}
