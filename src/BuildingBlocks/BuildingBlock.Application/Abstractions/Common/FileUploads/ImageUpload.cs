namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;

public sealed record ImageUpload(
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey,
    int? Width = null,
    int? Height = null) : FileUpload(FileName, ContentType, SizeBytes, StorageKey)
{
    public override FileUploadType Type => FileUploadType.Image;
}
