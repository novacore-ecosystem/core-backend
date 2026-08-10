using NovaCore.Auth.Domain.Entities.Accounts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Auth.Persistence.Configs;

public sealed class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Table
        builder.ToTable("refresh_tokens");

        // Properties
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.JwtId)
            .IsRequired()
            .HasComment("Access token JWT ID");

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.ExpiryDate)
            .IsRequired();

        builder.Property(rt => rt.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rt => rt.RevokedReason)
            .HasConversion<short?>();

        // Relationships
        builder.HasOne(rt => rt.Account)
            .WithMany(a => a.RefreshTokens)
            .HasForeignKey(rt => rt.AccountId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rt => rt.Session)
            .WithMany()
            .HasForeignKey(rt => rt.SessionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(rt => rt.JwtId)
            .IsUnique()
            .HasDatabaseName("idx_refresh_tokens_jwt_id");

        builder.HasIndex(rt => rt.AccountId)
            .HasDatabaseName("idx_refresh_tokens_user_id");

        builder.HasIndex(rt => rt.SessionId)
            .HasDatabaseName("idx_refresh_tokens_session_id");

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
