using NovaCore.Auth.Domain.Entities.Roles;

namespace NovaCore.Auth.Domain.Entities.Positions;

public sealed class PositionRole : BaseEntity, ITenantEntity
{
    public Guid PositionId { get; init; }
    public Position Position { get; init; } = default!;
    public Guid RoleId { get; init; }
    public Role Role { get; init; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private PositionRole() { }

    internal static PositionRole Create(Guid positionId, Guid roleId)
    {
        return new PositionRole
        {
            PositionId = positionId,
            RoleId = roleId,
        };
    }
}
