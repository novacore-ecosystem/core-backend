using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserPaymentMethodConfig : IEntityTypeConfiguration<UserPaymentMethod>
{
    public void Configure(EntityTypeBuilder<UserPaymentMethod> builder)
    {
        // Table
        builder.ToTable("user_payment_methods");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.PaymentAccountId)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.PaymentMethods)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsDefault });
        builder.HasIndex(x => x.PaymentAccountId)
            .IsUnique();

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
