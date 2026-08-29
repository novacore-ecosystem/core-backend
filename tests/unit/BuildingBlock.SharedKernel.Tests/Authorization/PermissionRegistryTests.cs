using NovaCore.BuildingBlock.SharedKernel.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Shouldly;

namespace NovaCore.BuildingBlock.SharedKernel.Tests.Authorization;

public class PermissionRegistryTests
{
    // Real catalog (Permissions.*.cs, split across per-owner files but one partial class) -
    // exercises PermissionRegistry.Instance, the lazy static singleton every consumer
    // (PermissionKey, DbMigrator sync, grant validation) actually uses.

    [Fact]
    public void Instance_ContainsRootAndUser()
    {
        PermissionRegistry.Instance.Contains(Permissions.Root).ShouldBeTrue();
        PermissionRegistry.Instance.Contains(Permissions.User).ShouldBeTrue();
    }

    [Fact]
    public void Instance_UnknownKey_NotContained()
    {
        PermissionRegistry.Instance.Contains("nonexistent:key").ShouldBeFalse();
    }

    [Fact]
    public void Instance_Get_ReturnsDefinitionForKnownKey()
    {
        var definition = PermissionRegistry.Instance.Get(Permissions.Role.View);

        definition.ShouldNotBeNull();
        definition.Key.ShouldBe(Permissions.Role.View);
    }

    [Fact]
    public void Instance_Get_ReturnsNullForUnknownKey()
    {
        PermissionRegistry.Instance.Get("nonexistent:key").ShouldBeNull();
    }

    [Fact]
    public void Instance_GetAll_IsNonEmptyAndIncludesEveryOwnerFile()
    {
        // One representative key per Permissions.<Owner>.cs file - proves discovery sees the whole
        // partial class regardless of which physical file declared a given const.
        var keys = PermissionRegistry.Instance.GetAll().Select(d => d.Key).ToHashSet(StringComparer.Ordinal);

        keys.ShouldContain(Permissions.Root);              // Permissions.Common.cs
        keys.ShouldContain(Permissions.Tenant.Manage);      // Permissions.Auth.cs
        keys.ShouldContain(Permissions.Product.Full);       // Permissions.Product.cs
        keys.ShouldContain(Permissions.Inventory.View);     // Permissions.Inventory.cs
        keys.ShouldContain(Permissions.Warehouse.Manage);   // Permissions.Inventory.cs (separate group, same file)
        keys.ShouldContain(Permissions.Order.Fulfill);      // Permissions.Order.cs
        keys.ShouldContain(Permissions.Audit.View);         // Permissions.Audit.cs
        keys.ShouldContain(Permissions.Notification.Send);  // Permissions.Notification.cs
        keys.ShouldContain(Permissions.Users.Manage);       // Permissions.User.cs
    }

    [Fact]
    public void Instance_EveryDefinition_DefaultsToRoleAllowedProvider()
    {
        foreach (var definition in PermissionRegistry.Instance.GetAll())
            definition.AllowedProviders.ShouldBe(PermissionProviderName.Role);
    }

    [Fact]
    public void Instance_IsProviderAllowed_TrueForRoleFalseForGuest()
    {
        PermissionRegistry.Instance.IsProviderAllowed(Permissions.Product.Manage, PermissionProviderName.Role).ShouldBeTrue();
        PermissionRegistry.Instance.IsProviderAllowed(Permissions.Product.Manage, PermissionProviderName.Guest).ShouldBeFalse();
    }

    [Fact]
    public void Instance_GetAllowedProviders_UnknownKey_ReturnsNone()
    {
        PermissionRegistry.Instance.GetAllowedProviders("nonexistent:key").ShouldBe(PermissionProviderName.None);
    }

    [Fact]
    public void Instance_RootAndUser_AreUngrouped()
    {
        PermissionRegistry.Instance.Get(Permissions.Root)!.GroupCode.ShouldBeNull();
        PermissionRegistry.Instance.Get(Permissions.User)!.GroupCode.ShouldBeNull();
    }

    [Fact]
    public void Instance_GetGroup_ReturnsEveryMemberOfThatGroup()
    {
        var group = PermissionRegistry.Instance.GetGroup("product");

        group.ShouldNotBeNull();
        group.Code.ShouldBe("product");
        group.PermissionKeys.ShouldBe([Permissions.Product.Manage, Permissions.Product.Reindex, Permissions.Product.Full], ignoreOrder: true);
    }

