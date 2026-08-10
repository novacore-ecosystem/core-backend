using NovaCore.BuildingBlock.Application.Abstractions.Common;

using NovaCore.Audit.Application.Abstractions.Persistence.AuditLogs;

using Mapster;

namespace NovaCore.Audit.Application.Features.AuditLogs.Queries.ListAuditLogs;

public sealed class ListAuditLogsHandler(IAuditLogReadService auditLogReadService)
    : IQueryHandler<ListAuditLogsQuery, PaginatedResult<AuditLogSummaryResponse>>
{
    public async Task<PaginatedResult<AuditLogSummaryResponse>> Handle(ListAuditLogsQuery request, CancellationToken ct = default)
    {
        var (items, totalCount) = await auditLogReadService.SearchAsync(
            request.Service, request.From, request.To, request.Page, request.PageSize, ct);

        var mapped = items.Adapt<List<AuditLogSummaryResponse>>();

        return PaginatedResult<AuditLogSummaryResponse>.Create(mapped, request.Page, request.PageSize, totalCount);
    }
}
