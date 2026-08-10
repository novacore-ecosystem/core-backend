namespace NovaCore.Notification.Domain.Entities;

/// <summary>Represents a target audience (a role, a set of specific users, a segment, ...) that a <see cref="NotificationCampaign"/> broadcasts to.</summary>
public sealed class NotificationGroup : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public NotificationGroupStatus Status { get; private set; }
    public AudienceSelector Audience { get; private set; } = null!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private NotificationGroup() { }

    public static NotificationGroup Create(Guid id, string name, string description, AudienceSelector audience)
    {
        ValidateName(name);

        return new NotificationGroup
        {
            Id = id,
            Name = name,
            Description = description,
            Audience = audience,
            Status = NotificationGroupStatus.Active,
        };
    }

    public void UpdateDetails(string name, string description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void UpdateAudience(AudienceSelector audience)
    {
        Audience = audience;
    }

    public void Activate()
    {
        Status = NotificationGroupStatus.Active;
    }

    public void Deactivate()
    {
        Status = NotificationGroupStatus.Inactive;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Notification group name cannot be empty.");
    }
}
