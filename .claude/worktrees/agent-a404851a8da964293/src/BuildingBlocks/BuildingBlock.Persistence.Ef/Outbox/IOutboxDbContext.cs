using Microsoft.EntityFrameworkCore;

namespace NovaCore.BuildingBlock.Persistence.Ef.Outbox;

/// <summary>
/// Marker interface for DbContext implementations that provide access to the Outbox table.
/// Used by EfOutboxStore to remain generic across all service DbContext types.
/// </summary>
public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }
}
