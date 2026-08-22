namespace NovaCore.Content.Application.Features.Contents.Commands.CreateContent;

public sealed record CreateContentCommand(
    Guid ContentTypeId,
    string Slug,
    string Title,
    string Summary,
    string Body,
    Guid CreatedBy,
    ContentVisibility Visibility = ContentVisibility.Private) : ICommand<CreateContentResponse>;

public sealed record CreateContentResponse(Guid ContentId, Guid VersionId);
