using NovaCore.BuildingBlock.Application.Abstractions.Authorization;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.BuildingBlock.Application.Authorization;

/// <summary>
/// Default <see cref="IAuthorizationGuard"/> - resolves the current actor's permission set once
/// per call via <see cref="ICurrentUserService"/> and evaluates the expression tree in memory.
/// </summary>
public sealed class AuthorizationGuard(ICurrentUserService currentUserService) : IAuthorizationGuard
{
    public Task RequirePermissionsAsync(PermissionExpression expression, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (!IsSatisfied(expression))
            throw new ForbiddenException("The current actor does not satisfy the required permission(s).");

        return Task.CompletedTask;
    }

    public Task RequirePermissionsAsync(params PermissionExpression[] expressions) =>
        RequirePermissionsAsync(PermissionExpression.All(expressions));

    public bool HasPermissions(params PermissionExpression[] expressions) =>
        IsSatisfied(PermissionExpression.All(expressions));

    private bool IsSatisfied(PermissionExpression expression) =>
        expression.IsSatisfiedBy(currentUserService.GetPermissions());
}
