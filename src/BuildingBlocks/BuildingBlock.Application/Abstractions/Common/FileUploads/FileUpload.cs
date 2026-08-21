using NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads.Metadata;

namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;

/// <summary>
/// A file already uploaded elsewhere (client-side direct-to-storage upload) and referenced here by
/// an opaque pointer - same "no binary content, only a storage pointer" doctrine as Chat's
/// MessageAttachment/Promotion's CampaignAttachment. Reusable across features that accept
/// attachments; classify raw upload metadata into one of the sealed subtypes via
/// FileUploadClassifier rather than constructing these directly.
///
/// ValidationState/ValidationDetails reflect what's knowable from declared metadata alone
/// (FileUploadClassifier sets these synchronously, before any I/O) - Metadata is filled in later
/// by a separate, explicit, async content-inspection step (see IFileMetadataExtractor) once a
/// caller actually has the file's bytes open, folded back in via `upload with { Metadata = ...,
/// ValidationState = ... }`. There is no single "construction" moment that does both: cheap
/// declared-metadata checks happen immediately, expensive/dangerous content parsing stays a
/// deliberate opt-in step gated by FileSizePolicy.MaxParseSizeBytes.
/// </summary>
public abstract record FileUpload(string FileName, string ContentType, long SizeBytes, string StorageKey)
{
    public abstract FileUploadType Type { get; }

    public FileUploadValidationState ValidationState { get; init; } = FileUploadValidationState.Valid;

    public IReadOnlyList<string> ValidationDetails { get; init; } = [];

    /// <summary>Populated only after an explicit IFileMetadataExtractor.ExtractAsync call - null until then, always null if ValidationState is Invalid due to exceeding MaxParseSizeBytes (the file is never opened in that case).</summary>
    public FileMetadata? Metadata { get; init; }

    /// <summary>Convenience for size-based FluentValidation rules - SizeBytes remains the precise source value.</summary>
    public double SizeMegabytes => SizeBytes / (1024.0 * 1024.0);
}
