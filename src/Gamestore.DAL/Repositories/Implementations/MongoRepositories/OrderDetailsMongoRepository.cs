using Gamestore.DAL.Data;
using Gamestore.Domain.Entities.Orders;
using MongoDB.Driver;

namespace Gamestore.DAL.Repositories.Implementations.MongoRepositories;

public class OrderDetailsMongoRepository : MongoRepository<OrderGame>
{
    public OrderDetailsMongoRepository(NorthwindMongoDbContext dbContext)
        : base(dbContext)
    {
    }

    public override async Task UpdateAsync(OrderGame entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<OrderGame>.Filter.And(
            Builders<OrderGame>.Filter.Eq(e => e.MongoOrderId, entity.MongoOrderId),
            Builders<OrderGame>.Filter.Eq(e => e.MongoProductId, entity.MongoProductId));

        var update = Builders<OrderGame>.Update
            .Set(e => e.Price, entity.Price)
            .Set(e => e.Quantity, entity.Quantity)
            .Set(e => e.Discount, entity.Discount);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}