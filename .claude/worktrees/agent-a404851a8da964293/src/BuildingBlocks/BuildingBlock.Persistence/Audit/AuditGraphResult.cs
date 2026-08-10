using NovaCore.BuildingBlock.Contract.Events.Audit;

namespace NovaCore.BuildingBlock.Persistence.Audit;

/// <summary>One resolved audit graph - exactly one of these is produced per changed Aggregate Root instance.</summary>
public sealed record AuditGraphResult(string RootEntityType, string RootEntityId, AuditNode Root);
