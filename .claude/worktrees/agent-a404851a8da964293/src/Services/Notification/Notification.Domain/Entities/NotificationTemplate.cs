namespace NovaCore.Notification.Domain.Entities;

/// <summary>
/// A reusable, channel-scoped template selected by <see cref="NotificationRule"/> or
/// <see cref="NotificationCampaign"/> targets. One template is scoped to exactly one
/// <see cref="NotificationChannelType"/> (an Email template's copy/placeholders are unrelated to
/// a Telegram template's) - a multi-channel notification is expressed as multiple targets, each
/// pointing at its own channel-appropriate template, not one template spanning channels.
/// </summary>
public sealed class NotificationTemplate : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public NotificationChannelType Channel { get; private set; }
    public TemplateContent Content { get; private set; } = null!;
    public NotificationTemplateStatus Status { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private NotificationTemplate() { }

    public static NotificationTemplate Create(Guid id, string name, NotificationChannelType channel, TemplateContent content)
    {
        ValidateName(name);

        return new NotificationTemplate
        {
            Id = id,
            Name = name,
            Channel = channel,
            Content = content,
            Status = NotificationTemplateStatus.Active,
        };
    }

    public void Rename(string name)
    {
        ValidateName(name);

        Name = name;
    }

    public void UpdateContent(TemplateContent content)
    {
        Content = content;
    }

    public void Activate()
    {
        Status = NotificationTemplateStatus.Active;
    }

    public void Deactivate()
    {
        Status = NotificationTemplateStatus.Inactive;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Template name cannot be empty.");
    }
}
