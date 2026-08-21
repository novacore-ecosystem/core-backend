namespace NovaCore.Chat.Application.Features.Messages.DTOs;

/// <summary>
/// A file the client already uploaded elsewhere and is attaching to this message by reference -
/// MediaService doesn't exist yet, so StorageKey is whatever pointer the client already has. See
/// SendMessageHandler, which classifies these into the shared FileUpload hierarchy.
/// </summary>
public sealed record MessageAttachmentRequest(
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey,
    int? Width = null,
    int? Height = null,
    int? DurationSeconds = null);
