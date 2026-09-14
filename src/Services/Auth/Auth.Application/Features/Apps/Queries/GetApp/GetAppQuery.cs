namespace NovaCore.Auth.Application.Features.Apps.Queries.GetApp;

public sealed record GetAppQuery(Guid Id) : IQuery<AppDetailResponse>;

public sealed record AppDetailResponse(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    IReadOnlyList<AppTranslationResponse> Translations,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record AppTranslationResponse(
    string LanguageCode,
    string DisplayName,
    string? Description);
