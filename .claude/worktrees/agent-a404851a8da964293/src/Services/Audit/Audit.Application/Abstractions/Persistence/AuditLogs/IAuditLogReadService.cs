namespace NovaCore.Audit.Application.Abstractions.Persistence.AuditLogs;

public interface IAuditLogReadService
{
    Task<AuditLogEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<(IReadOnlyList<AuditLogEntry> Items, int TotalCount)> SearchAsync(
        string? service,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
