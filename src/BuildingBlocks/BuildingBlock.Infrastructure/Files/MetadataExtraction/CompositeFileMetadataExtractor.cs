using Microsoft.Extensions.Options;

using NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.BuildingBlock.Infrastructure.Files.MetadataExtraction;

/// <summary>
/// Sole DI-registered IFileMetadataExtractor - enforces the MaxParseSizeBytes safety ceiling before
/// touching the stream at all, then dispatches to the format-specific extractor by FileUploadType.
/// Callers never talk to ImageFileMetadataExtractor/MediaFileMetadataExtractor/
/// DocumentFileMetadataExtractor directly.
/// </summary>
public sealed class CompositeFileMetadataExtractor(
    ImageFileMetadataExtractor imageExtractor,
    MediaFileMetadataExtractor mediaExtractor,
    DocumentFileMetadataExtractor documentExtractor,
    IOptions<FileParsingOptions> options) : IFileMetadataExtractor
{
    public async Task<FileMetadataExtractionResult> ExtractAsync(
        Stream content,
        FileUploadType type,
        string fileName,
        string contentType,
        long declaredSizeBytes,
        CancellationToken ct = default)
    {
        var maxParseSizeBytes = options.Value.MaxParseSizeBytes;
        if (declaredSizeBytes > maxParseSizeBytes)
        {
            return new FileMetadataExtractionResult(
                null,
                FileUploadValidationState.Invalid,
                [$"File size exceeds the maximum safe parse size ({maxParseSizeBytes / (1024 * 1024)} MB) - not opened for content inspection."]);
        }

        return type switch
        {
            FileUploadType.Image => await imageExtractor.ExtractAsync(content, ct),
            FileUploadType.Media => await mediaExtractor.ExtractAsync(content, fileName, IsVideo(contentType), ct),
            FileUploadType.Document => await documentExtractor.ExtractAsync(ct),
            _ => new FileMetadataExtractionResult(null, FileUploadValidationState.Valid, []),
        };
    }

    private static bool IsVideo(string contentType) =>
        contentType.Trim().StartsWith("video/", StringComparison.OrdinalIgnoreCase);
}
