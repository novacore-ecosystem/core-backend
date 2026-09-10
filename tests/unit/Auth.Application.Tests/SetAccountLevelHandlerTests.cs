using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Features.Accounts.Commands.SetAccountLevel;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Enums;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class SetAccountLevelHandlerTests
{
    private readonly Guid _actorId = Guid.NewGuid();
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IAccountAuthorizationGuard _authorizationGuard = Substitute.For<IAccountAuthorizationGuard>();
    private readonly IAccountReadService _accountReadService = Substitute.For<IAccountReadService>();
    private readonly IAccountWriteService _accountWriteService = Substitute.For<IAccountWriteService>();

    private SetAccountLevelHandler CreateHandler()
        => new(_currentUserService, _authorizationGuard, _accountReadService, _accountWriteService);

    private static Account CreateActorWithLevel(int level)
    {
        var account = Account.Create("actor", Email.Create("actor@novacore.local"), AccountStatus.Active);
        account.SetLevel(level);
        return account;
    }

    [Fact]
    public async Task Handle_SelfTarget_ThrowsForbidden_EvenBeforeGuardRuns()
    {
        _currentUserService.GetUserId().Returns(_actorId);

        await Should.ThrowAsync<ForbiddenException>(
            () => CreateHandler().Handle(new SetAccountLevelCommand(_actorId, 10)));

        await _authorizationGuard.DidNotReceive().EnsureCanManageAccountAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ActorLevelNotAboveRequestedLevel_ThrowsForbidden()
    {
        _currentUserService.GetUserId().Returns(_actorId);
        _accountReadService.GetByIdAsync(_actorId, Arg.Any<CancellationToken>())
            .Returns(CreateActorWithLevel(10));

        await Should.ThrowAsync<ForbiddenException>(
            () => CreateHandler().Handle(new SetAccountLevelCommand(_accountId, 10)));

        await _accountWriteService.DidNotReceive().SetLevelAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ActorOutranksRequestedLevel_SetsLevel()
    {
        _currentUserService.GetUserId().Returns(_actorId);
        _accountReadService.GetByIdAsync(_actorId, Arg.Any<CancellationToken>())
            .Returns(CreateActorWithLevel(50));

        await CreateHandler().Handle(new SetAccountLevelCommand(_accountId, 10));

        await _accountWriteService.Received(1).SetLevelAsync(_accountId, 10, Arg.Any<CancellationToken>());
    }
}
