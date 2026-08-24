using Gamestore.DAL.Repositories.Interfaces.Games;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Data.Interfaces;

public interface IProductRepositoryFacade : IRepositoryFacade<Game>, IGameRepository
{
    Task RecalculateUnitInStockCountAsync(ICollection<string> productKeys, CancellationToken cancellationToken = default);
}