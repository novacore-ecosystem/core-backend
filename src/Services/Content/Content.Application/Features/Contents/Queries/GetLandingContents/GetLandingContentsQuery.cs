using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Content.Application.Features.Contents.Queries.GetLandingContents;

/// <summary>Public landing-page content list - published content only, resolved to one language
/// via request-language-then-default fallback. Cursor-paginated on a fixed
/// (PublishedAt desc, Id desc) keyset; supports only a contentType filter for now (see WCM spec
/// section 14 - additional filters are future work, layered onto the same cursor shape).</summary>
public sealed record GetLandingContentsQuery(
    Guid? ContentTypeId,
    string? Language,
    string? Cursor,
    int Limit) : IQuery<CursorPaginatedResult<GetLandingContentsItemResponse>>;

public sealed record GetLandingContentsItemResponse(
    Guid Id,
    Guid ContentTypeId,
    string Slug,
    string Language,
    string Title,
    string Summary,
    DateTime PublishedAt);
