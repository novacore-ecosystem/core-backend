namespace NovaCore.Content.Application.Features.Contents.Commands.TranslateContentVersion;

/// <summary>
/// Adds or updates a target language's content on an existing version - the Translation API.
/// Deliberately the same underlying write path as UpdateContentDraft (Content.UpsertLocalization):
/// translating a language that already exists on the version overwrites it, exactly like editing a
/// draft, so there is no second localization mechanism to keep in sync with the first.
/// </summary>
public sealed record TranslateContentVersionCommand(
    Guid ContentId,
    Guid VersionId,
    string TargetLanguage,
    string Title,
    string Summary,
    string Body,
    Guid TranslatedBy) : ICommand<TranslateContentVersionResponse>;

public sealed record TranslateContentVersionResponse(Guid ContentId, Guid VersionId, string TargetLanguage, bool WasExistingLanguage);
