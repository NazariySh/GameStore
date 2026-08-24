using Gamestore.DAL.Data;
using Gamestore.Domain.Entities.Logging;
using MongoDB.Driver;

namespace Gamestore.DAL.Repositories.Implementations.MongoRepositories;

public class EntityChangeLogMongoRepository : MongoRepository<EntityChangeLog>
{
    public EntityChangeLogMongoRepository(NorthwindMongoDbContext dbContext)
        : base(dbContext)
    {
    }

    public override async Task UpdateAsync(EntityChangeLog entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<EntityChangeLog>.Filter.Eq(e => e.ObjectId, entity.ObjectId);
        var update = Builders<EntityChangeLog>.Update
            .Set(e => e.Date, entity.Date)
            .Set(e => e.Action, entity.Action)
            .Set(e => e.EntityType, entity.EntityType)
            .Set(e => e.OldVersion, entity.OldVersion)
            .Set(e => e.NewVersion, entity.NewVersion);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}