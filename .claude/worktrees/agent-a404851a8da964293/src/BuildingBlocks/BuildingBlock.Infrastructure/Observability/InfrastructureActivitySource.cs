using System.Diagnostics;

namespace NovaCore.BuildingBlock.Infrastructure.Observability;

/// <summary>
/// Manual instrumentation source for cross-cutting infrastructure that has no HTTP request to
/// hang a span off - background polling loops (Outbox relay, Inbox retry), etc. Registered for
/// export via <see cref="InfrastructureTracingExtensions.AddInfrastructureTracing"/>.
/// </summary>
public static class InfrastructureActivitySource
{
    public const string Name = "NovaCore.Infrastructure";

    public static readonly ActivitySource Instance = new(Name, "1.0.0");
}
