using NovaCore.BuildingBlock.SharedKernel.Authorization;

using Shouldly;

namespace NovaCore.BuildingBlock.SharedKernel.Tests.Authorization;

public class PermissionProviderNameTests
{
    [Theory]
    [InlineData(PermissionProviderName.Role, "Role")]
    [InlineData(PermissionProviderName.User, "User")]
    [InlineData(PermissionProviderName.Client, "Client")]
    [InlineData(PermissionProviderName.Guest, "Guest")]
    [InlineData(PermissionProviderName.ServiceAccount, "ServiceAccount")]
    public void ToName_SingleValue_ReturnsStableCanonicalName(PermissionProviderName provider, string expected)
    {
        provider.ToName().ShouldBe(expected);
    }

    [Theory]
    [InlineData("Role", PermissionProviderName.Role)]
    [InlineData("User", PermissionProviderName.User)]
    [InlineData("Client", PermissionProviderName.Client)]
    [InlineData("Guest", PermissionProviderName.Guest)]
    [InlineData("ServiceAccount", PermissionProviderName.ServiceAccount)]
    public void ParseName_RoundTripsWithToName(string name, PermissionProviderName expected)
    {
        PermissionProviderNameExtensions.ParseName(name).ShouldBe(expected);
    }

    [Fact]
    public void ToName_None_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PermissionProviderName.None.ToName());
    }

    [Fact]
    public void ToName_CombinedFlags_Throws()
    {
        var combined = PermissionProviderName.Role | PermissionProviderName.User;

        Should.Throw<ArgumentOutOfRangeException>(() => combined.ToName());
    }

    [Fact]
    public void ParseName_UnrecognizedName_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => PermissionProviderNameExtensions.ParseName("Nonexistent"));
    }

    [Theory]
    [InlineData(PermissionProviderName.Role, true)]
    [InlineData(PermissionProviderName.User, true)]
    [InlineData(PermissionProviderName.None, false)]
    public void IsSingleValue_ReportsWhetherExactlyOneProviderIsSet(PermissionProviderName provider, bool expected)
    {
        provider.IsSingleValue().ShouldBe(expected);
    }

    [Fact]
    public void IsSingleValue_CombinedFlags_ReturnsFalse()
    {
        (PermissionProviderName.Role | PermissionProviderName.User).IsSingleValue().ShouldBeFalse();
    }
}
