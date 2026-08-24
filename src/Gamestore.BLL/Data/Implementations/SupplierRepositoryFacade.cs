using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Data.Implementations;

public class SupplierRepositoryFacade : RepositoryFacade<Publisher>
{
    public SupplierRepositoryFacade(
        ISqlRepository<Publisher> sqlRepository,
        IMongoRepository<Publisher> mongoRepository)
        : base(sqlRepository, mongoRepository)
    {
    }

    protected override ICollection<Publisher> MergeCollections(
        IEnumerable<Publisher> firstCollection,
        IEnumerable<Publisher> secondCollection)
    {
        return firstCollection.Concat(secondCollection)
            .DistinctBy(s => s.CompanyName)
            .ToList();
    }
}