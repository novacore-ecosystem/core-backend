namespace NovaCore.Auth.Application.Features.Apps.Commands.UpsertAppTranslation;

public sealed record UpsertAppTranslationCommand(
    Guid AppId,
    string LanguageCode,
    string DisplayName,
    string? Description) : ICommand;
