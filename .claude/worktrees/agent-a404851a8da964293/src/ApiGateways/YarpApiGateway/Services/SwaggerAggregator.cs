using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.YarpApiGateway.Configuration;

namespace NovaCore.YarpApiGateway.Services;

public interface ISwaggerAggregator
{
    Task<string> GetAggregatedSwaggerAsync(CancellationToken ct = default);

    Task ServeSwaggerIndexAsync(HttpContext context);
}

public sealed class SwaggerAggregator(GatewayOptions options, IHttpClientFactory httpClientFactory) : ISwaggerAggregator
{
    private readonly GatewayOptions _options = options;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<string> GetAggregatedSwaggerAsync(CancellationToken ct = default)
    {
        var specs = new Dictionary<string, object>();

        foreach (var service in _options.Services.Values.Where(s => !string.IsNullOrEmpty(s.SwaggerUrl)))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(service.SwaggerUrl, ct);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    specs[service.Name] = content;
                }
            }
            catch
            {
                // Log and continue if one service fails
            }
        }

        return System.Text.Json.JsonSerializer.Serialize(specs);
    }

    public async Task ServeSwaggerIndexAsync(HttpContext context)
    {
        var gatewayUrl = "http://localhost:5000";
        var urlsJson = _options.Services.Values
            .Where(s => s.SwaggerUrl.IsNotNullOrWhiteSpace())
            .Select(s => $"{{ url: '{gatewayUrl}{s.Path}{s.SwaggerUrl}', name: '{s.Name}' }}")
            .JoinToString($",{Environment.NewLine}");

        var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>NovaCore API Gateway - All APIs</title>
                <meta charset='utf-8' />
                <meta name='viewport' content='width=device-width, initial-scale=1' />
                <link rel='stylesheet' href='https://unpkg.com/swagger-ui-dist@5.11.0/swagger-ui.css' />
                <style>
                    .topbar {{ background-color: #1e90ff; }}
                    .info .title {{ color: #1e90ff; }}
                </style>
            </head>
            <body>
                <div id='swagger-ui'></div>
                <script src='https://unpkg.com/swagger-ui-dist@5.11.0/swagger-ui-bundle.js' crossorigin></script>
                <script src='https://unpkg.com/swagger-ui-dist@5.11.0/swagger-ui-standalone-preset.js' crossorigin></script>
                <script>
                    document.addEventListener('DOMContentLoaded', (e) => {{
                        const ui = SwaggerUIBundle({{
                            urls: [
                                {urlsJson}
                            ],
                            urlsPrimaryName: 'Auth',
                            dom_id: '#swagger-ui',
                            presets: [
                                SwaggerUIBundle.presets.apis,
                                SwaggerUIStandalonePreset
                            ],
                            layout: 'StandaloneLayout',
                            deepLinking: true,
                            onFailure: (error) => {{
                                console.error('Swagger UI failed to load:', error);
                            }}
                        }});
                        window.ui = ui;
                    }});
                </script>
            </body>
            </html>";

        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(html);
    }
}
