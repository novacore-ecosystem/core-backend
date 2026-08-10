using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Entities.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class AccountRoleConfig : IEntityTypeConfiguration<AccountRole>
{
    public void Configure(EntityTypeBuilder<AccountRole> builder)
    {
        // Table
        builder.ToTable("user_roles");

        // Properties
        builder.HasKey(ar => new { ar.UserId, ar.RoleId });

        // Relationships
        builder.HasOne(ar => ar.Account)
            .WithMany(a => a.AccountRoles)
            .HasForeignKey(ar => ar.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ar => ar.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ar => ar.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
