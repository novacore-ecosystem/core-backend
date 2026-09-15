using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Features.Auth.Commands.DispatchVerificationEmail;

using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.Auth.Application.Tests;

public sealed class DispatchVerificationEmailHandlerTests
{
    [Fact]
    public async Task Handle_CooldownFree_DispatchesVerificationEmail()
    {
        var authEmailRequestService = Substitute.For<IAuthEmailRequestService>();
        authEmailRequestService.TryDispatchEmailVerificationAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(new CooldownClaim(true, 0));

        var handler = new DispatchVerificationEmailHandler(
            authEmailRequestService,
            Substitute.For<IAppLogger<DispatchVerificationEmailHandler>>());

        // Should not throw - a successful dispatch is a normal, silent outcome for this consumer-driven path.
        await handler.Handle(new DispatchVerificationEmailCommand("user@example.com"));

        await authEmailRequestService.Received(1).TryDispatchEmailVerificationAsync("user@example.com", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CooldownAlreadyClaimed_NoOpsWithoutThrowing()
    {
        // Register's own synchronous call already claimed the cooldown and sent the email before
        // this consumer ran (or a previous delivery of the same event already did) - this must be
        // a benign no-op, not an error, so Inbox retry/dead-letter tracking never sees it as one.
        var authEmailRequestService = Substitute.For<IAuthEmailRequestService>();
        authEmailRequestService.TryDispatchEmailVerificationAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(new CooldownClaim(false, 25));

        var handler = new DispatchVerificationEmailHandler(
            authEmailRequestService,
            Substitute.For<IAppLogger<DispatchVerificationEmailHandler>>());

        await handler.Handle(new DispatchVerificationEmailCommand("user@example.com"));
    }
}
