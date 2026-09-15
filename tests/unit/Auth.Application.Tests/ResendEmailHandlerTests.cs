using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Features.Auth.Commands.ResendEmail;

using Shouldly;

namespace NovaCore.Auth.Application.Tests;

public sealed class ResendEmailHandlerTests
{
    [Fact]
    public async Task Handle_EmailVerification_CooldownFree_DispatchesThroughSharedFlow()
    {
        var authEmailRequestService = Substitute.For<IAuthEmailRequestService>();
        authEmailRequestService.TryDispatchEmailVerificationAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(new CooldownClaim(true, 0));

        var handler = new ResendEmailHandler(authEmailRequestService);
        var command = new ResendEmailCommand("user@example.com", AuthMailPurpose.EmailVerification);

        await handler.Handle(command);

        await authEmailRequestService.Received(1).TryDispatchEmailVerificationAsync("user@example.com", Arg.Any<CancellationToken>());
        await authEmailRequestService.DidNotReceive().TryDispatchPasswordResetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailVerification_CooldownActive_ThrowsBadRequestWithRemainingSeconds()
    {
        var authEmailRequestService = Substitute.For<IAuthEmailRequestService>();
        authEmailRequestService.TryDispatchEmailVerificationAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(new CooldownClaim(false, 12));

        var handler = new ResendEmailHandler(authEmailRequestService);
        var command = new ResendEmailCommand("user@example.com", AuthMailPurpose.EmailVerification);

        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_PasswordReset_CooldownFree_DispatchesThroughSharedFlow()
    {
        var authEmailRequestService = Substitute.For<IAuthEmailRequestService>();
        authEmailRequestService.TryDispatchPasswordResetAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(new CooldownClaim(true, 0));

        var handler = new ResendEmailHandler(authEmailRequestService);
        var command = new ResendEmailCommand("user@example.com", AuthMailPurpose.PasswordReset);

        await handler.Handle(command);

        await authEmailRequestService.Received(1).TryDispatchPasswordResetAsync("user@example.com", Arg.Any<CancellationToken>());
        await authEmailRequestService.DidNotReceive().TryDispatchEmailVerificationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NormalizesEmailCasingAndWhitespace()
    {
        var authEmailRequestService = Substitute.For<IAuthEmailRequestService>();
        authEmailRequestService.TryDispatchEmailVerificationAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(new CooldownClaim(true, 0));

        var handler = new ResendEmailHandler(authEmailRequestService);
        var command = new ResendEmailCommand("  User@Example.com  ", AuthMailPurpose.EmailVerification);

        await handler.Handle(command);

        await authEmailRequestService.Received(1).TryDispatchEmailVerificationAsync("user@example.com", Arg.Any<CancellationToken>());
    }
}
