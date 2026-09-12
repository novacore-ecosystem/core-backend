namespace NovaCore.BuildingBlock.Application.Abstractions.Authorization;

/// <summary>
/// Application-layer authorization entry point: evaluates a required <see cref="PermissionExpression"/>
/// against the current actor and throws when unsatisfied, so callers never hand-roll a
/// check-then-throw around a permission lookup.
/// </summary>
public interface IAuthorizationGuard
{
    /// <summary>
    /// Ensures the current actor satisfies <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The required permission expression.</param>
    Task RequirePermissionsAsync(PermissionExpression expression, CancellationToken ct = default);

    /// <summary>
    /// Ensures the current actor satisfies every given expression (logical AND).
    /// </summary>
    /// <param name="expressions">The required permission expressions.</param>
    Task RequirePermissionsAsync(params PermissionExpression[] expressions);

    /// <summary>
    /// Non-throwing check of whether the current actor satisfies every given expression.
    /// </summary>
    /// <param name="expressions">The permission expressions to check.</param>
    bool HasPermissions(params PermissionExpression[] expressions);
}
