using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserProfileConfig : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        // Table
        builder.ToTable("user_profiles");

        // Properties
        // Shared primary key (1:1 with User) - no surrogate Id, exactly one profile row per user.
        builder.HasKey(x => x.UserId);

        builder.OwnsOne(x => x.PersonalName, name =>
        {
            name.Property(n => n.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            name.Property(n => n.MiddleName)
                .HasColumnName("middle_name")
                .HasMaxLength(100);

            name.Property(n => n.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();

            name.Property(n => n.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(300)
                .IsRequired();
        });

        builder.Navigation(x => x.PersonalName).IsRequired();

        builder.Property(x => x.Birthday);

        builder.Property(x => x.Gender)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(Gender.Unknown);

        builder.Property(x => x.Biography)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Occupation)
            .HasMaxLength(200);

        builder.Property(x => x.Company)
            .HasMaxLength(200);

        builder.Property(x => x.Website)
            .HasMaxLength(500);

        builder.Property(x => x.Language)
            .HasConversion(
                x => x == null ? null : x.Value,
                x => x == null ? null : LanguageCode.Create(x))
            .HasMaxLength(10);

        builder.Property(x => x.TimeZone)
            .HasMaxLength(50);

        builder.Property(x => x.CountryCode)
            .HasMaxLength(2);

        // Relationships
        // No Profile navigation property on User's side of the shared key beyond what's already
        // configured on UserConfig (HasOne<UserProfile>().WithOne(p => p.User) there).

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
