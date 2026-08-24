using System.Linq.Expressions;
using Gamestore.DAL.Data;
using Gamestore.DAL.Repositories.Interfaces.Games;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Games;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gamestore.DAL.Repositories.Implementations.MongoRepositories;

public class ProductMongoRepository : MongoRepository<Game>, IGameRepository
{
    private readonly IUserContext _userContext;

    public ProductMongoRepository(
        NorthwindMongoDbContext dbContext,
        IUserContext userContext)
        : base(dbContext)
    {
        _userContext = userContext;
    }

    public override async Task UpdateAsync(Game entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Game>.Filter.Eq(e => e.ProductId, entity.ProductId);
        var update = Builders<Game>.Update
            .Set(e => e.Name, entity.Name)
            .Set(e => e.Key, entity.Key)
            .Set(e => e.Description, entity.Description)
            .Set(e => e.Price, entity.Price)
            .Set(e => e.UnitInStock, entity.UnitInStock)
            .Set(e => e.Discount, entity.Discount)
            .Set(e => e.ViewCount, entity.ViewCount)
            .Set(e => e.CommentCount, entity.CommentCount);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public override async Task RemoveAsync(Game entity, CancellationToken cancellationToken = default)
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
                        { "q", new BsonDocument { { "Key", entity.Key } } },
                        { "limit", 1 },
                    },
                }
            },
        };

        await DbContext.Database.RunCommandAsync<BsonDocument>(delete, cancellationToken: cancellationToken);
    }

    public async Task SoftDeleteAsync(Game game, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Game>.Filter.Eq(e => e.Key, game.Key);
        var update = Builders<Game>.Update
            .Set(e => e.IsDeleted, true)
            .Set(e => e.ImageUrl, null);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task UpdateUnitInStockAsync(Game game, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Game>.Filter.Eq(e => e.Key, game.Key);
        var update = Builders<Game>.Update.Set(e => e.UnitInStock, game.UnitInStock);
        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task UpdateViewCountAsync(Game game, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Game>.Filter.Eq(e => e.Key, game.Key);
        var update = Builders<Game>.Update.Set(e => e.ViewCount, game.ViewCount);
        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task UpdateCommentCountAsync(Game game, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Game>.Filter.Eq(e => e.Key, game.Key);
        var update = Builders<Game>.Update.Set(e => e.CommentCount, game.CommentCount);
        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    protected override Expression<Func<Game, bool>> SetGlobalFilter()
    {
        return g =>
            !g.IsDeleted ||
            _userContext.HasPermission(Permissions.ViewDeletedGames);
    }
}