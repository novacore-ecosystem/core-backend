namespace NovaCore.Order.Domain.Entities.Returns;

/// <summary>
/// Independent, admin-managed return-reason catalog ("Defective", "Wrong Size", "Changed Mind") -
/// not hardcoded, same reasoning as OrderTagDefinition. ReturnItem references a reason via
/// ReasonId rather than a free-text field or enum, since reasons vary by business/category and
/// need to be manageable without a deploy.
/// </summary>
public sealed class ReturnReason : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ReturnReason() { }

    public static ReturnReason Create(string code, string name)
    {
        if (code.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Return reason code cannot be empty.");

        if (name.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Return reason name cannot be empty.");

        return new ReturnReason
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            IsActive = true,
        };
    }

    public void Rename(string name)
    {
        if (name.IsNullOrWhiteSpace())
            throw ExceptionFactory.RequiredField("Return reason name cannot be empty.");

        Name = name;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
