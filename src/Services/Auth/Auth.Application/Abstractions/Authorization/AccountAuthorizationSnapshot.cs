using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.Application.Abstractions.Authorization;

/// <summary>
/// An Account's resolved authorization state at one point in time.
/// </summary>
/// <remarks>
/// Backed by <see cref="IEffectiveAuthorizationCache"/> - a single cached unit reused for every
/// backend authorization decision (scope checks, grant validation) instead of resolving Level and
/// effective permissions separately on every check.
/// </remarks>
public sealed record AccountAuthorizationSnapshot(int Level, IReadOnlySet<string> PermissionKeys)
{
    /// <summary>Whether this Account holds the platform Root bypass.</summary>
    public bool HasRoot => PermissionKeys.Contains(Permissions.Root);
}
