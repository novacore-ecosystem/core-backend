namespace NovaCore.BuildingBlock.Infrastructure.Mail.Models;

public sealed record EmailAttachment
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Content { get; init; }
    public bool IsInline { get; init; }
    public string? ContentId { get; init; }

    public static EmailAttachment FromBytes(
        string fileName,
        string contentType,
        byte[] content,
        bool isInline = false) =>
        new()
        {
            FileName = fileName,
            ContentType = contentType,
            Content = content,
            IsInline = isInline,
        };

    public static async Task<EmailAttachment> FromStreamAsync(
        string fileName,
        string contentType,
        Stream stream,
        bool isInline = false,
        CancellationToken ct = default)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, ct);

        return new EmailAttachment
        {
            FileName = fileName,
            ContentType = contentType,
            Content = memoryStream.ToArray(),
            IsInline = isInline,
        };
    }
}
