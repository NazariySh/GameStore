using Gamestore.DAL.Data.Initializers.Interfaces;
using Gamestore.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataUpdaters;

public abstract class BaseMongoDataUpdater<TEntity> : IMongoDataUpdater
    where TEntity : BaseEntity
{
    protected BaseMongoDataUpdater(NorthwindMongoDbContext dbContext)
    {
        Collection = dbContext.GetCollection<TEntity>();
    }

    public virtual int Order => 1;

    protected IMongoCollection<TEntity> Collection { get; }

    public abstract Task UpdateAsync(CancellationToken cancellationToken = default);

    protected async Task ConvertStringFieldToDateTimeAsync(string fieldName, CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Type(fieldName, BsonType.String);
        PipelineDefinition<TEntity, TEntity> pipeline = new BsonDocument[]
        {
            new("$set", new BsonDocument
            {
                { fieldName, new BsonDocument("$toDate", $"${fieldName}") },
            }),
        };

        await Collection.UpdateManyAsync(filter, pipeline, cancellationToken: cancellationToken);
    }
}