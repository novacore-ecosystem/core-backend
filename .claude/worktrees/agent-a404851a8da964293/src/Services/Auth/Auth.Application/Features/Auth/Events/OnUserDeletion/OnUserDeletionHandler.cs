using NovaCore.Auth.Application.Abstractions.Auth;

namespace NovaCore.Auth.Application.Features.Auth.Events.OnUserDeletion;

public sealed class OnUserDeletionHandler(
    IAuthService authService,
    IAppLogger<OnUserDeletionHandler> logger) : IInternalEventHandler<OnUserDeletionEvent>
{
    public async Task Handle(OnUserDeletionEvent notification, CancellationToken ct)
    {
        logger.Warning(
            "Rolling back user account {UserId}. Reason: {Reason}. Correlation ID: {CorrelationId}",
            notification.UserId,
            notification.Reason,
            notification.CorrelationId);

        try
        {
            if (!Guid.TryParse(notification.UserId, out var userId))
                throw new InvalidOperationException($"Invalid user ID format: {notification.UserId}");

            await authService.DeleteUserAsync(userId, ct);
            logger.Information(
                "Successfully deleted user account {UserId}",
                notification.UserId);
        }
        catch (Exception ex)
        {
            logger.Error(
                ex,
                "Failed to delete user account {UserId} during rollback",
                notification.UserId);
            throw;
        }
    }
}
