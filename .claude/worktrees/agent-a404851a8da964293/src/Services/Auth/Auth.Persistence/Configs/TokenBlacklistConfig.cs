using NovaCore.Auth.Domain.Entities.TokenBlacklists;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class TokenBlacklistConfig : IEntityTypeConfiguration<TokenBlacklist>
{
    public void Configure(EntityTypeBuilder<TokenBlacklist> builder)
    {
        // Table
        builder.ToTable("token_blacklists");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Jti)
            .IsRequired();

        builder.Property(x => x.AccountId)
            .IsRequired();

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.BlacklistedAt)
            .IsRequired();

        // No FK relationship to Account - this is a standalone denylist, not an owned child
        // (see TokenBlacklist's class doc comment), and outlives the token's own lifecycle checks
        // independent of the Account row.

        // Indexes
        builder.HasIndex(x => x.Jti)
            .IsUnique();
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.ExpiresAt);

        // Audit & Concurrency
        // Append-only record - no updates after creation.
        builder.ConfigureAuditFields();
    }
}
