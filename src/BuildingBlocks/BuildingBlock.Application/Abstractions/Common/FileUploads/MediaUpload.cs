namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;

/// <summary>Covers both video and audio - IsVideo distinguishes them without splitting into two near-identical leaf types.</summary>
public sealed record MediaUpload(
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey,
    bool IsVideo,
    TimeSpan? Duration = null) : FileUpload(FileName, ContentType, SizeBytes, StorageKey)
{
    public override FileUploadType Type => FileUploadType.Media;
}
