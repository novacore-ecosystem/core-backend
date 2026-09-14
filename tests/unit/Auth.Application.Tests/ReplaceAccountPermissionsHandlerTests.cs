using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountPermissions;

using NovaCore.BuildingBlock.Application.Abstractions.Services;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class ReplaceAccountPermissionsHandlerTests
{
    private static IUnitOfWork BuildUnitOfWork()
    {
        var uow = Substitute.For<IUnitOfWork>();
        uow.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                await ci.ArgAt<Func<Task>>(0)();
                return true;
            });
        return uow;
    }

    [Fact]
    public async Task Handle_ForwardsToAuthorizationService()
    {
        var actorId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var permissionKeys = new[] { "order:view" };

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetUserId().Returns(actorId);

        var authorizationService = Substitute.For<IAccountAuthorizationService>();
        var handler = new ReplaceAccountPermissionsHandler(currentUserService, authorizationService, BuildUnitOfWork());

        await handler.Handle(new ReplaceAccountPermissionsCommand(accountId, permissionKeys));

        await authorizationService.Received(1).ReplacePermissionsAsync(actorId, accountId, permissionKeys, Guid.Empty, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoCurrentUser_ThrowsUnauthorized()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetUserId().Returns((Guid?)null);

        var handler = new ReplaceAccountPermissionsHandler(
            currentUserService,
            Substitute.For<IAccountAuthorizationService>(),
            BuildUnitOfWork());

        await Should.ThrowAsync<UnauthorizedException>(
            () => handler.Handle(new ReplaceAccountPermissionsCommand(Guid.NewGuid(), [])));
    }
}
