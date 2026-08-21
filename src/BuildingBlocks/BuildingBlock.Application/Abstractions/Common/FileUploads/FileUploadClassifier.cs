namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;

/// <summary>Classifies raw upload metadata into a concrete FileUpload subtype by content-type prefix, and groups already-classified uploads by type so callers don't hand-roll if/else chains.</summary>
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

    public static FileUpload Classify(
        string fileName,
        string contentType,
        long sizeBytes,
        string storageKey,
        int? width = null,
        int? height = null,
        TimeSpan? duration = null)
    {
        var normalizedContentType = contentType.Trim().ToLowerInvariant();

        if (normalizedContentType.StartsWith("image/", StringComparison.Ordinal))
            return new ImageUpload(fileName, contentType, sizeBytes, storageKey, width, height);

        if (normalizedContentType.StartsWith("video/", StringComparison.Ordinal))
            return new MediaUpload(fileName, contentType, sizeBytes, storageKey, IsVideo: true, duration);

        if (normalizedContentType.StartsWith("audio/", StringComparison.Ordinal))
            return new MediaUpload(fileName, contentType, sizeBytes, storageKey, IsVideo: false, duration);

        if (DocumentContentTypes.Contains(normalizedContentType))
            return new DocumentUpload(fileName, contentType, sizeBytes, storageKey);

        return new GenericFileUpload(fileName, contentType, sizeBytes, storageKey);
    }

    public static ILookup<FileUploadType, FileUpload> GroupByType(IEnumerable<FileUpload> uploads) =>
        uploads.ToLookup(u => u.Type);
}
