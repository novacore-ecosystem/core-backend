using NovaCore.Auth.Domain.Entities.Accounts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class AccountConfig : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // Table
        builder.ToTable("users");

        // Properties
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(AccountStatus.Active);

        builder.Property(a => a.IsMfaEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.FailedLoginCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasMany(a => a.AccountRoles)
            .WithOne(ar => ar.Account)
            .HasForeignKey(ar => ar.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.AccountPositions)
            .WithOne(ap => ap.Account)
            .HasForeignKey(ap => ap.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Permissions)
            .WithOne(p => p.Account)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.RefreshTokens)
            .WithOne(rt => rt.Account)
            .HasForeignKey(rt => rt.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Sessions)
            .WithOne(s => s.Account)
            .HasForeignKey(s => s.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Devices)
            .WithOne(d => d.Account)
            .HasForeignKey(d => d.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.MfaMethods)
            .WithOne(m => m.Account)
            .HasForeignKey(m => m.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.ExternalIdentities)
            .WithOne(e => e.Account)
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.PasswordHistories)
            .WithOne(p => p.Account)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        // Login (AccountReadService.GetByEmailAsync) filters the raw Email column - ASP.NET
        // Identity's own index only covers the normalized_email column, so this predicate was
        // otherwise served by a sequential scan.
        builder.HasIndex(a => a.Email);
        builder.HasIndex(a => a.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
