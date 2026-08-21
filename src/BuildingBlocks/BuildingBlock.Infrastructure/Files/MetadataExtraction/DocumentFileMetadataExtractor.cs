using NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.BuildingBlock.Infrastructure.Files.MetadataExtraction;

/// <summary>
/// No mature, stably-published .NET library for document (PDF/Office) metadata extraction exists
/// at the time this was written - UglyToad.PdfPig, the closest fit, has only ever published
/// prerelease versions to NuGet (0.1.9-alpha and a "1.7.0-custom" build, no stable release), and
/// hand-rolling PDF/OOXML parsing would violate "don't reinvent format-specific parsing" more than
/// skipping it does. This intentionally returns no metadata (Valid, not Invalid - document
/// extraction is best-effort per the original task, not a hard requirement) until a suitable
/// library is available; swap this implementation out then, the port (IFileMetadataExtractor)
/// doesn't need to change.
/// </summary>
public sealed class DocumentFileMetadataExtractor
{
    public Task<FileMetadataExtractionResult> ExtractAsync(CancellationToken ct = default)
    {
        return Task.FromResult(new FileMetadataExtractionResult(
            null, FileUploadValidationState.Valid, (IReadOnlyList<string>)["Document metadata extraction is not yet implemented - no stably-published parsing library is available."]));
    }
}
