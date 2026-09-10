using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Features.Accounts.Commands.SetAccountLevel;

using NovaCore.BuildingBlock.Application.Abstractions.Services;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class SetAccountLevelHandlerTests
{
    [Fact]
    public async Task Handle_ForwardsToAuthorizationService()
    {
        var actorId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetUserId().Returns(actorId);

        var authorizationService = Substitute.For<IAccountAuthorizationService>();
        var handler = new SetAccountLevelHandler(currentUserService, authorizationService);

        await handler.Handle(new SetAccountLevelCommand(accountId, 10));

        await authorizationService.Received(1).SetLevelAsync(actorId, accountId, 10, Guid.Empty, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoCurrentUser_ThrowsUnauthorized()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetUserId().Returns((Guid?)null);

        var handler = new SetAccountLevelHandler(currentUserService, Substitute.For<IAccountAuthorizationService>());

        await Should.ThrowAsync<UnauthorizedException>(
            () => handler.Handle(new SetAccountLevelCommand(Guid.NewGuid(), 10)));
    }
}
