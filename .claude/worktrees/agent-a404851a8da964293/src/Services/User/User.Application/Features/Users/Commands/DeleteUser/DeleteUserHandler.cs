using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.User.Application.Features.Users.Commands.DeleteUser;

public sealed class DeleteUserHandler(
    IUserReadService userReadService,
    IUserWriteService userWriteService,
    IAppLogger<DeleteUserHandler> logger)
    : ICommandHandler<DeleteUserCommand, DeleteUserProfileResponse>
{
    public async Task<DeleteUserProfileResponse> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var existing = await userReadService.GetByIdAsync(request.UserId, ct);
        if (existing is null)
        {
            logger.Information(
                "UserProfile {UserId} already absent, skipping compensating deletion",
                request.UserId);
            return new DeleteUserProfileResponse(Deleted: false);
        }

        await userWriteService.DeleteAsync(request.UserId, ct);

        logger.Warning(
            "Deleted UserProfile {UserId} due to compensating rollback. Reason: {Reason}. CorrelationId: {CorrelationId}",
            request.UserId,
            request.Reason,
            request.CorrelationId);

        return new DeleteUserProfileResponse(Deleted: true);
    }
}
