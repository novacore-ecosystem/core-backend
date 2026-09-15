namespace NovaCore.BuildingBlock.Infrastructure.Mail.Options;

public sealed record ResendMailOptions
{
    /// <summary>The Resend API key used to authenticate outgoing requests.</summary>
    public required string ApiKey { get; init; }
    /// <summary>The default sender email address, used when a message does not specify its own <c>From</c>.</summary>
    public required string SenderEmail { get; init; }
    /// <summary>The default sender display name.</summary>
    public string SenderName { get; init; } = string.Empty;
}
