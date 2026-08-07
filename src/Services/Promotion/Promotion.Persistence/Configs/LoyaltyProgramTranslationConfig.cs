using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Promotion.Persistence.Configs;

public sealed class LoyaltyProgramTranslationConfig : IEntityTypeConfiguration<LoyaltyProgramTranslation>
{
    public void Configure(EntityTypeBuilder<LoyaltyProgramTranslation> builder)
    {
        // Table
        builder.ToTable("loyalty_program_translations");

        // Properties
        // Identity is ProgramId + LanguageCode - no surrogate Id (Phase 3.1 Translation policy).
        builder.HasKey(x => new { x.ProgramId, x.LanguageCode });

        builder.Property(x => x.LanguageCode)
            .HasConversion(x => x.Value, x => LanguageCode.Create(x))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);

        builder.ConfigureCommonFields();

        // Relationships
        builder.HasOne(x => x.Program)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
