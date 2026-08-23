namespace NovaCore.Content.Application.Features.Contents.Queries.GetLandingContentCursorPoints;

/// <summary>
/// Same filter/pagination semantics as GetLandingContentsQuery, but returns only the id of the
/// last item on each page instead of full content - lets the frontend pre-fetch every page's
/// starting cursor once (in the background) and jump straight to page N later without offset
/// pagination. Deliberately approximate: if content changes between this call and a later page
/// request, a record may shift pages - acceptable for a WCM/landing search session (see WCM spec
/// section 16), not a snapshot/consistency guarantee.
/// </summary>
public sealed record GetLandingContentCursorPointsQuery(
    Guid? ContentTypeId,
    string? Language,
    int PageSize,
    int PageCount) : IQuery<GetLandingContentCursorPointsResult>;

public sealed record GetLandingContentCursorPointsResult(int PageSize, IReadOnlyList<string> CursorPoints);
