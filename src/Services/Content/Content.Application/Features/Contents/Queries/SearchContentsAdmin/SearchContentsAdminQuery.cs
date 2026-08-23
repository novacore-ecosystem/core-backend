using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.Content.Application.Features.Contents.Queries.SearchContentsAdmin;

/// <summary>Admin/WCM content list - existing Criteria filters plus cursor pagination on a fixed
/// (CreatedAt desc, Id desc) keyset. Deliberately lightweight: never returns the Editor.js body.</summary>
public sealed record SearchContentsAdminQuery(
    CriteriaRequest Criteria,
    string? Cursor,
    int Limit,
    string? DisplayLanguage) : IQuery<CursorPaginatedResult<SearchContentsAdminItemResponse>>;

public sealed record SearchContentsAdminItemResponse(
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
