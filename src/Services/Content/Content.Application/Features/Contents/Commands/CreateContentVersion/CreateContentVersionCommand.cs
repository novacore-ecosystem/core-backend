namespace NovaCore.Content.Application.Features.Contents.Commands.CreateContentVersion;

public sealed record CreateContentVersionCommand(
    Guid ContentId,
    string Language,
    string Title,
    string Summary,
    string Body,
    Guid CreatedBy) : ICommand<CreateContentVersionResponse>;

public sealed record CreateContentVersionResponse(Guid ContentId, Guid VersionId, int VersionNumber);
