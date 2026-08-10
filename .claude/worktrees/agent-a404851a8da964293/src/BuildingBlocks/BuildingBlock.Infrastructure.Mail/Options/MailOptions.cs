namespace NovaCore.BuildingBlock.Infrastructure.Mail.Options;

public sealed record MailOptions
{
    /// <summary>The SMTP server host name or IP address.</summary>
    public required string Host { get; init; }
    /// <summary>The SMTP server port.</summary>
    public required int Port { get; init; }
    /// <summary>The username for authentication. Leave empty to connect without authentication.</summary>
    public string? Username { get; init; }
    /// <summary>The password for authentication.</summary>
    public string? Password { get; init; }
    /// <summary>Indicates whether to negotiate SSL/TLS automatically based on the server capabilities.</summary>
    public bool UseSsl { get; init; } = true;
    /// <summary>The default sender email address, used when a message does not specify its own <c>From</c>.</summary>
    public required string SenderEmail { get; init; }
    /// <summary>The default sender display name.</summary>
    public string SenderName { get; init; } = string.Empty;
    /// <summary>The connection/send timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}
