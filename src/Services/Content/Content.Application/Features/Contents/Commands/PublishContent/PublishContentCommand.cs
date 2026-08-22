namespace NovaCore.Content.Application.Features.Contents.Commands.PublishContent;

public sealed record PublishContentCommand(Guid ContentId, Guid VersionId) : ICommand<PublishContentResponse>;

public sealed record PublishContentResponse(Guid ContentId, Guid VersionId, DateTime PublishedAt);
