using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Application.Features.Auth.Events.OnUserDeletion;
using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Features.Auth.Events.OnUserCreated;

public sealed class OnUserCreatedHandler(
    IAuthService authService,
    IRoleReadService roleReadService,
    IAccountRoleAssignmentService accountRoleAssignmentService,
    IInternalEventDispatcher appEventDispatcher,
    IAppLogger<OnUserCreatedHandler> logger) : IInternalEventHandler<OnUserCreatedEvent>
{
    public async Task Handle(OnUserCreatedEvent @event, CancellationToken ct = default)
    {
        var userId = Guid.Parse(@event.UserId);

        var existingAccount = await authService.GetUserByIdAsync(userId, ct);
        if (existingAccount is not null)
        {
            logger.Information(
                "User account creation confirmed for user {UserId}, correlation {CorrelationId}",
                @event.UserId,
                @event.CorrelationId);
            return;
        }

        try
        {
            var account = await authService.CreateUserAsync(
                userId,
                @event.Email,
                @event.UserName,
                @event.TempPassword,
                ct)
                ?? throw new InvalidOperationException("Failed to create Auth account");

            var roleIds = new List<Guid>(@event.Roles.Length);
            foreach (var role in @event.Roles)
            {
                var resolvedRole = await roleReadService.GetByCodeAsync(RoleCode.Create(role), ct)
                    ?? throw new InvalidOperationException($"Failed to assign role {role}");
                roleIds.Add(resolvedRole.Id);
            }

            await accountRoleAssignmentService.ReplaceRolesAsync(account.Id, roleIds, ct);

            logger.Information(
                "Provisioned Auth account for user {UserId} with roles {Roles}",
                @event.UserId,
                string.Join(",", @event.Roles));
        }
        catch (Exception ex)
        {
            logger.Error(
                ex,
                "Failed to provision Auth account for {UserId}, rolling back",
                @event.UserId);

            var deleteEvent = new OnUserDeletionEvent(
                @event.UserId,
                $"Account provisioning failed: {ex.Message}",
                @event.CorrelationId);

            await appEventDispatcher.PublishAsync(deleteEvent, ct);
            throw;
        }
    }
}
