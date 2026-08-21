namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;

/// <summary>Fallback for content that FileUploadClassifier cannot classify into a more specific type.</summary>
public sealed record GenericFileUpload(
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey) : FileUpload(FileName, ContentType, SizeBytes, StorageKey)
{
    public override FileUploadType Type => FileUploadType.Other;
}
