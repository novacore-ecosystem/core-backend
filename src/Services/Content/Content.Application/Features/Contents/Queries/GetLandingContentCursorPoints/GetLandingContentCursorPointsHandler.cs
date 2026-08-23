using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Application.Common;
using NovaCore.Content.Application.Features.Contents.Queries.GetLandingContents;

namespace NovaCore.Content.Application.Features.Contents.Queries.GetLandingContentCursorPoints;

public sealed class GetLandingContentCursorPointsHandler(IContentReadService contentReadService)
    : IQueryHandler<GetLandingContentCursorPointsQuery, GetLandingContentCursorPointsResult>
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private const int DefaultPageCount = 5;
    private const int MaxPageCount = 50;

    public async Task<GetLandingContentCursorPointsResult> Handle(GetLandingContentCursorPointsQuery request, CancellationToken ct = default)
    {
        var pageSize = request.PageSize is > 0 and <= MaxPageSize ? request.PageSize : DefaultPageSize;
        var pageCount = request.PageCount is > 0 and <= MaxPageCount ? request.PageCount : DefaultPageCount;
        var language = ContentLanguageDefaults.OrDefault(request.Language);

        var cursorPoints = new List<string>(pageCount);
        (DateTime PublishedAt, Guid Id)? cursor = null;

        for (var page = 0; page < pageCount; page++)
        {
            var items = await contentReadService.SearchLandingAsync(
                request.ContentTypeId, language, ContentLanguageDefaults.Default,
                cursor?.PublishedAt, cursor?.Id, pageSize, ct);

            // SearchLandingAsync fetches pageSize (not pageSize + 1) here - we only need the exact
            // boundary of each full page, not a "has more" flag.
            if (items.Count < pageSize)
                break;

            var last = items[^1];
            cursor = (last.PublishedAt, last.Id);
            cursorPoints.Add(ContentLandingCursor.Encode(last.PublishedAt, last.Id));
        }

        return new GetLandingContentCursorPointsResult(pageSize, cursorPoints);
    }
}
