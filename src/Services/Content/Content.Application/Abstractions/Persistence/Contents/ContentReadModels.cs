using NovaCore.Content.Domain.Enums;

namespace NovaCore.Content.Application.Abstractions.Persistence.Contents;

/// <summary>Lightweight row for the Admin content list - never carries the Editor.js body, only the
/// current version's title in <see cref="DisplayLanguage"/> (or any language if that one is
/// missing) for display.</summary>
public sealed record ContentAdminListItem(
    Guid Id,
    Guid ContentTypeId,
    string ContentTypeName,
    string Slug,
    ContentStatus Status,
    ContentVisibility Visibility,
    bool IsDeleted,
    string? Title,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>Lightweight row for the public Landing content list - published content only, resolved
/// to a single language via request-language-then-tenant-default fallback (see
/// NovaCore.Content.Application.Common.ContentLanguageDefaults). Never carries other languages,
/// drafts, or version history.</summary>
public sealed record ContentLandingItem(
    Guid Id,
    Guid ContentTypeId,
    string Slug,
    string Culture,
    string Title,
    string Summary,
    DateTime PublishedAt);
