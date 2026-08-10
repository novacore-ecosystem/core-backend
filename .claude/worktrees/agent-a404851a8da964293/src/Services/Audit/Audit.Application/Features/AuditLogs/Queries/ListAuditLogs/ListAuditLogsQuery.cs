using NovaCore.BuildingBlock.Application.Abstractions.Common;

namespace NovaCore.Audit.Application.Features.AuditLogs.Queries.ListAuditLogs;

public sealed record ListAuditLogsQuery(
    string? Service,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<AuditLogSummaryResponse>>;

public sealed record AuditLogSummaryResponse(
    Guid Id,
    string RootEntityType,
    string RootEntityId,
    string Service,
    string CorrelationId,
    DateTime Timestamp);
