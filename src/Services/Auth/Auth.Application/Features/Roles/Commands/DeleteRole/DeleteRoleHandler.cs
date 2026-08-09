using NovaCore.Auth.Application.Abstractions.Persistence.Roles;

namespace NovaCore.Auth.Application.Features.Roles.Commands.DeleteRole;

public sealed class DeleteRoleHandler(
    IRoleReadService roleReadService,
    IRoleWriteService roleWriteService) : ICommandHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand request, CancellationToken ct = default)
    {
        var role = await roleReadService.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Role", request.Id);

        if (role.IsSystemRole)
            throw new BadRequestException("A system role cannot be deleted.");

        await roleWriteService.DeleteAsync(request.Id, ct);
    }
}
