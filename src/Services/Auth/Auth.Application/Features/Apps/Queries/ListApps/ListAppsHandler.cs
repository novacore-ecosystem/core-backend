using NovaCore.Auth.Application.Abstractions.Persistence.Apps;

using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Auth.Application.Features.Apps.Queries.ListApps;

public sealed class ListAppsHandler(IAppReadService appReadService)
    : IQueryHandler<ListAppsQuery, PaginatedResult<AppSummaryResponse>>
{
    public async Task<PaginatedResult<AppSummaryResponse>> Handle(ListAppsQuery request, CancellationToken ct = default)
    {
        var (apps, totalCount) = await appReadService.SearchAsync(
            request.Search,
            request.Page,
            request.PageSize,
            ct);

        var items = apps.Select(a => new AppSummaryResponse(a.Id, a.Code.Value, a.Name, a.IsActive));

        return PaginatedResult<AppSummaryResponse>.Create(
            items,
            request.Page,
            request.PageSize,
            totalCount);
    }
}
