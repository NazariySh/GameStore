using Gamestore.DAL.Data;
using Gamestore.Domain.Entities.Shippers;
using MongoDB.Driver;

namespace Gamestore.DAL.Repositories.Implementations.MongoRepositories;

public class ShipperMongoRepository : MongoRepository<Shipper>
{
    public ShipperMongoRepository(NorthwindMongoDbContext dbContext)
        : base(dbContext)
    {
    }

    public override async Task UpdateAsync(Shipper entity, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Shipper>.Filter.Eq(e => e.ShipperId, entity.ShipperId);
        var update = Builders<Shipper>.Update
            .Set(e => e.CompanyName, entity.CompanyName)
            .Set(e => e.Phone, entity.Phone);

        await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }
}