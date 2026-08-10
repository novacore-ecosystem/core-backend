namespace NovaCore.User.Application.Abstractions.Search;

/// <summary>
/// Query params for User Search. Extending this later with e.g. a CreatedAt range is just a new
/// optional field here - no redesign of the repository/query pipeline.
/// </summary>
public sealed record UserSearchCriteria(
    string? Keyword,
    string? Role,
    bool ExcludeRole,
    string? Status,
    string? PhonePrefix,
    string? PhoneSuffix,
    string? SortBy,
    bool SortDescending,
    int Page,
    int PageSize);
