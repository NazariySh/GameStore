using Gamestore.BLL.DTOs.Orders;

namespace Gamestore.BLL.Interfaces.Orders;

public interface ICartService
{
    Task<IReadOnlyList<OrderGameDto>> GetCartItemsAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task AddAsync(string gameKey, Guid customerId, CancellationToken cancellationToken = default);

    Task RemoveAsync(string gameKey, Guid customerId, CancellationToken cancellationToken = default);
}