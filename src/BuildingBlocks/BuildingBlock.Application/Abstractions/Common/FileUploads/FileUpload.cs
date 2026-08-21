namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;

/// <summary>
/// A file already uploaded elsewhere (client-side direct-to-storage upload) and referenced here by
/// an opaque pointer - same "no binary content, only a storage pointer" doctrine as Chat's
/// MessageAttachment/Promotion's CampaignAttachment. Reusable across features that accept
/// attachments; classify raw upload metadata into one of the sealed subtypes via
/// FileUploadClassifier rather than constructing these directly.
/// </summary>
public abstract record FileUpload(string FileName, string ContentType, long SizeBytes, string StorageKey)
{
    public abstract FileUploadType Type { get; }
}
