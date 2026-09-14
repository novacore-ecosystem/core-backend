using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Features.Accounts.Commands.AssignAccountApp;
using NovaCore.Auth.Application.Features.Accounts.Commands.RemoveAccountApp;

namespace NovaCore.Auth.Application.Tests;

public sealed class AssignAccountAppHandlerTests
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
    public async Task Handle_AssignsAccountToApp()
    {
        var accountId = Guid.NewGuid();
        var appId = Guid.NewGuid();
        var assignmentService = Substitute.For<IAccountAppAssignmentService>();
        var handler = new AssignAccountAppHandler(
            BuildUnitOfWork(), assignmentService, Substitute.For<IAppMembershipCache>());

        await handler.Handle(new AssignAccountAppCommand(accountId, appId));

        await assignmentService.Received(1).AssignAsync(accountId, appId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RemovesAccountFromApp()
    {
        var accountId = Guid.NewGuid();
        var appId = Guid.NewGuid();
        var assignmentService = Substitute.For<IAccountAppAssignmentService>();
        var handler = new RemoveAccountAppHandler(
            BuildUnitOfWork(), assignmentService, Substitute.For<IAppMembershipCache>());

        await handler.Handle(new RemoveAccountAppCommand(accountId, appId));

        await assignmentService.Received(1).RemoveAsync(accountId, appId, Arg.Any<CancellationToken>());
    }
}
