using NovaCore.Auth.Application.Abstractions.Persistence.Roles;

namespace NovaCore.Auth.Application.Features.Roles.Commands.UpdateRole;

public sealed class UpdateRoleHandler(IRoleWriteService roleWriteService) : ICommandHandler<UpdateRoleCommand>
{
    public async Task Handle(UpdateRoleCommand request, CancellationToken ct = default)
    {
        await roleWriteService.UpdateAsync(request.Id, role =>
        {
            role.Rename(request.Name);
            role.UpdateDescription(request.Description);
        }, ct);
    }
}
