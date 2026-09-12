using NovaCore.Auth.Application.Abstractions.Authorization;

using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class AccountAuthorizationGuardTests
{
    private static AccountAuthorizationSnapshot Snapshot(int level, params string[] permissionKeys)
        => new(level, permissionKeys.ToHashSet());

    [Fact]
    public void EnsureCanManageAccount_ActorOutranksTarget_DoesNotThrow()
    {
        var actor = Snapshot(50);
        var target = Snapshot(10);

        Should.NotThrow(() => AccountAuthorizationGuard.EnsureCanManageAccount(actor, target));
    }

    [Fact]
    public void EnsureCanManageAccount_ActorDoesNotOutrankTarget_Throws()
    {
        var actor = Snapshot(10);
        var target = Snapshot(10);

        Should.Throw<ForbiddenException>(() => AccountAuthorizationGuard.EnsureCanManageAccount(actor, target));
    }

    [Fact]
    public void EnsureCanManageAccount_TargetHoldsRoot_Throws_EvenIfActorOutranksOnLevel()
    {
        var actor = Snapshot(int.MaxValue);
        var target = Snapshot(0, Permissions.Root);

        Should.Throw<ForbiddenException>(() => AccountAuthorizationGuard.EnsureCanManageAccount(actor, target));
    }

    [Fact]
    public void EnsureCanManageAccount_ActorHoldsRoot_BypassesLevelComparison()
    {
        var actor = Snapshot(0, Permissions.Root);
        var target = Snapshot(int.MaxValue);

        Should.NotThrow(() => AccountAuthorizationGuard.EnsureCanManageAccount(actor, target));
    }

    [Fact]
    public void EnsureCanGrantPermissions_ActorHoldsEveryKey_DoesNotThrow()
    {
        var actor = Snapshot(0, "order:view", "order:manage");

        Should.NotThrow(() => AccountAuthorizationGuard.EnsureCanGrantPermissions(actor, ["order:manage"]));
    }

    [Fact]
    public void EnsureCanGrantPermissions_ActorMissingAKey_Throws()
    {
        var actor = Snapshot(0, "order:view");

        Should.Throw<ForbiddenException>(() => AccountAuthorizationGuard.EnsureCanGrantPermissions(actor, ["order:manage"]));
    }

    [Fact]
    public void EnsureCanGrantPermissions_ActorHoldsRoot_BypassesOwnershipCheck()
    {
        var actor = Snapshot(0, Permissions.Root);

        Should.NotThrow(() => AccountAuthorizationGuard.EnsureCanGrantPermissions(actor, ["order:manage"]));
    }

    [Fact]
    public void EnsureCanGrantRole_ActorHoldsTheRole_DoesNotThrow()
    {
        var actor = Snapshot(0);

        Should.NotThrow(() => AccountAuthorizationGuard.EnsureCanGrantRole(actor, actorHoldsRole: true));
    }

    [Fact]
    public void EnsureCanGrantRole_ActorDoesNotHoldTheRole_Throws()
    {
        var actor = Snapshot(0);

        Should.Throw<ForbiddenException>(() => AccountAuthorizationGuard.EnsureCanGrantRole(actor, actorHoldsRole: false));
    }

    [Fact]
    public void EnsureCanGrantRole_ActorHoldsRoot_BypassesOwnershipCheck()
    {
        var actor = Snapshot(0, Permissions.Root);

        Should.NotThrow(() => AccountAuthorizationGuard.EnsureCanGrantRole(actor, actorHoldsRole: false));
    }

    [Fact]
    public void EnsureTargetDoesNotGrantRoot_TargetGrantsRoot_NonRootActorThrows()
    {
        var actor = Snapshot(int.MaxValue);
        var targetKeys = new HashSet<string> { Permissions.Root };

        Should.Throw<ForbiddenException>(() => AccountAuthorizationGuard.EnsureTargetDoesNotGrantRoot(actor, targetKeys));
    }

    [Fact]
    public void EnsureTargetDoesNotGrantRoot_ActorHoldsRoot_Bypasses()
    {
        var actor = Snapshot(0, Permissions.Root);
        var targetKeys = new HashSet<string> { Permissions.Root };

        Should.NotThrow(() => AccountAuthorizationGuard.EnsureTargetDoesNotGrantRoot(actor, targetKeys));
    }

    [Fact]
    public void EnsureTargetDoesNotGrantRoot_TargetDoesNotGrantRoot_DoesNotThrow()
    {
        var actor = Snapshot(0);
        var targetKeys = new HashSet<string> { "order:view" };

        Should.NotThrow(() => AccountAuthorizationGuard.EnsureTargetDoesNotGrantRoot(actor, targetKeys));
    }

    [Fact]
    public void EnsureWithinTenantBoundary_BoundaryDisabled_UnrestrictedRegardlessOfAllowedKeys()
    {
        var actor = Snapshot(0);

        Should.NotThrow(() => AccountAuthorizationGuard.EnsureWithinTenantBoundary(
            actor, boundaryEnabled: false, tenantAllowedKeys: new HashSet<string>(), newlyAddedKeys: ["order:manage"]));
    }

    [Fact]
    public void EnsureWithinTenantBoundary_BoundaryEnabled_KeyOutsideAllowedSet_Throws()
    {
        var actor = Snapshot(0);
        var allowed = new HashSet<string> { "order:view" };

        Should.Throw<ForbiddenException>(() => AccountAuthorizationGuard.EnsureWithinTenantBoundary(
            actor, boundaryEnabled: true, tenantAllowedKeys: allowed, newlyAddedKeys: ["order:manage"]));
    }

    [Fact]
    public void EnsureWithinTenantBoundary_BoundaryEnabled_EveryKeyAllowed_DoesNotThrow()
    {
        var actor = Snapshot(0);
        var allowed = new HashSet<string> { "order:view", "order:manage" };

        Should.NotThrow(() => AccountAuthorizationGuard.EnsureWithinTenantBoundary(
            actor, boundaryEnabled: true, tenantAllowedKeys: allowed, newlyAddedKeys: ["order:manage"]));
    }

    [Fact]
    public void EnsureWithinTenantBoundary_ActorHoldsRoot_BypassesBoundary()
    {
        var actor = Snapshot(0, Permissions.Root);

        Should.NotThrow(() => AccountAuthorizationGuard.EnsureWithinTenantBoundary(
            actor, boundaryEnabled: true, tenantAllowedKeys: new HashSet<string>(), newlyAddedKeys: ["order:manage"]));
    }
}
