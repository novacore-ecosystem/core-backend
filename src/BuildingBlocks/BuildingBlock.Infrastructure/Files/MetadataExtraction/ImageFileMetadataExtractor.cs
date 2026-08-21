using System.Globalization;

using NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;
using NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads.Metadata;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace NovaCore.BuildingBlock.Infrastructure.Files.MetadataExtraction;

/// <summary>
/// SixLabors.ImageSharp.Image.IdentifyAsync reads dimensions + EXIF/IPTC/XMP profiles without
/// decoding pixel data - much cheaper than a full Image.LoadAsync for metadata-only reads. GPS
/// coordinates are deliberately not extracted yet (EXIF stores them as rational-array + N/S/E/W
/// reference tags that are easy to get subtly wrong without a way to verify against real sample
/// files) - ImageFileMetadata keeps the properties for whenever that's added.
/// </summary>
public sealed class ImageFileMetadataExtractor
{
    public async Task<FileMetadataExtractionResult> ExtractAsync(Stream content, CancellationToken ct = default)
    {
        try
        {
            var info = await Image.IdentifyAsync(content, ct);
            if (info is null)
            {
                return new FileMetadataExtractionResult(
                    null, FileUploadValidationState.Invalid, ["Could not identify image format - file may be corrupt."]);
            }

            var exif = info.Metadata.ExifProfile;

            var metadata = new ImageFileMetadata(
                Width: info.Width,
                Height: info.Height,
                Orientation: GetExifOrientation(exif),
                CameraMake: GetExifString(exif, ExifTag.Make),
                CameraModel: GetExifString(exif, ExifTag.Model),
                TakenAt: GetExifDateTime(exif, ExifTag.DateTimeOriginal));

            return new FileMetadataExtractionResult(metadata, FileUploadValidationState.Valid, []);
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException or NotSupportedException)
        {
            return new FileMetadataExtractionResult(
                null, FileUploadValidationState.Invalid, [$"Image could not be parsed: {ex.Message}"]);
        }
    }

    private static string? GetExifString(ExifProfile? exif, ExifTag<string> tag)
    {
        if (exif is null)
            return null;

        return exif.TryGetValue(tag, out var value) ? value.Value : null;
    }

    private static string? GetExifOrientation(ExifProfile? exif)
    {
        if (exif is null || !exif.TryGetValue(ExifTag.Orientation, out var value))
            return null;

        return value.Value.ToString(CultureInfo.InvariantCulture);
    }

    private static DateTime? GetExifDateTime(ExifProfile? exif, ExifTag<string> tag)
    {
        var raw = GetExifString(exif, tag);
        if (raw is null)
            return null;

        // EXIF DateTimeOriginal format: "yyyy:MM:dd HH:mm:ss"
        return DateTime.TryParseExact(raw, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var parsed) ? parsed : null;
    }
}
