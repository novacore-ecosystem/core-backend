using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.User.Persistence.Configs;

public sealed class UserSettingConfig : IEntityTypeConfiguration<UserSetting>
{
    public void Configure(EntityTypeBuilder<UserSetting> builder)
    {
        // Table
        builder.ToTable("user_settings");

        // Properties
        // Shared primary key (1:1 with User) - no surrogate Id, exactly one settings row per user.
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.Theme)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(ThemeMode.System);

        builder.Property(x => x.Language)
            .HasConversion(
                x => x == null ? null : x.Value,
                x => x == null ? null : LanguageCode.Create(x))
            .HasMaxLength(10);

        builder.Property(x => x.Currency)
            .HasMaxLength(3);

        builder.Property(x => x.TimeZone)
            .HasMaxLength(50);

        builder.Property(x => x.DateFormat)
            .HasMaxLength(50);

        builder.Property(x => x.TimeFormat)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(TimeFormat.TwentyFourHours);

        builder.Property(x => x.FirstDayOfWeek)
            .IsRequired()
            .HasConversion<short>()
            .HasDefaultValue(WeekDay.Monday);

        builder.Property(x => x.DashboardLayout)
            .HasMaxLength(50);

        builder.Property(x => x.SidebarCollapsed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ItemsPerPage)
            .IsRequired()
            .HasDefaultValue(20);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
