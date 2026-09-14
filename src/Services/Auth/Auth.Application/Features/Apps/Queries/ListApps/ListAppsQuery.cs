using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Auth.Application.Features.Apps.Queries.ListApps;

public sealed record ListAppsQuery(
    string? Search = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<AppSummaryResponse>>;

public sealed record AppSummaryResponse(
    Guid Id,
    string Code,
    string Name,
    bool IsActive);
