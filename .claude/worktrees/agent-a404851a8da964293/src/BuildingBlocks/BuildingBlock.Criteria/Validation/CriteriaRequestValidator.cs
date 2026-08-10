using System.Text.Json;

using NovaCore.BuildingBlock.Criteria.Definition;
using NovaCore.BuildingBlock.Criteria.Enums;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.BuildingBlock.Criteria.Validation;

/// <summary>
/// Validates a <see cref="CriteriaRequest"/> against a <see cref="CriteriaDefinition{TEntity}"/> before any query is
/// built: unknown fields, operators not on that field's whitelist, and value shapes that don't match the operator.
/// Returns plain strings rather than throwing, so it stays dependency-free - consuming services fold these into
/// their own FluentValidation validator (see e.g. `SearchUsersValidator`), reusing the existing ValidationBehavior
/// pipeline instead of introducing a second error-reporting path.
/// </summary>
public static class CriteriaRequestValidator<TEntity>
{
    private const int MaxPageSize = 200;

    public static IReadOnlyList<string> Validate(CriteriaDefinition<TEntity> definition, CriteriaRequest request)
    {
        var errors = new List<string>();

        if (request.Page < 1)
            errors.Add("Page must be greater than or equal to 1.");

        if (request.PageSize is < 1 or > MaxPageSize)
            errors.Add($"PageSize must be between 1 and {MaxPageSize}.");

        foreach (var filter in request.Filters)
            ValidateFilter(definition, filter, errors);

        foreach (var sort in request.Sorts)
            ValidateSort(definition, sort, errors);

        return errors;
    }

    private static void ValidateFilter(CriteriaDefinition<TEntity> definition, CriteriaFilter filter, List<string> errors)
    {
        if (!definition.TryGetField(filter.Field, out var field))
        {
            errors.Add($"Unknown field '{filter.Field}'.");
            return;
        }

        if (!field.AllowedOperators.Contains(filter.Operator))
        {
            errors.Add($"Operator '{filter.Operator}' is not allowed for field '{filter.Field}'.");
            return;
        }

        ValidateValueShape(filter, errors);
    }

    private static void ValidateValueShape(CriteriaFilter filter, List<string> errors)
    {
        switch (filter.Operator)
        {
            case CriteriaOperator.IsNull:
            case CriteriaOperator.IsNotNull:
                return;

            case CriteriaOperator.In:
            case CriteriaOperator.NotIn:
                if (filter.Value is not { ValueKind: JsonValueKind.Array } array || array.GetArrayLength() == 0)
                    errors.Add($"Operator '{filter.Operator}' on field '{filter.Field}' requires a non-empty array value.");
                return;

            case CriteriaOperator.Between:
                if (filter.Value is not { ValueKind: JsonValueKind.Array } range || range.GetArrayLength() != 2)
                    errors.Add($"Operator 'between' on field '{filter.Field}' requires an array of exactly 2 values.");
                return;

            default:
                if (filter.Value is null || filter.Value.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                    errors.Add($"Field '{filter.Field}' requires a value for operator '{filter.Operator}'.");
                return;
        }
    }

    private static void ValidateSort(CriteriaDefinition<TEntity> definition, CriteriaSort sort, List<string> errors)
    {
        if (!definition.TryGetField(sort.Field, out var field))
        {
            errors.Add($"Unknown sort field '{sort.Field}'.");
            return;
        }

        if (!field.Sortable)
            errors.Add($"Field '{sort.Field}' is not sortable.");
    }
}
