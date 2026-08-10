using System.Linq.Expressions;

using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Building;
using NovaCore.BuildingBlock.Criteria.Definition;
using NovaCore.BuildingBlock.Criteria.Requests;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.BuildingBlock.Persistence.Ef.Criteria;

/// <summary>
/// The EF-specific half of the Criteria pipeline: Request -> Validation -> Definition -> (this) EF Criteria Builder.
/// Applies the provider-agnostic predicate from <see cref="CriteriaPredicateBuilder{TEntity}"/>, then dynamic
/// sorting and paging. A future Mongo/Elastic/Dapper builder would live beside its own persistence project,
/// consuming the same <see cref="CriteriaDefinition{TEntity}"/>/<see cref="CriteriaRequest"/> - nothing here changes.
/// </summary>
public static class CriteriaQueryableExtensions
{
    public static IQueryable<TEntity> ApplyCriteria<TEntity>(
        this IQueryable<TEntity> query,
        CriteriaDefinition<TEntity> definition,
        CriteriaRequest request)
    {
        var predicate = CriteriaPredicateBuilder<TEntity>.Build(definition, request);
        if (predicate is not null)
            query = query.Where(predicate);

        return ApplySorting(query, definition, request.Sorts);
    }

    public static async Task<PaginatedResult<TEntity>> ToCriteriaPagedResultAsync<TEntity>(
        this IQueryable<TEntity> query,
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return PaginatedResult<TEntity>.Create(items, request.Page, request.PageSize, totalCount);
    }

    private static IQueryable<TEntity> ApplySorting<TEntity>(
        IQueryable<TEntity> query,
        CriteriaDefinition<TEntity> definition,
        IReadOnlyList<CriteriaSort> sorts)
    {
        var isFirst = true;
        foreach (var sort in sorts)
        {
            if (!definition.TryGetField(sort.Field, out var field) || !field.Sortable)
                continue;

            query = ApplyOrderBy(query, field.Accessor, sort.Direction, isFirst);
            isFirst = false;
        }

        return query;
    }

    private static IQueryable<TEntity> ApplyOrderBy<TEntity>(
        IQueryable<TEntity> query,
        LambdaExpression accessor,
        SortDirection direction,
        bool isFirst)
    {
        var methodName = (isFirst, direction) switch
        {
            (true, SortDirection.Asc) => nameof(Queryable.OrderBy),
            (true, SortDirection.Desc) => nameof(Queryable.OrderByDescending),
            (false, SortDirection.Asc) => nameof(Queryable.ThenBy),
            (false, SortDirection.Desc) => nameof(Queryable.ThenByDescending),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

        var method = typeof(Queryable).GetMethods()
            .Single(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TEntity), accessor.ReturnType);

        return (IQueryable<TEntity>)method.Invoke(null, [query, accessor])!;
    }
}
