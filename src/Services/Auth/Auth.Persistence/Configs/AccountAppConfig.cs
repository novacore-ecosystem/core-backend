using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NovaCore.Auth.Domain.Entities.Accounts;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class AccountAppConfig : IEntityTypeConfiguration<AccountApp>
{
    public void Configure(EntityTypeBuilder<AccountApp> builder)
    {
        // Table
        builder.ToTable("account_apps");

        // Properties
        builder.HasKey(x => new { x.AccountId, x.AppId });

        // Relationships
        builder.HasOne(x => x.Account)
            .WithMany(a => a.AccountApps)
            .HasForeignKey(x => x.AccountId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.App)
            .WithMany()
            .HasForeignKey(x => x.AppId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
