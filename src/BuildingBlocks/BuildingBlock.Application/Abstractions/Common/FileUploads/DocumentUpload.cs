namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;

public sealed record DocumentUpload(
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey) : FileUpload(FileName, ContentType, SizeBytes, StorageKey)
{
    public override FileUploadType Type => FileUploadType.Document;
}
