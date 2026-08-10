using System.Linq.Expressions;

using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository.Helpers;
using NovaCore.BuildingBlock.Persistence.Repository;

using Microsoft.EntityFrameworkCore;
using EfDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace NovaCore.BuildingBlock.Persistence.Ef.Repository;

public abstract class GenericRepository<TContext, TEntity>(TContext context) : IRepository<TEntity>
    where TContext : EfDbContext
    where TEntity : class, IEntity
{
    protected readonly TContext _dbContext = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    #region Get

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, ct);
    }
    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        CancellationToken ct = default)
    {
        return await ApplyIncludes(includes)
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, ct);
    }
    public async Task<TEntity?> GetAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        TValue value,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                PredicateExpressionBuilder.BuildEquals(selector, value),
                ct);
    }
    public async Task<TEntity?> GetAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        TValue value,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        CancellationToken ct = default)
    {
        return await ApplyIncludes(includes)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                PredicateExpressionBuilder.BuildEquals(selector, value),
                ct);
    }

    public async Task<TEntity[]> GetManyAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        IEnumerable<TValue> values,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(PredicateExpressionBuilder.BuildIn(selector, values))
            .ToArrayAsync(ct);
    }
    public async Task<TEntity[]> GetManyAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        IEnumerable<TValue> values,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        CancellationToken ct = default)
    {
        return await ApplyIncludes(includes)
            .AsNoTracking()
            .Where(PredicateExpressionBuilder.BuildIn(selector, values))
            .ToArrayAsync(ct);
    }

    #endregion

    #region Exists

    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(predicate, ct);
    }
    public async Task<bool> ExistsAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        TValue value,
        CancellationToken ct = default)
    {
        return await ExistsAsync(
            PredicateExpressionBuilder.BuildEquals(selector, value),
            ct);
    }

    public async Task<HashSet<TValue>> GetExistingValuesAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        IEnumerable<TValue> values,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(PredicateExpressionBuilder.BuildIn(selector, values))
            .Select(selector)
            .ToHashSetAsync(ct);
    }

    #endregion

    #region Aggregate

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        return await _dbSet
            .AsNoTracking()
            .CountAsync(predicate, ct);
    }

    #endregion

    #region Add

    public async Task AddAsync(
        TEntity entity,
        CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken ct = default)
    {
        await _dbSet.AddRangeAsync(entities, ct);
    }

    #endregion

    #region Update

    public async Task UpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<TEntity> updateAction,
        CancellationToken ct = default)
    {
        var entity = await FirstOrThrowAsync(
            _dbSet,
            predicate,
            ct);

        updateAction(entity);
    }
    public async Task UpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        var entity = await FirstOrThrowAsync(
            _dbSet,
            predicate,
            ct);

        await updateAction(entity);
    }
    public async Task UpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Action<TEntity> updateAction,
        CancellationToken ct = default)
    {
        var entity = await FirstOrThrowAsync(
            ApplyIncludes(includes),
            predicate,
            ct);

        updateAction(entity);
    }
    public async Task UpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        var entity = await FirstOrThrowAsync(
            ApplyIncludes(includes),
            predicate,
            ct);

        await updateAction(entity);
    }

    #endregion

    #region Delete

    public async Task DeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        var entity = await FirstOrThrowAsync(
            _dbSet,
            predicate,
            ct);

        _dbSet.Remove(entity);
    }

    public async Task<int> DeleteWithNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Where(predicate)
            .ExecuteDeleteAsync(ct);
    }

    #endregion
    #region Protected Helpers

    /// <summary>
    /// Applies include chain when provided.
    /// </summary>
    protected virtual IQueryable<TEntity> ApplyIncludes(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includes)
    {
        if (includes is null)
            return _dbSet;

        return includes(_dbSet);
    }

    /// <summary>
    /// Returns the first matched entity or throws a NotFoundException.
    /// </summary>
    protected static async Task<TEntity> FirstOrThrowAsync(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
    {
        return await query.FirstOrDefaultAsync(predicate, ct)
            ?? throw new NotFoundException(
                $"{typeof(TEntity).Name} was not found.");
    }

    #endregion
}