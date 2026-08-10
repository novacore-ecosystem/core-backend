using Microsoft.AspNetCore.Builder;

namespace NovaCore.BuildingBlock.Infrastructure.Idempotency;

public static class IdempotencyEndpointExtensions
{
    /// <summary>Marks the endpoint as requiring an Idempotency-Key; enforced by <see cref="IdempotencyMiddleware"/> once <c>UseIdempotency()</c> is registered.</summary>
    public static TBuilder RequireIdempotency<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithMetadata(new IdempotencyMetadata());

        return builder;
    }
}
