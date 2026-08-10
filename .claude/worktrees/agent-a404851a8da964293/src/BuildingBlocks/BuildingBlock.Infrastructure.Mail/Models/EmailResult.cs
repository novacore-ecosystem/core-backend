namespace NovaCore.BuildingBlock.Infrastructure.Mail.Models;

public sealed record EmailResult
{
    public required bool IsSuccess { get; init; }
    public string? MessageId { get; init; }
    public string? Error { get; init; }
    public required DateTimeOffset SentAt { get; init; }

    public static EmailResult Success(string? messageId = null) =>
        new()
        {
            IsSuccess = true,
            MessageId = messageId,
            SentAt = DateTimeOffset.UtcNow,
        };

    public static EmailResult Failure(string error) =>
        new()
        {
            IsSuccess = false,
            Error = error,
            SentAt = DateTimeOffset.UtcNow,
        };
}
