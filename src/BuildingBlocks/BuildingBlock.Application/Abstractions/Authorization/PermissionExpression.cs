using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.BuildingBlock.Application.Abstractions.Authorization;

/// <summary>
/// A composable permission requirement - a single permission key, or a logical AND/OR of nested
/// expressions - evaluated by <see cref="IAuthorizationGuard"/> against an actor's resolved
/// permission set.
/// </summary>
/// <remarks>
/// A bare permission key converts implicitly, so the common case never touches this type directly:
/// <c>RequirePermissionsAsync(Permissions.Product.View)</c>. <see cref="All"/>/<see cref="Or"/>
/// nest arbitrarily for the uncommon cross-permission cases - import them with
/// <c>using static PermissionExpression;</c> to write <c>Or(PermA, All(PermB, PermC))</c> instead
/// of the fully-qualified form.
/// </remarks>
public abstract record PermissionExpression
{
    public static implicit operator PermissionExpression(string permissionKey) => new Requirement(permissionKey);

    /// <summary>Every expression must be satisfied (logical AND).</summary>
    public static PermissionExpression All(params PermissionExpression[] expressions) =>
        new AllOf(RequireNonEmpty(expressions, nameof(All)));

    /// <summary>At least one expression must be satisfied (logical OR).</summary>
    public static PermissionExpression Or(params PermissionExpression[] expressions) =>
        new AnyOf(RequireNonEmpty(expressions, nameof(Or)));

    internal abstract bool IsSatisfiedBy(IReadOnlySet<string> ownedPermissions);

    /// <summary>
    /// Whether <paramref name="ownedPermissions"/> grants <paramref name="permissionKey"/> -
    /// exactly, via <see cref="Permissions.Root"/>, or via that permission's module aggregate
    /// ("{module}:full").
    /// </summary>
    public static bool IsGranted(IReadOnlySet<string> ownedPermissions, string permissionKey)
    {
        if (ownedPermissions.Contains(Permissions.Root) || ownedPermissions.Contains(permissionKey))
            return true;

        var separatorIndex = permissionKey.IndexOf(':');
        return separatorIndex > 0 && ownedPermissions.Contains($"{permissionKey[..separatorIndex]}:full");
    }

    private static IReadOnlyList<PermissionExpression> RequireNonEmpty(PermissionExpression[] expressions, string factoryName)
    {
        if (expressions is null || expressions.Length == 0)
            throw new ArgumentException($"{factoryName}(...) requires at least one expression.", nameof(expressions));

        foreach (var expression in expressions)
            ArgumentNullException.ThrowIfNull(expression, nameof(expressions));

        return expressions;
    }

    private sealed record Requirement : PermissionExpression
    {
        private readonly string _key;

        public Requirement(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("A permission key must not be null or empty.", nameof(key));

            _key = key;
        }

        internal override bool IsSatisfiedBy(IReadOnlySet<string> ownedPermissions) => IsGranted(ownedPermissions, _key);
    }

    private sealed record AllOf(IReadOnlyList<PermissionExpression> Expressions) : PermissionExpression
    {
        internal override bool IsSatisfiedBy(IReadOnlySet<string> ownedPermissions) =>
            Expressions.All(expression => expression.IsSatisfiedBy(ownedPermissions));
    }

    private sealed record AnyOf(IReadOnlyList<PermissionExpression> Expressions) : PermissionExpression
    {
        internal override bool IsSatisfiedBy(IReadOnlySet<string> ownedPermissions) =>
            Expressions.Any(expression => expression.IsSatisfiedBy(ownedPermissions));
    }
}
