using NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;
using NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads.Metadata;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.BuildingBlock.Infrastructure.Files.MetadataExtraction;

/// <summary>
/// TagLibSharp is the only mature .NET library that reads (and, later, writes - relevant once the
/// MinIO write-back workflow lands) tags across both audio and video containers, matching
/// MediaUpload's own single-type audio+video split. TagLib seeks around the stream to find
/// ID3/MP4-atom/etc. headers, so the caller must supply a seekable stream. Format resolution is
/// driven by fileName's extension via TagLib's own IFileAbstraction contract, not by sniffing
/// bytes - pass the original uploaded name, not a generated one.
/// </summary>
public sealed class MediaFileMetadataExtractor
{
    public Task<FileMetadataExtractionResult> ExtractAsync(Stream content, string fileName, bool isVideo, CancellationToken ct = default)
    {
        try
        {
            using var tagFile = TagLib.File.Create(new StreamFileAbstraction(fileName, content));

            var properties = tagFile.Properties;
            var tag = tagFile.Tag;

            var metadata = new MediaFileMetadata(
                IsVideo: isVideo,
                Duration: properties?.Duration,
                Width: properties?.VideoWidth > 0 ? properties.VideoWidth : null,
                Height: properties?.VideoHeight > 0 ? properties.VideoHeight : null,
                BitrateKbps: properties?.AudioBitrate > 0 ? properties.AudioBitrate : null,
                Codec: properties?.Description,
                Artist: tag?.FirstPerformer,
                Album: tag?.Album,
                Title: tag?.Title);

            return Task.FromResult(new FileMetadataExtractionResult(metadata, FileUploadValidationState.Valid, (IReadOnlyList<string>)[]));
        }
        catch (Exception ex) when (ex is TagLib.UnsupportedFormatException or TagLib.CorruptFileException)
        {
            return Task.FromResult(new FileMetadataExtractionResult(
                null, FileUploadValidationState.Invalid, (IReadOnlyList<string>)[$"Media file could not be parsed: {ex.Message}"]));
        }
    }

    /// <summary>Adapts a plain Stream to TagLib's file-access contract - the caller owns the stream's lifetime, so CloseStream is a no-op.</summary>
    private sealed class StreamFileAbstraction(string name, Stream stream) : TagLib.File.IFileAbstraction
    {
        public string Name { get; } = name;
        public Stream ReadStream => stream;
        public Stream WriteStream => stream;
        public void CloseStream(Stream stream) { }
    }
}
