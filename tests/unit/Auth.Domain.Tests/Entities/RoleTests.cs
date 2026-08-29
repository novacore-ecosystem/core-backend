using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Domain.ValueObjects;

using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

using NovaCore.TestKit.ShouldlyExtensions;

using Shouldly;

namespace NovaCore.Auth.Domain.Tests.Entities;

public class RoleTests
{
    [Fact]
    public void Create_NoProviderSpecified_DefaultsToUserProvider()
    {
        var role = Role.Create("Operator", RoleCode.Create("operator"));

        role.ProviderName.ShouldBe(PermissionProviderName.User);
        role.ProviderKey.ShouldBeNull();
    }

    [Theory]
    [InlineData(PermissionProviderName.User)]
    [InlineData(PermissionProviderName.Client)]
    [InlineData(PermissionProviderName.Guest)]
    [InlineData(PermissionProviderName.ServiceAccount)]
    public void Create_SingleNonRoleProvider_Succeeds(PermissionProviderName providerName)
    {
        var role = Role.Create("Dispatcher", RoleCode.Create("dispatcher"), providerName: providerName);

        role.ProviderName.ShouldBe(providerName);
    }

    [Fact]
    public void Create_ProviderNameRole_Throws()
    {
        Action act = () => Role.Create(
            "Invalid",
            RoleCode.Create("invalid"),
            providerName: PermissionProviderName.Role);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_CombinedProviderFlags_Throws()
    {
        Action act = () => Role.Create(
            "Invalid",
            RoleCode.Create("invalid"),
            providerName: PermissionProviderName.User | PermissionProviderName.Client);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }

    [Fact]
    public void Create_ProviderNameNone_Throws()
    {
        Action act = () => Role.Create(
            "Invalid",
            RoleCode.Create("invalid"),
            providerName: PermissionProviderName.None);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }
}
