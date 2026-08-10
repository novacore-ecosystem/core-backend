using System.Linq.Expressions;

using NovaCore.BuildingBlock.Domain.Abstractions;

namespace NovaCore.BuildingBlock.Persistence.Repository;

public interface IRepository<TEntity>
    where TEntity : IEntity
{
    #region Get
    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);
    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        CancellationToken ct = default);
    Task<TEntity?> GetAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        TValue value,
        CancellationToken ct = default);
    Task<TEntity?> GetAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        TValue value,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        CancellationToken ct = default);

    Task<TEntity[]> GetManyAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        IEnumerable<TValue> values,
        CancellationToken ct = default);
    Task<TEntity[]> GetManyAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        IEnumerable<TValue> values,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        CancellationToken ct = default);

    #endregion

    #region Exists

    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);
    Task<bool> ExistsAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        TValue value,
        CancellationToken ct = default);

    Task<HashSet<TValue>> GetExistingValuesAsync<TValue>(
        Expression<Func<TEntity, TValue>> selector,
        IEnumerable<TValue> values,
        CancellationToken ct = default);

    #endregion

    #region Aggregate

    Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);

    #endregion

    #region Write

    Task AddAsync(
        TEntity entity,
        CancellationToken ct = default);
    Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken ct = default);

    #endregion

    #region Update

    Task UpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<TEntity> updateAction,
        CancellationToken ct = default);
    Task UpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default);
    Task UpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Action<TEntity> updateAction,
        CancellationToken ct = default);
    Task UpdateAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default);

    #endregion

    #region Delete

    Task DeleteAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);

    Task<int> DeleteWithNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default);

    #endregion
}

public interface IRepository<TEntity, TId>
    : IRepository<TEntity> where TEntity : IEntity<TId>
{
    #region Get

    Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken ct = default);
    Task<TEntity?> GetByIdAsync(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        CancellationToken ct = default);

    Task<List<TEntity>> GetManyByIdsAsync(
        IEnumerable<TId> ids,
        CancellationToken ct = default);
    Task<List<TEntity>> GetManyByIdsAsync(
        IEnumerable<TId> ids,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        CancellationToken ct = default);

    #endregion

    #region Exists

    Task<bool> ExistsByIdAsync(
        TId id,
        CancellationToken ct = default);
    Task<HashSet<TId>> GetExistingIdsAsync(
        IEnumerable<TId> ids,
        CancellationToken ct = default);

    #endregion

    #region Update

    Task UpdateAsync(
        TId id,
        Action<TEntity> updateAction,
        CancellationToken ct = default);
    Task UpdateAsync(
        TId id,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default);
    Task UpdateAsync(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Action<TEntity> updateAction,
        CancellationToken ct = default);
    Task UpdateAsync(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default);

    #endregion

    #region Delete

    Task DeleteByIdAsync(
        TId id,
        CancellationToken ct = default);

    Task DeleteRangeAsync(
        IEnumerable<TId> ids,
        CancellationToken ct = default);

    #endregion
}
