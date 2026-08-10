namespace NovaCore.Audit.Application.Abstractions.Persistence.AuditLogs;

public interface IAuditLogWriteService
{
    Task AddAsync(AuditLogEntry entity, CancellationToken ct = default);
}
