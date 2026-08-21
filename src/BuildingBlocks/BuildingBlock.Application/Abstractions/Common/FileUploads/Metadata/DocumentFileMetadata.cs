namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads.Metadata;

public sealed record DocumentFileMetadata(
    int? PageCount = null,
    string? Author = null,
    string? Title = null,
    DateTime? CreatedAt = null,
    string? Producer = null) : FileMetadata;
