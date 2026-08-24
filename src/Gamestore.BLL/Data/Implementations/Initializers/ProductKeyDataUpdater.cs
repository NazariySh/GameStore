using Gamestore.BLL.Interfaces.Games;
using Gamestore.DAL.Data;
using Gamestore.DAL.Data.Initializers.Implementations.DataUpdaters;
using Gamestore.Domain.Entities.Games;
using MongoDB.Driver;

namespace Gamestore.BLL.Data.Implementations.Initializers;

public class ProductKeyDataUpdater : BaseMongoDataUpdater<Game>
{
    private readonly IGameKeyGenerator _gameKeyGenerator;

    public ProductKeyDataUpdater(
        NorthwindMongoDbContext dbContext,
        IGameKeyGenerator gameKeyGenerator)
        : base(dbContext)
    {
        _gameKeyGenerator = gameKeyGenerator;
    }

    public override int Order => 5;

    public override async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        var findFilter = Builders<Game>.Filter.Or(
            Builders<Game>.Filter.Eq(g => g.Key, string.Empty),
            Builders<Game>.Filter.Exists(g => g.Key, false));

        var products = await Collection.Find(findFilter).ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            product.Key = await _gameKeyGenerator.GenerateUniqueAsync(product.Name, cancellationToken);

            var filter = Builders<Game>.Filter.Eq(g => g.ObjectId, product.ObjectId);
            var update = Builders<Game>.Update.Set(g => g.Key, product.Key);
            await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }
    }
}