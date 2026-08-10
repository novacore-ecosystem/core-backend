using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.User;

using NovaCore.User.Application.Abstractions.Services;

namespace NovaCore.User.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler(
    IUserWriteService userWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser,
    IUserProfileDetailCache userProfileCache) : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken ct = default)
    {
        var correlationId = currentUser.GetCorrelationId();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await userWriteService.UpdateProfileDetailsAsync(
                request.UserId,
                request.FirstName,
                request.MiddleName,
                request.LastName,
                request.PhoneNumber,
                ct);

            var integrationEvent = new UserProfileUpdatedIntegrationEvent(
                request.UserId,
                correlationId);
            await outboxStore.EnqueueAsync(integrationEvent, ct);
        }, ct: ct);

        // Only now, after the transaction has actually committed, is it safe to invalidate -
        // doing this inside the transaction delegate above would drop the cache before the write
        // is durable, so a concurrent read could repopulate it with pre-commit (or, on rollback,
        // permanently wrong) data.
        await userProfileCache.InvalidateAsync(request.UserId, ct);

        return new UpdateUserResponse();
    }
}
