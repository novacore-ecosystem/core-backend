namespace NovaCore.Content.Application.Features.Contents.Commands.RestoreContentVersion;

/// <summary>Restores a prior version's full language set into a brand-new current version -
/// history is never overwritten, a restore always creates a new version.</summary>
public sealed record RestoreContentVersionCommand(Guid ContentId, Guid VersionId, Guid RestoredBy) : ICommand<RestoreContentVersionResponse>;

public sealed record RestoreContentVersionResponse(Guid ContentId, Guid VersionId, int VersionNumber);
