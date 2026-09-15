using NovaCore.Auth.Application.Abstractions.Persistence.LoginHistories;
using NovaCore.Auth.Application.Abstractions.Persistence.Sessions;
using NovaCore.Auth.Application.Configurations;
using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Features.Auth.Events.OnAuthenticationSucceeded;

/// <summary>
/// Records the side effects of a successful Login/RefreshToken (Session, LoginHistory) out of the
/// request path - Login/RefreshToken only enqueue the integration event that eventually reaches
/// this handler via AuthenticationSucceededConsumer.
/// </summary>
/// <remarks>
/// TODO: Device upsert/touch (IDeviceReadService/IDeviceWriteService) is intentionally not wired
/// in yet - Login/RefreshToken never collect a device fingerprint from the client today, so this
/// event never carries one to key a Device lookup on. Inject those services and branch on a
/// fingerprint field once the frontend contract actually adds one, rather than fabricating device
/// data here.
/// </remarks>
public sealed class OnAuthenticationSucceededHandler(
    ISessionWriteService sessionWriteService,
    ILoginHistoryWriteService loginHistoryWriteService,
    IUnitOfWork unitOfWork,
    SessionJwtSetting jwtSetting,
    IAppLogger<OnAuthenticationSucceededHandler> logger) : IInternalEventHandler<OnAuthenticationSucceededEvent>
{
    public async Task Handle(OnAuthenticationSucceededEvent @event, CancellationToken ct = default)
    {
        var ipAddress = IpAddress.TryCreate(@event.IpAddress, out var parsedIpAddress)
            ? parsedIpAddress!
            : IpAddress.Create("0.0.0.0");

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var expiresAt = DateTime.UtcNow.AddDays(jwtSetting.RefreshTokenExpirationDays);
            await sessionWriteService.CreateAsync(@event.AccountId, ipAddress, expiresAt, ct);
            await loginHistoryWriteService.RecordSuccessLoginAsync(@event.AccountId, ipAddress, ct);
        }, ct: ct);

        logger.Information(
            "Recorded Session and LoginHistory for account {AccountId} ({AuthenticationType})",
            @event.AccountId,
            @event.AuthenticationType);
    }
}
