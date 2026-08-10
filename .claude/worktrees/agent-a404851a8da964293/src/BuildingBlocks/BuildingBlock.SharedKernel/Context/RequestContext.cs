namespace NovaCore.BuildingBlock.SharedKernel.Context;

/// <summary>
/// Ambient accessor for the current request's identity. Not a DbContext, not an application
/// service, not a DI-resolved abstraction, not a service locator, not a general-purpose mutable
/// state container - it exists purely so that framework components which cannot (or should not)
/// take a DI dependency on HttpContext - EF interceptors, model-building conventions, Mapster
/// profiles, Outbox/Inbox, Audit - can still read who/what a unit of work is running for.
/// Consumers only ever read <see cref="Current"/>; the AsyncLocal storage underneath is an
/// implementation detail. Only RequestContextMiddleware is allowed to call
/// <see cref="Initialize"/>/<see cref="Clear"/> - see that type, and
/// docs/reference/request-context.md, for why.
/// </summary>
public static class RequestContext
{
    private static readonly AsyncLocal<RequestContextData?> Storage = new();

    private static readonly RequestContextData Anonymous = new() { CorrelationId = string.Empty };

    public static RequestContextData Current => Storage.Value ?? Anonymous;

    /// <summary>Framework-only initialization point - called exclusively by RequestContextMiddleware,
    /// once per request, before any other component in the pipeline runs. Never call this from
    /// application code, a handler, or a background job; there is deliberately no public
    /// update/mutate method, since request identity is read-only for the rest of the request's
    /// lifetime. Must always be paired with <see cref="Clear"/> once the request finishes.</summary>
    public static void Initialize(RequestContextData data) => Storage.Value = data;

    /// <summary>Framework-only cleanup point - called exclusively by RequestContextMiddleware in a
    /// `finally` block, guaranteeing no request's identity can ever be observed by a later request.
    /// AsyncLocal already scopes per logical call chain, so this is defense-in-depth rather than a
    /// strict technical requirement - the framework does not rely on that alone.</summary>
    public static void Clear() => Storage.Value = null;
}
