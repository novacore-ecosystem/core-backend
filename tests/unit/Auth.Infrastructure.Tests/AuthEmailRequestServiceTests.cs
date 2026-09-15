using NSubstitute;

using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Security.PasswordReset;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Configurations;
using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Domain.Enums;
using NovaCore.Auth.Infrastructure.Configurations.Settings;
using NovaCore.Auth.Infrastructure.Services;

using NovaCore.BuildingBlock.Contract.Events;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using Shouldly;

namespace NovaCore.Auth.Infrastructure.Tests;

public sealed class AuthEmailRequestServiceTests
{
    private static readonly TimeSpan CooldownWindow = TimeSpan.FromSeconds(30);

    private sealed class Fixture
    {
        public IAccountReadService AccountReadService { get; } = Substitute.For<IAccountReadService>();
        public IAuthService AuthService { get; } = Substitute.For<IAuthService>();
        public IPasswordResetTokenService PasswordResetTokenService { get; } = Substitute.For<IPasswordResetTokenService>();
        public IOutboxStore OutboxStore { get; } = Substitute.For<IOutboxStore>();
        public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();
        public IEmailResendCooldown Cooldown { get; } = Substitute.For<IEmailResendCooldown>();
        public IAppLogger<AuthEmailRequestService> Logger { get; } = Substitute.For<IAppLogger<AuthEmailRequestService>>();

        public AuthEmailRequestService BuildService()
        {
            UnitOfWork.ExecuteTransactionAsync(Arg.Any<Func<Task>>(), Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
                .Returns(async ci =>
                {
                    await ci.ArgAt<Func<Task>>(0)();
                    return true;
                });

            return new AuthEmailRequestService(
                AccountReadService,
                AuthService,
                PasswordResetTokenService,
                new ClientSetting { EmailVerificationUrl = "https://app.test/verify", ResetPasswordUrl = "https://app.test/reset" },
                OutboxStore,
                UnitOfWork,
                Cooldown,
                new EmailResendCooldownSetting { Window = CooldownWindow },
                Logger);
        }
    }

    private static Account CreateUnconfirmedAccount(string email)
    {
        var account = Account.Create(email, Email.Create(email), AccountStatus.Active);
        return account;
    }

    [Fact]
    public async Task TryDispatchEmailVerificationAsync_CooldownFree_ClaimsAndSendsVerificationEmail()
    {
        var fixture = new Fixture();
        var account = CreateUnconfirmedAccount("user@example.com");

        fixture.Cooldown.TryClaimAsync($"EmailVerification:{account.Email}", CooldownWindow, Arg.Any<CancellationToken>())
            .Returns(new CooldownClaim(true, 0));
        fixture.AccountReadService.GetByEmailAsync(account.Email!, Arg.Any<CancellationToken>()).Returns(account);
        fixture.AuthService.GenerateEmailConfirmationTokenAsync(account.Id, Arg.Any<CancellationToken>()).Returns("token");

        var service = fixture.BuildService();

        var claim = await service.TryDispatchEmailVerificationAsync(account.Email!);

        claim.Claimed.ShouldBeTrue();
        await fixture.OutboxStore.Received(1).EnqueueAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
        await fixture.Cooldown.DidNotReceive().ReleaseAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryDispatchEmailVerificationAsync_CooldownActive_DoesNotSendAndReturnsUnclaimed()
    {
        var fixture = new Fixture();
        const string email = "user@example.com";

        fixture.Cooldown.TryClaimAsync($"EmailVerification:{email}", CooldownWindow, Arg.Any<CancellationToken>())
            .Returns(new CooldownClaim(false, 17));

        var service = fixture.BuildService();

        var claim = await service.TryDispatchEmailVerificationAsync(email);

        claim.Claimed.ShouldBeFalse();
        claim.RemainingSeconds.ShouldBe(17);
        await fixture.AccountReadService.DidNotReceive().GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await fixture.OutboxStore.DidNotReceive().EnqueueAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryDispatchEmailVerificationAsync_SendThrows_ReleasesCooldownAndRethrows()
    {
        var fixture = new Fixture();
        const string email = "user@example.com";

        fixture.Cooldown.TryClaimAsync($"EmailVerification:{email}", CooldownWindow, Arg.Any<CancellationToken>())
            .Returns(new CooldownClaim(true, 0));
        fixture.AccountReadService.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Account?>(new InvalidOperationException("boom")));

        var service = fixture.BuildService();

        await Should.ThrowAsync<InvalidOperationException>(() => service.TryDispatchEmailVerificationAsync(email));

        await fixture.Cooldown.Received(1).ReleaseAsync($"EmailVerification:{email}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryDispatchPasswordResetAsync_CooldownFree_ClaimsAndSendsResetEmail()
    {
        var fixture = new Fixture();
        var account = CreateUnconfirmedAccount("user@example.com");

        fixture.Cooldown.TryClaimAsync($"PasswordReset:{account.Email}", CooldownWindow, Arg.Any<CancellationToken>())
            .Returns(new CooldownClaim(true, 0));
        fixture.AccountReadService.GetByEmailAsync(account.Email!, Arg.Any<CancellationToken>()).Returns(account);
        fixture.PasswordResetTokenService.GenerateTokenAsync(account.Id, Arg.Any<CancellationToken>()).Returns("reset-token");

        var service = fixture.BuildService();

        var claim = await service.TryDispatchPasswordResetAsync(account.Email!);

        claim.Claimed.ShouldBeTrue();
        await fixture.OutboxStore.Received(1).EnqueueAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryDispatchPasswordResetAsync_CooldownActive_RejectedWithoutSending()
    {
        var fixture = new Fixture();
        const string email = "user@example.com";

        fixture.Cooldown.TryClaimAsync($"PasswordReset:{email}", CooldownWindow, Arg.Any<CancellationToken>())
            .Returns(new CooldownClaim(false, 5));

        var service = fixture.BuildService();

        var claim = await service.TryDispatchPasswordResetAsync(email);

        claim.Claimed.ShouldBeFalse();
        claim.RemainingSeconds.ShouldBe(5);
        await fixture.OutboxStore.DidNotReceive().EnqueueAsync(Arg.Any<IIntegrationEvent>(), Arg.Any<CancellationToken>());
    }
}
