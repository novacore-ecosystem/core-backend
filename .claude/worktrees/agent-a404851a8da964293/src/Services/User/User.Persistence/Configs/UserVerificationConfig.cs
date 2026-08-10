using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserVerificationConfig : IEntityTypeConfiguration<UserVerification>
{
    public void Configure(EntityTypeBuilder<UserVerification> builder)
    {
        // Table
        builder.ToTable("user_verifications");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.VerificationType)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.VerificationStatus)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(VerificationStatus.Pending);

        builder.Property(x => x.VerifiedAt);
        builder.Property(x => x.ExpiredAt);

        builder.Property(x => x.Note)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.Verifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        // Serves User.RequestVerification's "already-pending" check.
        builder.HasIndex(x => new { x.UserId, x.VerificationType, x.VerificationStatus });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
