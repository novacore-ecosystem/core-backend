using System.Linq.Expressions;

using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository.Helpers;
using NovaCore.BuildingBlock.Persistence.Repository;

using EfDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace NovaCore.BuildingBlock.Persistence.Ef.Repository;

public abstract class EntityGenericRepository<TContext, TEntity, TId>(TContext context)
    : GenericRepository<TContext, TEntity>(context), IRepository<TEntity, TId>
        where TContext : EfDbContext
        where TEntity : class, IEntity<TId>
{
    #region Get

    public Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken ct = default)
    {
        return GetAsync(
            entity => entity.Id,
            id,
            ct);
    }
    public Task<TEntity?> GetByIdAsync(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        CancellationToken ct = default)
    {
        return GetAsync(
            entity => entity.Id,
            id,
            includes,
            ct);
    }

    public async Task<List<TEntity>> GetManyByIdsAsync(
        IEnumerable<TId> ids,
        CancellationToken ct = default)
    {
        var entities = await GetManyAsync(
            entity => entity.Id,
            ids,
            ct);

        return [.. entities];
    }

    public async Task<List<TEntity>> GetManyByIdsAsync(
        IEnumerable<TId> ids,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        CancellationToken ct = default)
    {
        var entities = await GetManyAsync(
            entity => entity.Id,
            ids,
            includes,
            ct);

        return [.. entities];
    }

    #endregion

    #region Exists

    public Task<bool> ExistsByIdAsync(
        TId id,
        CancellationToken ct = default)
    {
        return ExistsAsync(
            entity => entity.Id,
            id,
            ct);
    }

    public Task<HashSet<TId>> GetExistingIdsAsync(
        IEnumerable<TId> ids,
        CancellationToken ct = default)
    {
        return GetExistingValuesAsync(
            entity => entity.Id,
            ids,
            ct);
    }

    #endregion

    #region Update

    public Task UpdateAsync(
        TId id,
        Action<TEntity> updateAction,
        CancellationToken ct = default)
    {
        return UpdateAsync(
            BuildIdPredicate(id),
            updateAction,
            ct);
    }
    public Task UpdateAsync(
        TId id,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        return UpdateAsync(
            BuildIdPredicate(id),
            updateAction,
            ct);
    }
    public Task UpdateAsync(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Action<TEntity> updateAction,
        CancellationToken ct = default)
    {
        return UpdateAsync(
            BuildIdPredicate(id),
            includes,
            updateAction,
            ct);
    }
    public Task UpdateAsync(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        return UpdateAsync(
            BuildIdPredicate(id),
            includes,
            updateAction,
            ct);
    }

    #endregion

    #region Delete

    public Task DeleteByIdAsync(
        TId id,
        CancellationToken ct = default)
    {
        return DeleteAsync(
            BuildIdPredicate(id),
            ct);
    }

    public async Task DeleteRangeAsync(
        IEnumerable<TId> ids,
        CancellationToken ct = default)
    {
        await DeleteWithNoTrackingAsync(
            BuildIdsPredicate(ids),
            ct);
    }

    #endregion

    #region Internal Helper

    protected Expression<Func<TEntity, bool>> BuildIdPredicate(TId id)
    {
        return PredicateExpressionBuilder.BuildIdEquals<TEntity, TId>(id);
    }

    protected Expression<Func<TEntity, bool>> BuildIdsPredicate(
        IEnumerable<TId> ids)
    {
        return PredicateExpressionBuilder.BuildIdIn<TEntity, TId>(ids);
    }

    #endregion
}
