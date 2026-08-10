using NovaCore.Audit.Application.Abstractions.Persistence.AuditLogs;
using NovaCore.Audit.Persistence.Contexts.AuditLogs.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;

namespace NovaCore.Audit.Persistence.Contexts.AuditLogs.Write;

public sealed class AuditLogWriteService(
    IAuditLogRepository repo,
    IUnitOfWork unitOfWork) : IAuditLogWriteService
{
    public async Task AddAsync(AuditLogEntry entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);

        // Mongo's IUnitOfWork.SaveChangesAsync is a documented no-op (writes already committed
        // by InsertOneAsync above) - kept only so this Write Service's shape matches every other
        // service's, not because it does anything here.
        await unitOfWork.SaveChangesAsync(ct);
    }
}
