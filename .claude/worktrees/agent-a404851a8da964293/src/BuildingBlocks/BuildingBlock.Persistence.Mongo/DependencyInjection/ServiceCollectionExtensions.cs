using NovaCore.BuildingBlock.Persistence.Mongo.MongoContext;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace NovaCore.BuildingBlock.Persistence.Mongo.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private static bool _conventionsRegistered;

    /// <summary>
    /// Registers the Mongo client, database and context as singletons.
    /// Unlike AddPersistenceDbContext&lt;TContext&gt;'s Scoped EF DbContext, TContext here is
    /// registered as a Singleton: the Mongo driver's client/database/collection handles are
    /// stateless and thread-safe by design, and there is no per-request change tracker to
    /// isolate between requests the way EF's DbContext requires.
    /// </summary>
    public static IServiceCollection AddPersistenceMongoContext<TContext>(
        this IServiceCollection services,
        string connectionString,
        string databaseName)
        where TContext : MongoContextBase
    {
        RegisterConventionsOnce();

        services.AddSingleton<IMongoClient>(_ =>
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            // Emits an Activity per Mongo command via MongoDB.Driver.Core.Extensions.DiagnosticSources
            // - matched on the export side by MongoTracingExtensions.AddMongoTracing().
            settings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
            return new MongoClient(settings);
        });
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
        services.AddSingleton<TContext>();

        return services;
    }

    /// <summary>
    /// Element names default to camelCase across every Mongo-backed document (Outbox/Inbox and
    /// any per-service entity) - EF's equivalent is UseSnakeCaseNamingConvention() in
    /// AddPersistenceDbContext. Also pins Guid serialization to the modern "Standard" (UUID
    /// subtype 4) representation - the driver otherwise defaults to GuidRepresentation
    /// .Unspecified and throws on any Guid property (every Id in this codebase is a Guid), since
    /// legacy BSON UUID subtypes are ambiguous across drivers/languages. Registration is
    /// process-global and idempotent by design in the driver, but guarded here anyway since
    /// multiple services' DI containers may call this in the same test host.
    /// </summary>
    private static void RegisterConventionsOnce()
    {
        if (_conventionsRegistered)
            return;

        var pack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("NovaCore Camel Case", pack, _ => true);

        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonSerializer.RegisterSerializer(new NullableSerializer<Guid>(new GuidSerializer(GuidRepresentation.Standard)));

        _conventionsRegistered = true;
    }
}
