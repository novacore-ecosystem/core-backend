using Microsoft.Extensions.Configuration;

using Serilog;
using Serilog.Enrichers.Span;

namespace NovaCore.BuildingBlock.Observability.Logging;

public static class SerilogBootstrap
{
    public static LoggerConfiguration ConfigureAppLogging(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        string serviceName)
    {
        var seqUrl = configuration["Logging:Seq:Url"] ?? "http://seq:5341";
        var elasticsearchUrl = configuration["Logging:Elasticsearch:Url"];
        var elasticsearchUsername = configuration["Logging:Elasticsearch:Username"];
        var elasticsearchPassword = configuration["Logging:Elasticsearch:Password"];

        loggerConfiguration
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            // Stamps TraceId/SpanId from Activity.Current onto every log event, so a trace in
            // Kibana APM/Observability can be pivoted to its exact log lines in Discover.
            .Enrich.WithSpan()
            .WriteTo.Console()
            .WriteTo.Seq(seqUrl);

        if (!string.IsNullOrWhiteSpace(elasticsearchUrl))
        {
            loggerConfiguration.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticsearchUrl))
            {
                IndexFormat = $"{serviceName}-logs-{{0:yyyy.MM.dd}}",
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,
                // Scope the auto-registered template's name/pattern to this service so multiple
                // services don't fight over the shared default "serilog-events-template" name.
                TemplateName = $"{serviceName}-logs-template",
                TemplateCustomSettings = new Dictionary<string, string>
                {
                    ["index.lifecycle.name"] = "logs-ilm-policy"
                },
                ModifyConnectionSettings = !string.IsNullOrWhiteSpace(elasticsearchUsername) && !string.IsNullOrWhiteSpace(elasticsearchPassword)
                    ? conn => conn.BasicAuthentication(elasticsearchUsername, elasticsearchPassword)
                    : null
            });
        }

        return loggerConfiguration;
    }
}
