using Gamestore.DAL.Data;
using Gamestore.Domain.Entities.Games;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gamestore.DAL.Repositories.Implementations.MongoRepositories;

public class SupplierMongoRepository : MongoRepository<Publisher>
{
    public SupplierMongoRepository(NorthwindMongoDbContext dbContext)
        : base(dbContext)
    {
    }

    public override async Task UpdateAsync(Publisher entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Publisher>.Filter.Eq(e => e.SupplierId, entity.SupplierId);
        var update = Builders<Publisher>.Update
            .Set(e => e.CompanyName, entity.CompanyName)
            .Set(e => e.HomePage, entity.HomePage)
            .Set(e => e.Description, entity.Description);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public override async Task RemoveAsync(Publisher entity, CancellationToken cancellationToken = default)
    {
        var delete = new BsonDocument
        {
            { "delete", Collection.CollectionNamespace.CollectionName },
            {
                "deletes",
                new BsonArray
                {
                    new BsonDocument
                    {
                        { "q", new BsonDocument { { "CompanyName", entity.CompanyName } } },
                        { "limit", 1 },
                    },
                }
            },
        };

        await DbContext.Database.RunCommandAsync<BsonDocument>(delete, cancellationToken: cancellationToken);
    }
}