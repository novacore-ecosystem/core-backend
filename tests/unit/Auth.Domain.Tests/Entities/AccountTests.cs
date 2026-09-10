using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.TestKit.ShouldlyExtensions;

using Shouldly;

namespace NovaCore.Auth.Domain.Tests.Entities;

public class AccountTests
{
    private static Account CreateAccount()
        => Account.Create("operator", Email.Create("operator@novacore.local"), AccountStatus.Active);

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(int.MaxValue)]
    public void SetLevel_NonNegative_Succeeds(int level)
    {
        var account = CreateAccount();

        account.SetLevel(level);

        account.Level.ShouldBe(level);
    }

    [Fact]
    public void SetLevel_Negative_Throws()
    {
        var account = CreateAccount();

        Action act = () => account.SetLevel(-1);

        act.ShouldThrowDomainException<InvalidArgumentException>(MessageCode.InvalidInput);
    }
}
