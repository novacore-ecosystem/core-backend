using NovaCore.Audit.Application.Abstractions.Persistence.AuditLogs;
using NovaCore.Audit.Persistence.Engine;

namespace NovaCore.Audit.Persistence.Contexts.AuditLogs.Read;

public sealed class AuditLogReadService(AuditMongoContext context) : IAuditLogReadService
{
    public async Task<AuditLogEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.AuditLogs.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<AuditLogEntry> Items, int TotalCount)> SearchAsync(
        string? service,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<AuditLogEntry>.Filter;
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrWhiteSpace(service))
            filter &= filterBuilder.Eq(x => x.Service, service);

        if (from is not null)
            filter &= filterBuilder.Gte(x => x.Timestamp, from.Value);

        if (to is not null)
            filter &= filterBuilder.Lte(x => x.Timestamp, to.Value);

        var totalCount = (int)await context.AuditLogs.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await context.AuditLogs
            .Find(filter)
            .SortByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
