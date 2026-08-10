using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Features.Auth.Events.OnUserRegistered;
using NovaCore.Auth.Application.Security;

namespace NovaCore.Auth.Application.Features.Auth.Commands.Register;

public sealed class RegisterHandler(
    IUnitOfWork unitOfWork,
    IAuthService authService,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    ICurrentUserService currentUserService,
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<RegisterHandler> logger) : ICommandHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken ct = default)
    {
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

                var roleAssigned = await authService.AssignRoleAsync(
                    account.Id,
                    AppRoleConstant.User,
                    ct);
                if (!roleAssigned)

                    throw new BadRequestException("Failed to assign default role to user");
            },
            ct: ct);

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
        // 
        // 
        // 

        // Generate AccessToken and Refresh Token which are set to HttpOnly
        var jwtId = Guid.NewGuid();
        var roles = await authService.GetUserRolesAsync(account.Id, ct);
        var permissions = RolePermissionMap.Resolve(roles);
        var accessToken = tokenGenerator.GenerateAccessToken(
            account.Id,
            account.Email!,
            account.UserName!,
            roles,
            permissions,
            jwtId);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(
            account.Id,
            jwtId,
            ct);
        currentUserService.SetAccessToken(accessToken);
        currentUserService.SetRefreshToken(refreshToken);

        return new RegisterResult();
    }
}
