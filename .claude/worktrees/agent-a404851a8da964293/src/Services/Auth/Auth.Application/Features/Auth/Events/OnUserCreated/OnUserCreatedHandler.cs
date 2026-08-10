using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Features.Auth.Events.OnUserDeletion;

namespace NovaCore.Auth.Application.Features.Auth.Events.OnUserCreated;

public sealed class OnUserCreatedHandler(
    IAuthService authService,
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
            var account = await authService.CreateUserAsync(userId, @event.Email, @event.UserName, @event.TempPassword, ct)
                ?? throw new InvalidOperationException("Failed to create Auth account");

            foreach (var role in @event.Roles)
            {
                var assigned = await authService.AssignRoleAsync(account.Id, role, ct);
                if (!assigned)
                    throw new InvalidOperationException($"Failed to assign role {role}");
            }

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
