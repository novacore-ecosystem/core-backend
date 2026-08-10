namespace NovaCore.Audit.Persistence.Contexts.AuditLogs.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntry entity, CancellationToken ct = default);
}
