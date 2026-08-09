using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;

namespace NovaCore.Auth.Application.Features.Permissions.Commands.DeletePermission;

/// <summary>Enforces the "root/user cannot be deleted" invariant (see docs/services/auth-service.md,
/// Phase 3) - PermissionDefinition has no Domain-level Delete method to guard this itself, so the
/// check lives here, the one place deletion is actually possible.</summary>
public sealed class DeletePermissionHandler(
    IPermissionReadService permissionReadService,
    IPermissionWriteService permissionWriteService) : ICommandHandler<DeletePermissionCommand>
{
    public async Task Handle(DeletePermissionCommand request, CancellationToken ct = default)
    {
        var permission = await permissionReadService.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Permission", request.Id);

        if (permission.IsSystemPermission)
            throw new BadRequestException("A system permission cannot be deleted.");

        await permissionWriteService.DeleteAsync(request.Id, ct);
    }
}
