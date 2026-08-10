using NovaCore.BuildingBlock.Search.Abstractions;
using NovaCore.BuildingBlock.Search.Configuration;
using NovaCore.BuildingBlock.Search.Indexing;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NovaCore.BuildingBlock.Search.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a singleton ElasticsearchClient (thread-safe/stateless, same registration
    /// lifetime rationale as NovaCore.BuildingBlock.Persistence.Mongo's IMongoClient) plus the generic
    /// IElasticsearchIndexer&lt;&gt; - the only reusable piece; document shape, mapping, and
    /// query DSL stay in each service's own Persistence project.
    /// </summary>
    public static IServiceCollection AddElasticsearchClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.Url))
                throw new InvalidOperationException($"Configuration '{ElasticsearchOptions.SectionName}:Url' not found.");

            var settings = new ElasticsearchClientSettings(new Uri(options.Url))
                .MaximumRetries(options.MaxRetries)
                .RequestTimeout(TimeSpan.FromSeconds(options.RequestTimeoutSeconds));

            if (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password))
            {
                settings.Authentication(new BasicAuthentication(options.Username, options.Password));
            }

            return new ElasticsearchClient(settings);
        });

        services.AddSingleton(typeof(IElasticsearchIndexer<>), typeof(ElasticsearchIndexer<>));

        return services;
    }
}
