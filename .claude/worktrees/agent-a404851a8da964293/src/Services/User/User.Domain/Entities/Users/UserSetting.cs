namespace NovaCore.User.Domain.Entities.Users;

/// <summary>Owned 1:1 extension of User holding display/locale/UI preferences.</summary>
public sealed class UserSetting : BaseEntity, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public ThemeMode Theme { get; private set; } = ThemeMode.System;
    public LanguageCode? Language { get; private set; }
    public string? Currency { get; private set; }
    public string? TimeZone { get; private set; }
    public string? DateFormat { get; private set; }
    public TimeFormat TimeFormat { get; private set; } = TimeFormat.TwentyFourHours;
    public WeekDay FirstDayOfWeek { get; private set; } = WeekDay.Monday;
    public string? DashboardLayout { get; private set; }
    public bool SidebarCollapsed { get; private set; }
    public int ItemsPerPage { get; private set; } = 20;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserSetting() { }

    public static UserSetting Create(
        Guid userId,
        ThemeMode theme = ThemeMode.System,
        LanguageCode? language = null,
        string? currency = null,
        string? timeZone = null,
        string? dateFormat = null,
        TimeFormat timeFormat = TimeFormat.TwentyFourHours,
        WeekDay firstDayOfWeek = WeekDay.Monday,
        string? dashboardLayout = null,
        bool sidebarCollapsed = false,
        int itemsPerPage = 20)
    {
        ValidateItemsPerPage(itemsPerPage);

        return new UserSetting
        {
            UserId = userId,
            Theme = theme,
            Language = language,
            Currency = currency,
            TimeZone = timeZone,
            DateFormat = dateFormat,
            TimeFormat = timeFormat,
            FirstDayOfWeek = firstDayOfWeek,
            DashboardLayout = dashboardLayout,
            SidebarCollapsed = sidebarCollapsed,
            ItemsPerPage = itemsPerPage,
        };
    }

    internal void UpdateDetails(
        ThemeMode theme,
        LanguageCode? language,
        string? currency,
        string? timeZone,
        string? dateFormat,
        TimeFormat timeFormat,
        WeekDay firstDayOfWeek,
        string? dashboardLayout,
        bool sidebarCollapsed,
        int itemsPerPage)
    {
        ValidateItemsPerPage(itemsPerPage);

        Theme = theme;
        Language = language;
        Currency = currency;
        TimeZone = timeZone;
        DateFormat = dateFormat;
        TimeFormat = timeFormat;
        FirstDayOfWeek = firstDayOfWeek;
        DashboardLayout = dashboardLayout;
        SidebarCollapsed = sidebarCollapsed;
        ItemsPerPage = itemsPerPage;
    }

    private static void ValidateItemsPerPage(int itemsPerPage)
    {
        if (itemsPerPage <= 0)
            throw ExceptionFactory.InvalidRange("Items per page must be greater than zero.");
    }
}
