using Gamestore.Domain.Entities.Games;
using MongoDB.Driver;

namespace Gamestore.DAL.Data.Initializers.Implementations.DataUpdaters;

public class ProductDataUpdater : BaseMongoDataUpdater<Game>
{
    public ProductDataUpdater(NorthwindMongoDbContext dbContext)
        : base(dbContext)
    {
    }

    public override int Order => 2;

    public override async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        var findFilter = Builders<Game>.Filter.Exists(e => e.CreatedAt, false);

        var products = await Collection.Find(findFilter).ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            var filter = Builders<Game>.Filter.Eq(p => p.ObjectId, product.ObjectId);
            var update = Builders<Game>.Update
                .Set(p => p.CreatedAt, DateTime.UtcNow)
                .Set(p => p.ViewCount, 0)
                .Set(p => p.CommentCount, 0);

            await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }
    }
}