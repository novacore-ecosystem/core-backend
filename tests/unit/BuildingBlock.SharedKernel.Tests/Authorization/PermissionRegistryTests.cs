using NovaCore.BuildingBlock.SharedKernel.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Shouldly;

namespace NovaCore.BuildingBlock.SharedKernel.Tests.Authorization;

public class PermissionRegistryTests
{
    // Real catalog (Permissions.cs) - exercises PermissionRegistry.Instance, the lazy static
    // singleton every consumer (PermissionKey, DbMigrator sync, grant validation) actually uses.

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
    public void Instance_GetAll_IsNonEmptyAndIncludesEveryModule()
    {
        var keys = PermissionRegistry.Instance.GetAll().Select(d => d.Key).ToHashSet(StringComparer.Ordinal);

        keys.ShouldContain(Permissions.Root);
        keys.ShouldContain(Permissions.Product.Full);
        keys.ShouldContain(Permissions.Tenant.Manage);
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

    // Fixture-based discovery - no DB/reflection-target dependency beyond a plain type, verifying
    // Discover() itself rather than the real catalog's current contents.

    [Fact]
    public void Discover_FindsAttributedConstsAcrossNestedTypes()
    {
        var registry = PermissionRegistry.Discover(typeof(FixtureCatalog));

        registry.Contains(FixtureCatalog.TopLevel).ShouldBeTrue();
        registry.Contains(FixtureCatalog.Nested.Inner).ShouldBeTrue();
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
        registry.GetAllowedProviders(FixtureCatalog.Nested.Inner)
            .ShouldBe(PermissionProviderName.Role | PermissionProviderName.User);
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

        public static class Nested
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
