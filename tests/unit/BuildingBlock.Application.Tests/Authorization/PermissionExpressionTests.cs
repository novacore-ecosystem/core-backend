using NovaCore.BuildingBlock.Application.Abstractions.Authorization;

using Shouldly;

namespace NovaCore.BuildingBlock.Application.Tests.Authorization;

public sealed class PermissionExpressionTests
{
    [Fact]
    public void IsGranted_ExactMatch_ReturnsTrue()
    {
        var owned = new HashSet<string> { "product:view" };

        PermissionExpression.IsGranted(owned, "product:view").ShouldBeTrue();
    }

    [Fact]
    public void IsGranted_NoMatch_ReturnsFalse()
    {
        var owned = new HashSet<string> { "product:view" };

        PermissionExpression.IsGranted(owned, "product:manage").ShouldBeFalse();
    }

    [Fact]
    public void IsGranted_RootOwned_BypassesEverything()
    {
        var owned = new HashSet<string> { "system:root" };

        PermissionExpression.IsGranted(owned, "anything:at-all").ShouldBeTrue();
    }

    [Fact]
    public void IsGranted_ModuleAggregateOwned_GrantsEveryKeyInThatModule()
    {
        var owned = new HashSet<string> { "product:full" };

        PermissionExpression.IsGranted(owned, "product:view").ShouldBeTrue();
    }

    [Fact]
    public void IsGranted_AggregateFromDifferentModule_DoesNotGrant()
    {
        var owned = new HashSet<string> { "order:full" };

        PermissionExpression.IsGranted(owned, "product:view").ShouldBeFalse();
    }

    [Fact]
    public void All_NoExpressions_Throws()
    {
        Should.Throw<ArgumentException>(() => PermissionExpression.All());
    }

    [Fact]
    public void Or_NoExpressions_Throws()
    {
        Should.Throw<ArgumentException>(() => PermissionExpression.Or());
    }

    [Fact]
    public void All_ContainingNull_Throws()
    {
        Should.Throw<ArgumentNullException>(() => PermissionExpression.All("product:view", null!));
    }

    [Fact]
    public void ImplicitConversion_EmptyKey_Throws()
    {
        Should.Throw<ArgumentException>(() => _ = (PermissionExpression)string.Empty);
    }

    [Fact]
    public void ImplicitConversion_WhitespaceKey_Throws()
    {
        Should.Throw<ArgumentException>(() => _ = (PermissionExpression)"   ");
    }
}
