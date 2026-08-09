using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;

namespace NovaCore.Auth.Application.Features.Permissions.Commands.UpdatePermission;

public sealed class UpdatePermissionHandler(IPermissionWriteService permissionWriteService) : ICommandHandler<UpdatePermissionCommand>
{
    public async Task Handle(UpdatePermissionCommand request, CancellationToken ct = default)
    {
        await permissionWriteService.UpdateAsync(request.Id, permission =>
        {
            permission.MoveToGroup(request.PermissionGroupId);
        }, ct);
    }
}
