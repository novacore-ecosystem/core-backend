using NovaCore.Audit.Persistence.Engine;

namespace NovaCore.Audit.Persistence.Contexts.AuditLogs.Repositories;

public sealed class AuditLogRepo(AuditMongoContext context) : IAuditLogRepository
{
    public async Task AddAsync(AuditLogEntry entity, CancellationToken ct = default)
    {
        await context.AuditLogs.InsertOneAsync(entity, cancellationToken: ct);
    }
}
