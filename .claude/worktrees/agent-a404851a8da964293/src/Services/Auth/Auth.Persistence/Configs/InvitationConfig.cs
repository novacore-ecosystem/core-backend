using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Entities.Invitations;
using NovaCore.Auth.Domain.Entities.Roles;

using NovaCore.BuildingBlock.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class InvitationConfig : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        // Table
        builder.ToTable("invitations");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasConversion(x => x.Value, x => Email.Create(x))
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.InvitedByAccountId)
            .IsRequired();

        builder.Property(x => x.RoleId)
            .IsRequired();

        builder.Property(x => x.Token)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(InvitationStatus.Pending);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        // Relationships
        // No nav properties on either side (shadow references) - Invitation isn't owned by
        // Account or Role, it only points at them.
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.InvitedByAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Token)
            .IsUnique();
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
