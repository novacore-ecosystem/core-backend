namespace NovaCore.Audit.Domain.Entities;

/// <summary>Embedded, optional context captured alongside an audit graph. Every field is additive - a future field is a new nullable property here, never a breaking schema change.</summary>
public sealed class AuditTrailMetadata
{
    public string? Actor { get; private set; }
    public string? Service { get; private set; }
    public string? ClientIp { get; private set; }
    public string? UserAgent { get; private set; }
    public string? BusinessAction { get; private set; }
    public string? Reason { get; private set; }
    public string? RequestPath { get; private set; }
    public string? TraceId { get; private set; }

    private AuditTrailMetadata() { }

    public static AuditTrailMetadata Create(
        string? actor,
        string? service,
        string? clientIp,
        string? userAgent,
        string? businessAction,
        string? reason,
        string? requestPath,
        string? traceId)
    {
        return new AuditTrailMetadata
        {
            Actor = actor,
            Service = service,
            ClientIp = clientIp,
            UserAgent = userAgent,
            BusinessAction = businessAction,
            Reason = reason,
            RequestPath = requestPath,
            TraceId = traceId,
        };
    }
}
