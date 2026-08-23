namespace NovaCore.Content.Application.Features.Contents.Commands.DeleteContent;

public sealed record DeleteContentCommand(Guid ContentId) : ICommand<DeleteContentResponse>;

public sealed record DeleteContentResponse(Guid ContentId, DateTime DeletedAt);
