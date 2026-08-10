using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class LoginHistoryConfig : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        // Table
        builder.ToTable("login_histories");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasConversion(x => x.Value, x => IpAddress.Create(x))
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.Result)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.AttemptedAt)
            .IsRequired();

        // Relationships
        // No back-collection on Account (one-directional) - LoginHistory is queried by AccountId,
        // never eager-loaded as part of the aggregate.
        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.AttemptedAt);

        // Audit & Concurrency
        // Append-only record - no updates after creation, so CreatedAt/UpdatedAt only, no
        // optimistic-concurrency token needed (same as InventoryTransaction).
        builder.ConfigureAuditFields();
    }
}
