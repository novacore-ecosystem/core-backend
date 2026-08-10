using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Entities.Positions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class AccountPositionConfig : IEntityTypeConfiguration<AccountPosition>
{
    public void Configure(EntityTypeBuilder<AccountPosition> builder)
    {
        // Table
        builder.ToTable("account_positions");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.PositionId)
            .IsRequired();

        builder.Property(x => x.AssignedAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(AccountPositionStatus.Active);

        // Relationships
        builder.HasOne(x => x.Account)
            .WithMany(a => a.AccountPositions)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not Cascade - a Position with grant history should not be deletable while
        // that history still references it, per project standard to prefer restrictive
        // relationships for important business/audit data.
        builder.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.AccountId);
        // Serves Account.AssignPosition's "does an active assignment already exist" check.
        builder.HasIndex(x => new { x.AccountId, x.PositionId, x.Status });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
