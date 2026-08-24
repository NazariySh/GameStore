using Gamestore.DAL.Data;
using Gamestore.Domain.Entities.Games;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gamestore.DAL.Repositories.Implementations.MongoRepositories;

public class CategoryMongoRepository : MongoRepository<Genre>
{
    public CategoryMongoRepository(NorthwindMongoDbContext dbContext)
        : base(dbContext)
    {
    }

    public override async Task UpdateAsync(Genre entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Genre>.Filter.Eq(e => e.CategoryId, entity.CategoryId);
        var update = Builders<Genre>.Update
            .Set(e => e.Name, entity.Name)
            .Set(e => e.ParentGenreId, entity.ParentGenreId);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public override async Task RemoveAsync(Genre entity, CancellationToken cancellationToken = default)
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
                        { "q", new BsonDocument { { "CategoryName", entity.Name } } },
                        { "limit", 1 },
                    },
                }
            },
        };

        await DbContext.Database.RunCommandAsync<BsonDocument>(delete, cancellationToken: cancellationToken);
    }
}