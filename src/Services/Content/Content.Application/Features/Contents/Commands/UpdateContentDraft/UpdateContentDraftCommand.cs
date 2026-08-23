namespace NovaCore.Content.Application.Features.Contents.Commands.UpdateContentDraft;

/// <summary>Updates a language's content on a specific (non-published, non-archived) version - the
/// application-facing entry point onto Content.UpsertLocalization for ordinary draft editing.</summary>
public sealed record UpdateContentDraftCommand(
    Guid ContentId,
    Guid VersionId,
    string Language,
    string Title,
    string Summary,
    string Body,
    Guid UpdatedBy) : ICommand<UpdateContentDraftResponse>;

public sealed record UpdateContentDraftResponse(Guid ContentId, Guid VersionId, string Language);