    [Fact]
    public void Instance_InventoryAndWarehouse_AreSeparateGroupsDespiteSameFile()
    {
        // File organization != permission group organization (both live in Permissions.Inventory.cs).
        var inventory = PermissionRegistry.Instance.GetGroup("inventory");
        var warehouse = PermissionRegistry.Instance.GetGroup("warehouse");

        inventory.ShouldNotBeNull();
        warehouse.ShouldNotBeNull();
        inventory.PermissionKeys.ShouldNotContain(Permissions.Warehouse.Manage);
        warehouse.PermissionKeys.ShouldNotContain(Permissions.Inventory.View);
    }

    [Fact]
    public void Instance_GetPermissions_MatchesGetGroupPermissionKeys()
    {
        PermissionRegistry.Instance.GetPermissions("order").ShouldBe(PermissionRegistry.Instance.GetGroup("order")!.PermissionKeys);
    }

    [Fact]
    public void Instance_GetGroup_UnknownCode_ReturnsNull()
    {
        PermissionRegistry.Instance.GetGroup("nonexistent-group").ShouldBeNull();
    }

    [Fact]
    public void Instance_GetPermissions_UnknownCode_ReturnsEmpty()
    {
        PermissionRegistry.Instance.GetPermissions("nonexistent-group").ShouldBeEmpty();
    }

    [Fact]
    public void Instance_GetGroups_IsNonEmptyAndEveryGroupHasAtLeastOnePermission()
    {
        var groups = PermissionRegistry.Instance.GetGroups();

        groups.ShouldNotBeEmpty();
        foreach (var group in groups)
            group.PermissionKeys.ShouldNotBeEmpty();
    }

    // Fixture-based discovery - no DB/reflection-target dependency beyond a plain type, verifying
    // Discover() itself rather than the real catalog's current contents.

    [Fact]
    public void Discover_FindsAttributedConstsAcrossNestedTypes()
    {
        var registry = PermissionRegistry.Discover(typeof(FixtureCatalog));

        registry.Contains(FixtureCatalog.TopLevel).ShouldBeTrue();
        registry.Contains(FixtureCatalog.Grouped.Inner).ShouldBeTrue();
    }

    [Fact]
    public void Discover_IgnoresConstsWithoutTheAttribute()
    {
        var registry = PermissionRegistry.Discover(typeof(FixtureCatalog));

        registry.Contains(FixtureCatalog.Undeclared).ShouldBeFalse();
    }

    [Fact]
    public void Discover_PreservesEachConstsAllowedProviders()
    {
        var registry = PermissionRegistry.Discover(typeof(FixtureCatalog));

        registry.GetAllowedProviders(FixtureCatalog.TopLevel).ShouldBe(PermissionProviderName.Role);
        registry.GetAllowedProviders(FixtureCatalog.Grouped.Inner)
            .ShouldBe(PermissionProviderName.Role | PermissionProviderName.User);
    }

    [Fact]
    public void Discover_TopLevelConst_NotNestedUnderAnyGroup_IsUngrouped()
    {
        var registry = PermissionRegistry.Discover(typeof(FixtureCatalog));

        registry.Get(FixtureCatalog.TopLevel)!.GroupCode.ShouldBeNull();
        registry.GetGroups().ShouldAllBe(g => !g.PermissionKeys.Contains(FixtureCatalog.TopLevel));
    }

    [Fact]
    public void Discover_ClassWithPermissionGroupAttribute_GroupsItsConsts()
    {
        var registry = PermissionRegistry.Discover(typeof(FixtureCatalog));

        registry.Get(FixtureCatalog.Grouped.Inner)!.GroupCode.ShouldBe("fixture-group");
        registry.GetGroup("fixture-group")!.PermissionKeys.ShouldContain(FixtureCatalog.Grouped.Inner);
    }

    [Fact]
    public void Discover_DuplicateKeyAcrossFields_Throws()
    {
        Should.Throw<InvalidOperationException>(() => PermissionRegistry.Discover(typeof(FixtureCatalogWithDuplicate)));
    }

    private static class FixtureCatalog
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string TopLevel = "fixture:top-level";

        public const string Undeclared = "fixture:undeclared";

        [PermissionGroup("fixture-group")]
        public static class Grouped
        {
            [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
            public const string Inner = "fixture:inner";
        }
    }

    private static class FixtureCatalogWithDuplicate
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string First = "fixture:duplicate";

        [PermissionDefinition(Providers = PermissionProviderName.User)]
        public const string Second = "fixture:duplicate";
    }
}
