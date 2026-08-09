using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Domain.ValueObjects;

namespace NovaCore.Auth.Application.Features.Roles.Commands.CreateRole;

public sealed class CreateRoleHandler(IRoleWriteService roleWriteService) : ICommandHandler<CreateRoleCommand, Guid>
{
    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken ct = default)
    {
        var role = Role.Create(request.Name, RoleCode.Create(request.Code), request.Description);

        await roleWriteService.CreateAsync(role, ct);

        return role.Id;
    }
}
