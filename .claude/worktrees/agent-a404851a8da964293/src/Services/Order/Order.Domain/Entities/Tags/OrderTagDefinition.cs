namespace NovaCore.Order.Domain.Entities.Tags;

/// <summary>
/// Independent, admin-managed backoffice label ("VIP", "FRAUD_RISK", "URGENT") - flat, no
/// hierarchy, no hardcoded list (see the Order Service architecture refactor plan's OrderTag
/// section). Orders reference definitions via OrderTag, the same way User references UserTag via
/// UserTagMapping. System tags (IsSystem) are predefined by the platform and cannot be renamed,
/// only have their description/color/active state updated.
/// </summary>
public sealed class OrderTagDefinition : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Color { get; private set; }
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private OrderTagDefinition() { }

    public static OrderTagDefinition Create(
        string code,
        string name,
        string? description = null,
        string? color = null,
        bool isSystem = false)
    {
        if (code.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Tag code cannot be empty.");

        if (name.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Tag name cannot be empty.");

        return new OrderTagDefinition
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            Color = color,
            IsSystem = isSystem,
            IsActive = true,
        };
    }

    #region Details & lifecycle
    public void UpdateDetails(string name, string? description, string? color)
    {
        if (name.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Tag name cannot be empty.");

        Name = name;
        Description = description;
        Color = color;
    }

    public void Activate() => IsActive = true;

    public void Deactivate()
    {
        EnsureNotSystem();

        IsActive = false;
    }

    private void EnsureNotSystem()
    {
        if (IsSystem)
            throw ExceptionFactory.InvalidState("System tags cannot be deactivated.");
    }
    #endregion
}
