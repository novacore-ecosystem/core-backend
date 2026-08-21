namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads.Metadata;

public sealed record ImageFileMetadata(
    int? Width = null,
    int? Height = null,
    string? Orientation = null,
    string? ColorSpace = null,
    string? CameraMake = null,
    string? CameraModel = null,
    DateTime? TakenAt = null,
    double? GpsLatitude = null,
    double? GpsLongitude = null) : FileMetadata;
