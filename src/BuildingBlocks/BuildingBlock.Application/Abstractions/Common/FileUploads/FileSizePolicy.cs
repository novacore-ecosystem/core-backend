namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads;

/// <summary>
/// The two-tier size limit FileUploadClassifier validates against. AllowedSizeBytes is a business
/// rule the calling use case owns and may legitimately override per call (e.g. a CS-upload flow
/// allowing larger files than a customer-facing one) - never hard-coded inside FileUpload itself.
/// MaxParseSizeBytes is an infrastructure safety ceiling (see FileParsingOptions) that callers
/// source from configuration, not invent ad hoc - a file above it is never opened for content
/// inspection, regardless of what any business rule allows.
/// </summary>
public sealed record FileSizePolicy(long AllowedSizeBytes, long MaxParseSizeBytes);
