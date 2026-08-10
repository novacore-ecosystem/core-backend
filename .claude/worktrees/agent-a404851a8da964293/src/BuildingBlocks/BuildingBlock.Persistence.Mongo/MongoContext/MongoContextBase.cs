using MongoDB.Driver;

namespace NovaCore.BuildingBlock.Persistence.Mongo.MongoContext;

/// <summary>
/// Thin Mongo equivalent of NovaCore.BuildingBlock.Persistence.Ef's DbContextBase.
/// Mongo has no per-request change tracker, so this only exposes the database handle
/// collections are pulled from by derived contexts.
/// </summary>
public abstract class MongoContextBase(IMongoDatabase database)
{
    protected IMongoDatabase Database { get; } = database;
}
