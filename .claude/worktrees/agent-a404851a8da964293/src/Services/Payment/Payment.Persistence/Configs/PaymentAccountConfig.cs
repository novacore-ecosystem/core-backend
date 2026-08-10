using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Payment.Persistence.Configs;

public sealed class PaymentAccountConfig : IEntityTypeConfiguration<PaymentAccount>
{
    public void Configure(EntityTypeBuilder<PaymentAccount> builder)
    {
        // Table
        builder.ToTable("payment_accounts");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerReferenceId).IsRequired();
        builder.Property(x => x.AccountType).HasConversion<short>().IsRequired();
        builder.Property(x => x.Token).HasMaxLength(500).IsRequired();
        builder.Property(x => x.MaskedNumber).HasMaxLength(30);
        builder.Property(x => x.HolderName).HasMaxLength(200);
        builder.Property(x => x.Issuer).HasMaxLength(100);
        builder.Property(x => x.IsDefault).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.IsVerified).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.Metadata).HasColumnType("jsonb");

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasMany(x => x.Tokens)
            .WithOne()
            .HasForeignKey(t => t.PaymentAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.OwnerReferenceId);
        builder.HasIndex(x => new { x.OwnerReferenceId, x.IsDefault });
    }
}
