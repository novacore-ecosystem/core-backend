using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Audit.Application.Features.AuditLogs.Queries.ListAuditLogs;

namespace NovaCore.Audit.API.Endpoints;

public sealed class ListAuditLogsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## List Audit Logs",
        "",
        "Paginated, filterable list of recorded audit log entries. Audit data is sensitive - admin only.",
        "",
        "### Query Parameters",
        "- **service**: Filter by originating service name (optional, e.g. \"Product\", \"Order\", \"User\")",
        "- **from** / **to**: Filter by event timestamp range (optional, ISO-8601)",
        "- **page**: Page number, 1-based (default 1)",
        "- **pageSize**: Items per page (default 20)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/audit-logs", Handle)
            .WithTags("Audit")
            .RequirePermissions(Permissions.Audit.View)
            .WithName("ListAuditLogs")
            .WithDisplayName("List Audit Logs API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<AuditLogSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromQuery] string? service,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListAuditLogsQuery(
            service,
            from,
            to,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<AuditLogSummaryResponse>>.Ok(response));
    }
}
