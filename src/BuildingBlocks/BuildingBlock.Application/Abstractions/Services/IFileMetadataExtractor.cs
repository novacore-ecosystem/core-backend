using NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;
using NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads.Metadata;

namespace NovaCore.BuildingBlock.Application.Abstractions.Services;

/// <summary>
/// Port onto content inspection - reads real metadata (EXIF, video/audio tags, document info) out
/// of a file's actual bytes. Stream-based rather than tied to a local path or a specific storage
/// SDK, so whatever eventually supplies the stream (a buffered upload today, a future MinIO
/// GetObjectAsync read, an HTTP download) can call through the same port. Implemented in
/// BuildingBlock.Infrastructure, one concrete extractor per media family behind a composite that
/// dispatches by FileUploadType.
/// </summary>
public interface IFileMetadataExtractor
{
    /// <summary>
    /// declaredSizeBytes lets the implementation refuse to touch the stream at all above the
    /// configured safety ceiling without needing a reliable Stream.Length (some sources aren't
    /// seekable). fileName is the original name (its extension helps format-sniffing libraries
    /// like TagLib that key off it rather than the stream's bytes alone). Never throws for a
    /// corrupt/unreadable file - that's an Invalid result, not an exception.
    /// </summary>
    Task<FileMetadataExtractionResult> ExtractAsync(
        Stream content,
        FileUploadType type,
        string fileName,
        string contentType,
        long declaredSizeBytes,
        CancellationToken ct = default);
}

public sealed record FileMetadataExtractionResult(
    FileMetadata? Metadata,
    FileUploadValidationState State,
    IReadOnlyList<string> Details);
