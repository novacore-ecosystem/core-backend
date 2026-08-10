namespace NovaCore.User.Application.Features.Users.Queries.GetUserById;

/// <summary>Backs the gRPC GetUser RPC. GetUserByIdHandler explicitly calls IUserProfileDetailCache (read-through: cache -&gt; DB -&gt; refresh cache -&gt; return), not a decorator behind IUserReadService.</summary>
public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserLookupResult?>;
