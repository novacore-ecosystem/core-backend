namespace NovaCore.Content.Application.Features.Contents.Commands.RestoreContent;

public sealed record RestoreContentCommand(Guid ContentId) : ICommand<RestoreContentResponse>;

public sealed record RestoreContentResponse(Guid ContentId);
