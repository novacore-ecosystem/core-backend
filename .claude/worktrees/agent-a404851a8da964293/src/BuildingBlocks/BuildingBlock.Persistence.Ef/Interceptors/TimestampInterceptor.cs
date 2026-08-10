using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.DbContext;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NovaCore.BuildingBlock.Persistence.Ef.Interceptors;

public sealed class TimestampInterceptor : ISaveChangesInterceptor
{
    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);

        return result;
    }

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        Stamp(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private static void Stamp(Microsoft.EntityFrameworkCore.DbContext? context)
    {
        if (context is not DbContextBase { IsDisableTimestamps: false } dbContext)
            return;

        var entries = dbContext.ChangeTracker.Entries<IEntity>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
                entry.Entity.Track();
            else
                entry.Entity.Touch();
        }
    }
}
