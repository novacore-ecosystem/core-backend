using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserContactConfig : IEntityTypeConfiguration<UserContact>
{
    public void Configure(EntityTypeBuilder<UserContact> builder)
    {
        // Table
        builder.ToTable("user_contacts");

        // Properties
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ContactType)
            .IsRequired()
            .HasConversion<short>();

        builder.Property(x => x.Value)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Label)
            .HasMaxLength(100);

        builder.Property(x => x.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.VerifiedAt);

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany(u => u.Contacts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        // User.AddContact rejects a duplicate (ContactType, Value) pair for the same user.
        builder.HasIndex(x => new { x.UserId, x.ContactType, x.Value })
            .IsUnique();
        builder.HasIndex(x => new { x.UserId, x.ContactType, x.IsPrimary });

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
