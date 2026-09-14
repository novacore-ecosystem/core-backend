using NovaCore.Auth.Application.Abstractions.Auth;
using NovaCore.Auth.Application.Abstractions.Authorization;
using NovaCore.Auth.Application.Abstractions.Persistence.Accounts;
using NovaCore.Auth.Application.Abstractions.Persistence.Apps;
using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Application.Abstractions.Security.Jwt;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Application.Features.Auth.Events.OnUserRegistered;
using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Features.Auth.Commands.Register;

public sealed class RegisterHandler(
    IUnitOfWork unitOfWork,
    IAuthService authService,
    IAppReadService appReadService,
    IRoleReadService roleReadService,
    IAccountRoleAssignmentService accountRoleAssignmentService,
    IAccountAppAssignmentService accountAppAssignmentService,
    IAccountReadService accountReadService,
    IEffectivePermissionReadService effectivePermissionReadService,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    ICurrentUserService currentUserService,
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<RegisterHandler> logger) : ICommandHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken ct = default)
    {
        var app = await appReadService.GetByCodeAsync(AppCode.Create(request.AppCode), ct)
            ?? throw new NotFoundException("App", request.AppCode);
        if (!app.IsActive)
            throw new BadRequestException($"App ({request.AppCode}) is not active.");

        var existingUser = await authService.GetUserByEmailAsync(request.Email, ct);
        if (existingUser is not null)
            throw new ConflictException($"Email ({request.Email}) already exists");

        var defaultRole = await roleReadService.GetByCodeAsync(RoleCode.Create(AppRoleConstant.User), ct)
            ?? throw new BadRequestException("Default \"User\" role is not seeded.");

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

                await accountRoleAssignmentService.ReplaceRolesAsync(account.Id, [defaultRole.Id], ct);
                await accountAppAssignmentService.AssignAsync(account.Id, app.Id, ct);
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

        // Generate AccessToken and Refresh Token which are set to HttpOnly
        var jwtId = Guid.NewGuid();
        var roles = await accountReadService.GetRoleNamesAsync(account.Id, ct);
        var permissions = await effectivePermissionReadService.GetEffectivePermissionsAsync(account.Id, account.TenantId, ct);
        var accessToken = tokenGenerator.GenerateAccessToken(
            userId: account.Id,
            email: account.Email!,
            username: account.UserName!,
            roles: roles,
            permissions: permissions,
            tenantId: account.TenantId,
            appId: app.Id,
            jwtId: jwtId);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(
            account.Id,
            jwtId,
            ct);
        currentUserService.SetAccessToken(accessToken);
        currentUserService.SetRefreshToken(refreshToken);

        return new RegisterResult();
    }
}
