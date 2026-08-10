using NovaCore.Auth.Application.Abstractions.Services;

using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Contract.Events.User;

namespace NovaCore.Auth.Application.Features.Auth.Events.OnUserRegistered;

public sealed class OnUserRegisteredHandler(
    IUserProfileService userProfileService,
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork,
    IAppLogger<OnUserRegisteredHandler> logger) : IInternalEventHandler<OnUserRegisteredEvent>
{
    public async Task Handle(OnUserRegisteredEvent @event, CancellationToken ct = default)
    {
        logger.Information(
            "Processing user registration for user {UserId}",
            @event.UserId);

        var result = await userProfileService.CreateUserProfileAsync(
            @event.UserId,
            @event.Email,
            @event.UserName,
            @event.FirstName,
            @event.MiddleName,
            @event.LastName,
            @event.PhoneNumber,
            @event.CorrelationId,
            ct);

        if (result.Success && result.Data is not null)
        {
            logger.Information(
                "Successfully created user profile {ProfileId}",
                result.Data.UserId);
        }
        else
        {
            logger.Error(
                null,
                "Failed to create user profile for account {UserId}. Error: {ErrorCode}",
                @event.UserId,
                result.ErrorCode);

            var deleteEvent = new UserDeletionIntegrationEvent(
                @event.UserId.ToString(),
                $"User profile creation failed: {result.ErrorCode}",
                @event.CorrelationId);

            await outboxStore.EnqueueAsync(deleteEvent, ct);
            await unitOfWork.SaveChangesAsync(ct);

            throw new BadRequestException($"Failed to create user profile: {result.Message}");
        }
    }
}
