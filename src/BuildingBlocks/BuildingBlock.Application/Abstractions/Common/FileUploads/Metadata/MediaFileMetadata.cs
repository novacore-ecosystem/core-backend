namespace NovaCore.BuildingBlock.Application.Abstractions.Common.FileUploads.Metadata;

/// <summary>Covers both video and audio, same split as MediaUpload - IsVideo distinguishes them rather than two near-identical record types.</summary>
public sealed record MediaFileMetadata(
    bool IsVideo,
    TimeSpan? Duration = null,
    int? Width = null,
    int? Height = null,
    double? FrameRate = null,
    string? Codec = null,
    int? BitrateKbps = null,
    string? Artist = null,
    string? Album = null,
    string? Title = null) : FileMetadata;
