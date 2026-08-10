namespace NovaCore.User.Application.Features.Users.Commands.RebuildUserSearchIndex;

public sealed record RebuildUserSearchIndexCommand : ICommand<RebuildUserSearchIndexResponse>;

public sealed record RebuildUserSearchIndexResponse(int UsersIndexed);
