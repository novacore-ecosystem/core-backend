using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.User;

using NovaCore.User.Application.Abstractions.Services;

namespace NovaCore.User.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserHandler(
    IUserWriteService userWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    IAuthClientService authClient,
    ICurrentUserService currentUser) : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken ct = default)
    {
        // Check existing email
        var isExistEmail = await authClient.EmailExistsAsync(request.Email, ct);
        if (isExistEmail)
            throw new ConflictException("User {UserId} already exists, returning existing profile (idempotent)");

        // Role/position grant authorization used to be enforced here against AppRoleConstant's
        // fixed Root/Admin/User set - removed along with that constant. Once Auth's Role/Position
        // admin APIs back this field for real, authorization belongs there (Auth already knows
        // which roles/positions the caller may grant), not re-implemented against a hardcoded set
        // here. See CreateUserCommand's doc comment.

        var correlationId = currentUser.GetCorrelationId();
        UserReadModel user = null!;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            user = await userWriteService.CreateAsync(
                new CreateUserRequest(
                    request.UserName,
                    $"{request.FirstName} {request.LastName}".Trim(),
                    UserType.Customer,
                    request.Email,
                    request.PhoneNumber,
                    request.FirstName,
                    request.MiddleName,
                    request.LastName),
                ct);
            await PublishProfileCreatedEventAsync(
                user,
                request.Roles,
                request.TempPassword,
                correlationId,
                ct);
        }, ct: ct);

        return new CreateUserResponse(user.Id);
    }

    private async Task PublishProfileCreatedEventAsync(
        UserReadModel createdUser,
        string[] roles,
        string tempPassword,
        string correlationId,
        CancellationToken ct)
    {
        var integrationEvent = new UserProfileCreatedIntegrationEvent(
            createdUser.TenantId,
            createdUser.Id,
            createdUser.Email,
            createdUser.UserName,
            createdUser.FirstName,
            createdUser.MiddleName,
            createdUser.LastName,
            correlationId,
            roles,
            tempPassword);
        await outboxStore.EnqueueAsync(integrationEvent, ct);
    }
}
