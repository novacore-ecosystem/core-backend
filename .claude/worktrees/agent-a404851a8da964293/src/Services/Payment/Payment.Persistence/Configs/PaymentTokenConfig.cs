using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentTokenConfig : IEntityTypeConfiguration<PaymentToken>
{
    public void Configure(EntityTypeBuilder<PaymentToken> builder)
    {
        // Table
        builder.ToTable("payment_tokens");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GatewayId).IsRequired();
        builder.Property(x => x.Token).HasMaxLength(500).IsRequired();
        builder.Property(x => x.TokenType).HasMaxLength(50).IsRequired();

        builder.ConfigureAuditFields();

        // Indexes
        builder.HasIndex(x => x.PaymentAccountId);
        builder.HasIndex(x => new { x.PaymentAccountId, x.GatewayId }).IsUnique();
    }
}
