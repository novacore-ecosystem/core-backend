using NovaCore.BuildingBlock.Application.Abstractions.Common;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Application.Common;

namespace NovaCore.Content.Application.Features.Contents.Queries.SearchContentsAdmin;

public sealed class SearchContentsAdminHandler(IContentReadService contentReadService)
    : IQueryHandler<SearchContentsAdminQuery, CursorPaginatedResult<SearchContentsAdminItemResponse>>
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 100;

    public async Task<CursorPaginatedResult<SearchContentsAdminItemResponse>> Handle(SearchContentsAdminQuery request, CancellationToken ct = default)
    {
        var cursor = ContentAdminCursor.Decode(request.Cursor);
        var limit = request.Limit is > 0 and <= MaxLimit ? request.Limit : DefaultLimit;
        var displayLanguage = ContentLanguageDefaults.OrDefault(request.DisplayLanguage);

        var items = await contentReadService.SearchAdminAsync(
            request.Criteria, cursor?.CreatedAt, cursor?.Id, limit, displayLanguage, ct);

        var hasMore = items.Count > limit;
        var page = (hasMore ? items.Take(limit) : items)
            .Select(x => new SearchContentsAdminItemResponse(
                x.Id, x.ContentTypeId, x.ContentTypeName, x.Slug, x.Status, x.Visibility, x.IsDeleted,
                x.Title, x.CreatedAt, x.UpdatedAt))
            .ToList();

        var nextCursor = hasMore
            ? ContentAdminCursor.Encode(page[^1].CreatedAt, page[^1].Id)
            : null;

        return CursorPaginatedResult<SearchContentsAdminItemResponse>.Create(page, nextCursor);
    }
}
