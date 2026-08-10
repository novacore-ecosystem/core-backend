namespace NovaCore.BuildingBlock.Web;

/// <summary>
/// Shared settings consumed by the individual NovaCore.BuildingBlock.Web Add*/Use* extensions.
/// Each service composes only the extensions it needs and passes the slice of this
/// options object that extension requires — there is no single "call everything" method,
/// since not every future service will need every piece (Swagger/Carter/JWT/etc.).
/// </summary>
public sealed class BuildingBlockWebOptions
{
    // ---- Swagger (NovaCore.BuildingBlock.Web.Swagger) ----
    public required string ServiceTitle { get; init; }
    public required string ServiceDescription { get; init; }
    public required string SwaggerRoutePrefix { get; init; }
    public required string ContactUrl { get; init; }

    /// <summary>
    /// Short name shown in the Swagger UI version dropdown, e.g. "Auth Service".
    /// </summary>
    public required string SwaggerUiTitle { get; init; }
    public bool IncludeJwtBearerSwaggerAuth { get; init; }

    // ---- CORS (NovaCore.BuildingBlock.Web.Cors) ----

    /// <summary>
    /// CORS policy name registered via AddCors.
    /// </summary>
    public string CorsPolicyName { get; init; } = "AllowAll";

    /// <summary>
    /// CORS policy name applied via UseCors. Defaults to <see cref="CorsPolicyName"/>;
    /// override only to preserve a pre-existing mismatch between registration and usage.
    /// </summary>
    public string? CorsAppliedPolicyName { get; init; }
}
