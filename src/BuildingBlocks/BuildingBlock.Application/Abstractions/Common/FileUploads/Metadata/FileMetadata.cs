namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads.Metadata;

/// <summary>
/// Marker base for the normalized, extracted metadata of a FileUpload - one sealed record per
/// media family (ImageFileMetadata/MediaFileMetadata/DocumentFileMetadata), each carrying only the
/// properties genuinely specific to that family. Plain, System.Text.Json-serializable records with
/// no extractor/storage coupling, so a consuming feature can persist one directly as a jsonb column
/// on its own entity without inventing a new pattern.
/// </summary>
public abstract record FileMetadata;
