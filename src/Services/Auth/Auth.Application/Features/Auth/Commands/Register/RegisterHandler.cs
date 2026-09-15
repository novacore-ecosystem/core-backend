using NovaCore.Auth.Application.Abstractions.Apps;
using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Application.Abstractions.Registrations;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Features.Auth.Events.OnUserRegistered;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Application.Features.Auth.Commands.Register;

/// <summary>
/// Creates the account and triggers its initial verification email via the same
/// claim-then-send flow ResendEmail uses (see <see cref="IAuthEmailRequestService.TryDispatchEmailVerificationAsync"/>),
/// so the initial send establishes the same resend cooldown a follow-up resend is bound by; also
/// publishes <see cref="UserRegisteredIntegrationEvent"/> so the dispatch can be retried/observed
/// as a consumer-driven reaction, independent of this request's own lifetime. Never issues
/// access/refresh tokens - the account's email stays unconfirmed until the link is clicked (see
/// LoginHandler's EmailConfirmed gate), so Register cannot authenticate it yet.
/// </summary>
public sealed class RegisterHandler(
    IUnitOfWork unitOfWork,
    IAuthService authService,
    IAppCollectionCache appCollectionCache,
    IAppMembershipCache appMembershipCache,
    IRegistrationDefaultsCache registrationDefaultsCache,
    IAccountRoleAssignmentService accountRoleAssignmentService,
    IPermissionGrantService permissionGrantService,
    IAccountAppAssignmentService accountAppAssignmentService,
    IAuthEmailRequestService authEmailRequestService,
    IOutboxStore outboxStore,
    ICurrentUserService currentUserService,
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<RegisterHandler> logger) : ICommandHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken ct = default)
    {
        var app = await appCollectionCache.GetByCodeAsync(request.AppCode, ct)
            ?? throw new NotFoundException("App", request.AppCode);
        if (!app.IsActive)
            throw new BadRequestException($"App ({request.AppCode}) is not active.");

        var existingUser = await authService.GetUserByEmailAsync(request.Email, ct);
        if (existingUser is not null)
            throw new ConflictException($"Email ({request.Email}) already exists");

        var correlationId = currentUserService.GetCorrelationId()
            ?? Guid.NewGuid().ToString();

        Account? account = null;
        await unitOfWork.ExecuteTransactionAsync(
            action: async () =>
            {
                account = await authService.CreateUserAsync(
                    request.Email,
                    request.Email,
                    request.Password,
                    ct) ?? throw new BadRequestException("Failed to create user");

                await outboxStore.EnqueueAsync(
                    new UserRegisteredIntegrationEvent(account.Id.ToString(), account.Email!, correlationId),
                    ct);

                // (Tenant, App)-scoped defaults, not a hard-seeded Role - gracefully grants
                // nothing when no defaults are configured for this pair.
                var defaults = await registrationDefaultsCache.GetAsync(account.TenantId, app.Id, ct);

                if (defaults.RoleIds.Count > 0)
                    await accountRoleAssignmentService.ReplaceRolesAsync(account.Id, defaults.RoleIds, ct);

                if (defaults.PermissionKeys.Count > 0)
                    await permissionGrantService.ReplaceForProviderAsync(
                        PermissionProviderName.User,
                        account.Id.ToString(),
                        defaults.PermissionKeys,
                        account.TenantId,
                        ct);

                await accountAppAssignmentService.AssignAsync(account.Id, app.Id, ct);
            },
            ct: ct);

        // Registration just changed this App's membership - invalidate after the transaction
        // commits (never before) so the next membership lookup rebuilds a fresh set.
        await appMembershipCache.InvalidateAsync(app.Id, ct);

        // Publish an event to create new user profile via gRPC
        var @event = new OnUserRegisteredEvent(
            account!.Id,
            request.Email,
            request.Email,
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.PhoneNumber,
            correlationId);
        await eventDispatcher.PublishAsync(@event, ct);
        logger.Information(
            "Successfully created account {AccountId} with correlation ID {CorrelationId}",
            account.Id,
            correlationId);

        // TODO: Publish audit log event bus

        // Claims the resend cooldown and sends the email synchronously, in the same request -
        // UserRegisteredIntegrationEvent's consumer also triggers this same flow, but the
        // cooldown must be established here so an immediate resend right after this call returns
        // is rejected regardless of when (or whether) the async consumer has run yet.
        await authEmailRequestService.TryDispatchEmailVerificationAsync(account.Email!, ct);

        return new RegisterResult();
    }
}
