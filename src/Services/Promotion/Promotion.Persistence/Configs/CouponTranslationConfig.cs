using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class CouponTranslationConfig : IEntityTypeConfiguration<CouponTranslation>
{
    public void Configure(EntityTypeBuilder<CouponTranslation> builder)
    {
        // Table
        builder.ToTable("coupon_translations");

        // Properties
        // Identity is CouponId + LanguageCode - no surrogate Id (Phase 3.1 Translation policy).
        builder.HasKey(x => new { x.CouponId, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Coupon)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
