using Gamestore.DAL.Data;
using Gamestore.Domain.Entities.Orders;
using MongoDB.Driver;

namespace Gamestore.DAL.Repositories.Implementations.MongoRepositories;

public class OrderMongoRepository : MongoRepository<Order>
{
    public OrderMongoRepository(NorthwindMongoDbContext dbContext)
        : base(dbContext)
    {
    }

    public override async Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Order>.Filter.Eq(e => e.MongoOrderId, entity.MongoOrderId);
        var update = Builders<Order>.Update
            .Set(e => e.Date, entity.Date)
            .Set(e => e.Status, entity.Status)
            .Set(e => e.ShippedDate, entity.ShippedDate);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}