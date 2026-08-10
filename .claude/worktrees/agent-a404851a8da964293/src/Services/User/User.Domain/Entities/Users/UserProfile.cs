namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned 1:1 extension of User holding optional personal/demographic details. Split from User
/// itself so the aggregate root stays lean (identity/status/type) while richer, rarely-queried
/// profile data lives separately.
/// </summary>
public sealed class UserProfile : BaseEntity, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public PersonalName PersonalName { get; private set; } = null!;
    public DateOnly? Birthday { get; private set; }
    public Gender Gender { get; private set; } = Gender.Unknown;
    public string Biography { get; private set; } = string.Empty;
    public string? Occupation { get; private set; }
    public string? Company { get; private set; }
    public string? Website { get; private set; }
    public LanguageCode? Language { get; private set; }
    public string? TimeZone { get; private set; }
    public string? CountryCode { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserProfile() { }

    public static UserProfile Create(
        Guid userId,
        PersonalName personalName,
        DateOnly? birthday = null,
        Gender gender = Gender.Unknown,
        string biography = "",
        string? occupation = null,
        string? company = null,
        string? website = null,
        LanguageCode? language = null,
        string? timeZone = null,
        string? countryCode = null)
    {
        return new UserProfile
        {
            UserId = userId,
            PersonalName = personalName,
            Birthday = birthday,
            Gender = gender,
            Biography = biography,
            Occupation = occupation,
            Company = company,
            Website = website,
            Language = language,
            TimeZone = timeZone,
            CountryCode = countryCode,
        };
    }

    internal void UpdateDetails(
        PersonalName personalName,
        DateOnly? birthday,
        Gender gender,
        string biography,
        string? occupation,
        string? company,
        string? website,
        LanguageCode? language,
        string? timeZone,
        string? countryCode)
    {
        PersonalName = personalName;
        Birthday = birthday;
        Gender = gender;
        Biography = biography;
        Occupation = occupation;
        Company = company;
        Website = website;
        Language = language;
        TimeZone = timeZone;
        CountryCode = countryCode;
    }
}
