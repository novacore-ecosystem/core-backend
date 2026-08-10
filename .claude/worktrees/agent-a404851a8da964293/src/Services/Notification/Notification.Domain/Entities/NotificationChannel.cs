namespace NovaCore.Notification.Domain.Entities;

/// <summary>
/// Runtime configuration for one delivery platform (SMTP host/credentials, a Telegram bot token,
/// ...) - not a business channel selector (that's <see cref="NotificationChannelType"/> used
/// directly on targets/templates/dispatches). Rows are seeded by the system; Admin may only
/// Enable/Disable, update or reset configuration, and record validation results - never create,
/// delete, or change which <see cref="NotificationChannelType"/> a row represents. InApp is
/// rejected in <see cref="Create"/> since the Notification Center has no runtime delivery
/// configuration to seed.
/// </summary>
public sealed class NotificationChannel : AggregateRoot<Guid>, IAuditable
{
    public NotificationChannelType ChannelType { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public NotificationChannelStatus Status { get; private set; }
    public ChannelConfiguration Configuration { get; private set; } = ChannelConfiguration.Empty;
    public ChannelValidationStatus ValidationStatus { get; private set; }
    public DateTime? LastValidatedAt { get; private set; }
    public string? LastValidationError { get; private set; }

    private NotificationChannel() { }

    public static NotificationChannel Create(Guid id, NotificationChannelType channelType, string displayName, ChannelConfiguration configuration)
    {
        if (channelType == NotificationChannelType.InApp)
            throw ExceptionFactory.InvalidState("InApp is not a deliverable channel and cannot have runtime configuration.");

        ValidateDisplayName(displayName);

        return new NotificationChannel
        {
            Id = id,
            ChannelType = channelType,
            DisplayName = displayName,
            Configuration = configuration,
            Status = NotificationChannelStatus.Inactive,
            ValidationStatus = ChannelValidationStatus.NotValidated,
        };
    }

    public void UpdateConfiguration(ChannelConfiguration configuration)
    {
        Configuration = configuration;
        ValidationStatus = ChannelValidationStatus.NotValidated;
        LastValidationError = null;
    }

    public void ResetConfiguration()
    {
        Configuration = ChannelConfiguration.Empty;
        ValidationStatus = ChannelValidationStatus.NotValidated;
        LastValidationError = null;
    }

    public void RecordValidationResult(bool isValid, string? error = null)
    {
        ValidationStatus = isValid ? ChannelValidationStatus.Valid : ChannelValidationStatus.Invalid;
        LastValidatedAt = DateTime.UtcNow;
        LastValidationError = isValid ? null : error;
    }

    public void Enable()
    {
        if (ValidationStatus != ChannelValidationStatus.Valid)
            throw ExceptionFactory.InvalidState("Cannot enable a channel whose configuration has not been validated.");

        Status = NotificationChannelStatus.Active;
    }

    public void Disable()
    {
        Status = NotificationChannelStatus.Inactive;
    }

    public static bool IsValidDisplayName(string? displayName) => !string.IsNullOrWhiteSpace(displayName);

    private static void ValidateDisplayName(string displayName)
    {
        if (!IsValidDisplayName(displayName))
            throw ExceptionFactory.RequiredField("Channel display name cannot be empty.");
    }
}
