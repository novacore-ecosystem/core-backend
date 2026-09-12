using static NovaCore.BuildingBlock.Application.Abstractions.Authorization.PermissionExpression;

using NovaCore.BuildingBlock.Application.Abstractions.Authorization;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Authorization;
using NovaCore.BuildingBlock.Application.Exceptions;

using NSubstitute;

using Shouldly;

namespace NovaCore.BuildingBlock.Application.Tests.Authorization;

public sealed class AuthorizationGuardTests
{
    private const string PermA = "product:view";
    private const string PermB = "product:manage";
    private const string PermC = "order:view";
    private const string PermD = "order:manage";

    private static IAuthorizationGuard CreateGuard(params string[] ownedPermissions)
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.GetPermissions().Returns(new HashSet<string>(ownedPermissions, StringComparer.Ordinal));
        return new AuthorizationGuard(currentUser);
    }

    [Fact]
    public async Task RequirePermissionsAsync_SinglePermissionOwned_Succeeds()
    {
        var guard = CreateGuard(PermA);

        await guard.RequirePermissionsAsync(PermA);
    }

    [Fact]
    public async Task RequirePermissionsAsync_SinglePermissionMissing_ThrowsForbidden()
    {
        var guard = CreateGuard();

        await Should.ThrowAsync<ForbiddenException>(() => guard.RequirePermissionsAsync(PermA));
    }

    [Fact]
    public async Task RequirePermissionsAsync_MultipleArguments_RequiresAll()
    {
        var guard = CreateGuard(PermA);

        await Should.ThrowAsync<ForbiddenException>(() => guard.RequirePermissionsAsync(PermA, PermB));
    }

    [Fact]
    public async Task RequirePermissionsAsync_MultipleArguments_AllOwned_Succeeds()
    {
        var guard = CreateGuard(PermA, PermB);

        await guard.RequirePermissionsAsync(PermA, PermB);
    }

    [Fact]
    public async Task RequirePermissionsAsync_OrExpression_AnyOneOwned_Succeeds()
    {
        var guard = CreateGuard(PermA, PermC);

        await guard.RequirePermissionsAsync(PermA, Or(PermB, PermC, PermD));
    }

    [Fact]
    public async Task RequirePermissionsAsync_OrExpression_NoneOwned_ThrowsForbidden()
    {
        var guard = CreateGuard(PermA);

        await Should.ThrowAsync<ForbiddenException>(() => guard.RequirePermissionsAsync(PermA, Or(PermB, PermC, PermD)));
    }

    [Fact]
    public async Task RequirePermissionsAsync_NestedOrOfAll_MatchesFirstBranch_Succeeds()
    {
        // (PermA AND PermB) OR (PermA AND PermC) - only the first branch is owned.
        var guard = CreateGuard(PermA, PermB);

        await guard.RequirePermissionsAsync(Or(All(PermA, PermB), All(PermA, PermC)));
    }

    [Fact]
    public async Task RequirePermissionsAsync_NestedOrOfAll_NeitherBranchFullySatisfied_ThrowsForbidden()
    {
        // Owns PermB and PermC individually - a flattened "OR of every leaf" would wrongly
        // succeed here, but neither AND-branch is fully satisfied without PermA.
        var guard = CreateGuard(PermB, PermC);

        await Should.ThrowAsync<ForbiddenException>(
            () => guard.RequirePermissionsAsync(Or(All(PermA, PermB), All(PermA, PermC))));
    }

    [Fact]
    public async Task RequirePermissionsAsync_RootOwned_BypassesEveryRequirement()
    {
        var guard = CreateGuard("system:root");

        await guard.RequirePermissionsAsync(All(PermA, PermB), Or(PermC, PermD));
    }

    [Fact]
    public void HasPermissions_DoesNotThrow_ReturnsBooleanInstead()
    {
        var guard = CreateGuard(PermA);

        guard.HasPermissions(PermA).ShouldBeTrue();
        guard.HasPermissions(PermB).ShouldBeFalse();
    }
}
