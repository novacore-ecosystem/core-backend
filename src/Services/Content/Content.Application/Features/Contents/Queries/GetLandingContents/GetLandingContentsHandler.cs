using NovaCore.BuildingBlock.Application.Abstractions.Common;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Application.Common;

namespace NovaCore.Content.Application.Features.Contents.Queries.GetLandingContents;

public sealed class GetLandingContentsHandler(IContentReadService contentReadService)
    : IQueryHandler<GetLandingContentsQuery, CursorPaginatedResult<GetLandingContentsItemResponse>>
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 100;

    public async Task<CursorPaginatedResult<GetLandingContentsItemResponse>> Handle(GetLandingContentsQuery request, CancellationToken ct = default)
    {
        var cursor = ContentLandingCursor.Decode(request.Cursor);
        var limit = request.Limit is > 0 and <= MaxLimit ? request.Limit : DefaultLimit;
        var language = ContentLanguageDefaults.OrDefault(request.Language);

        var items = await contentReadService.SearchLandingAsync(
            request.ContentTypeId, language, ContentLanguageDefaults.Default, cursor?.PublishedAt, cursor?.Id, limit, ct);

        var hasMore = items.Count > limit;
        var page = (hasMore ? items.Take(limit) : items)
            .Select(x => new GetLandingContentsItemResponse(
                x.Id, x.ContentTypeId, x.Slug, x.Culture, x.Title, x.Summary, x.PublishedAt))
            .ToList();

        var nextCursor = hasMore
            ? ContentLandingCursor.Encode(page[^1].PublishedAt, page[^1].Id)
            : null;

        return CursorPaginatedResult<GetLandingContentsItemResponse>.Create(page, nextCursor);
    }
}
