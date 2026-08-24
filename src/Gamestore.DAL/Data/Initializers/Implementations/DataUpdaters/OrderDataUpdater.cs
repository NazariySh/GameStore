using Gamestore.Domain.Enums;
using MongoDB.Driver;
using OrderEntity = Gamestore.Domain.Entities.Orders.Order;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataUpdaters;

public class OrderDataUpdater : BaseMongoDataUpdater<OrderEntity>
{
    public OrderDataUpdater(NorthwindMongoDbContext dbContext)
        : base(dbContext)
    {
    }

    public override async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        await ConvertStringFieldToDateTimeAsync("OrderDate", cancellationToken);

        await UpdateOrdersAsync(cancellationToken);
    }

    private async Task UpdateOrdersAsync(CancellationToken cancellationToken)
    {
        var findFilter = Builders<OrderEntity>.Filter.Exists(e => e.Status, false);

        var orders = await Collection.Find(findFilter).ToListAsync(cancellationToken);

        foreach (var order in orders)
        {
            order.Status = OrderStatus.Shipped;

            var filter = Builders<OrderEntity>.Filter.Eq(e => e.ObjectId, order.ObjectId);
            var update = Builders<OrderEntity>.Update.Set(e => e.Status, order.Status);
            await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }
    }
}