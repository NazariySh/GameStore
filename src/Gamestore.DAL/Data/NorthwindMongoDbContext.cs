using System.Reflection;
using Gamestore.Domain.Attributes;
using MongoDB.Driver;

namespace Gamestore.DAL.Data;

public class NorthwindMongoDbContext
{
    public NorthwindMongoDbContext(IMongoClient client, string databaseName)
    {
        Client = client;
        Database = client.GetDatabase(databaseName);
    }

    public IMongoClient Client { get; }

    public IMongoDatabase Database { get; }

    public IMongoCollection<TEntity> GetCollection<TEntity>(string name)
        where TEntity : class
    {
        return Database.GetCollection<TEntity>(name);
    }

    public IMongoCollection<TEntity> GetCollection<TEntity>()
        where TEntity : class
    {
        var type = typeof(TEntity);
        var name = type.GetCustomAttribute<BsonCollectionAttribute>()?.CollectionName ?? type.Name;
        return GetCollection<TEntity>(name);
    }
}