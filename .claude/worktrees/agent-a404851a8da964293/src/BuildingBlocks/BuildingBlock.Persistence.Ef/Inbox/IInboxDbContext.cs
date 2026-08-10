using Microsoft.EntityFrameworkCore;

namespace NovaCore.BuildingBlock.Persistence.Ef.Inbox;

/// <summary>
/// Marker interface for DbContext implementations that provide access to the Inbox table.
/// Used by EfInboxStore to remain generic across all service DbContext types.
/// </summary>
public interface IInboxDbContext
{
    DbSet<InboxMessage> InboxMessages { get; }
    DbSet<InboxRetryHistory> InboxRetryHistories { get; }
}
