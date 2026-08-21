namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;

/// <summary>
/// Classifies raw upload metadata into a concrete FileUpload subtype by content-type prefix, and
/// groups already-classified uploads by type so callers don't hand-roll if/else chains. This is
/// opt-in for use cases that want type-specific handling - a use case accepting arbitrary files
/// regardless of extension/category should construct GenericFileUpload directly instead of routing
/// everything through here.
/// </summary>
public static class FileUploadClassifier
{
    private static readonly HashSet<string> DocumentContentTypes =
    [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain",
        "text/csv",
    ];

    /// <summary>sizePolicy is required (no default) so the caller always makes an explicit business-size decision rather than one being silently assumed here - see FileSizePolicy.</summary>
    public static FileUpload Classify(
        string fileName,
        string contentType,
        long sizeBytes,
        string storageKey,
        FileSizePolicy sizePolicy,
        int? width = null,
        int? height = null,
        TimeSpan? duration = null)
    {
        var normalizedContentType = contentType.Trim().ToLowerInvariant();
        var (state, details) = EvaluateSize(sizeBytes, sizePolicy);

        FileUpload upload = normalizedContentType switch
        {
            _ when normalizedContentType.StartsWith("image/", StringComparison.Ordinal) =>
                new ImageUpload(fileName, contentType, sizeBytes, storageKey, width, height),
            _ when normalizedContentType.StartsWith("video/", StringComparison.Ordinal) =>
                new MediaUpload(fileName, contentType, sizeBytes, storageKey, IsVideo: true, duration),
            _ when normalizedContentType.StartsWith("audio/", StringComparison.Ordinal) =>
                new MediaUpload(fileName, contentType, sizeBytes, storageKey, IsVideo: false, duration),
            _ when DocumentContentTypes.Contains(normalizedContentType) =>
                new DocumentUpload(fileName, contentType, sizeBytes, storageKey),
            _ => new GenericFileUpload(fileName, contentType, sizeBytes, storageKey),
        };

        return upload with { ValidationState = state, ValidationDetails = details };
    }

    public static ILookup<FileUploadType, FileUpload> GroupByType(IEnumerable<FileUpload> uploads) =>
        uploads.ToLookup(u => u.Type);

    private static (FileUploadValidationState State, IReadOnlyList<string> Details) EvaluateSize(
        long sizeBytes, FileSizePolicy sizePolicy)
    {
        if (sizeBytes > sizePolicy.MaxParseSizeBytes)
        {
            return (FileUploadValidationState.Invalid,
                [$"File size ({ToMb(sizeBytes)} MB) exceeds the maximum safe parse size ({ToMb(sizePolicy.MaxParseSizeBytes)} MB) - it will not be opened for content inspection."]);
        }

        if (sizeBytes > sizePolicy.AllowedSizeBytes)
        {
            return (FileUploadValidationState.Abnormal,
                [$"File size ({ToMb(sizeBytes)} MB) exceeds the allowed size ({ToMb(sizePolicy.AllowedSizeBytes)} MB) but is within the safe parse limit."]);
        }

        return (FileUploadValidationState.Valid, []);
    }

    private static double ToMb(long bytes) => Math.Round(bytes / (1024.0 * 1024.0), 2);
}
