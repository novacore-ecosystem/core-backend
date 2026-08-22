namespace NovaCore.Content.Application.Features.ContentTypes.Commands.CreateContentType;

public sealed record CreateContentTypeCommand(
    string Key,
    string Name,
    string Description) : ICommand<CreateContentTypeResponse>;

public sealed record CreateContentTypeResponse(Guid ContentTypeId);
