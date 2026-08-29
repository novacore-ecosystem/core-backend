using NovaCore.Auth.Domain.Entities.Permissions;

using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

using NovaCore.TestKit.ShouldlyExtensions;

using Shouldly;

namespace NovaCore.Auth.Domain.Tests.Entities;

public class PermissionGrantTests
{
    [Fact]
    public void Create_ValidRoleGrant_Succeeds()
    {
        var permissionDefinitionId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var grant = PermissionGrant.Create(permissionDefinitionId, PermissionProviderName.Role, roleId.ToString());

        grant.PermissionDefinitionId.ShouldBe(permissionDefinitionId);
        grant.ProviderName.ShouldBe(PermissionProviderName.Role);
        grant.ProviderKey.ShouldBe(roleId.ToString());
    }

    [Fact]
    public void Create_DoesNotKnowAboutRoleSpecifically_AcceptsAnySingleProvider()
    {
        // Deliberately generic - a future direct User/Client/Guest grant reuses this exact
        // factory, only the provider changes.
        var grant = PermissionGrant.Create(Guid.NewGuid(), PermissionProviderName.Guest, "*");

        grant.ProviderName.ShouldBe(PermissionProviderName.Guest);
        grant.ProviderKey.ShouldBe("*");
    }

    [Fact]
    public void Create_CombinedProviderFlags_Throws()
    {
        Action act = () => PermissionGrant.Create(
            Guid.NewGuid(),
            PermissionProviderName.Role | PermissionProviderName.User,
            "key");

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyProviderKey_Throws(string? providerKey)
    {
        Action act = () => PermissionGrant.Create(Guid.NewGuid(), PermissionProviderName.Role, providerKey!);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }
}
