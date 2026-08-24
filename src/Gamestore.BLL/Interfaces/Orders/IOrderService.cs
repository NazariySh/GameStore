using Gamestore.BLL.DTOs.Orders;

namespace Gamestore.BLL.Interfaces.Orders;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> GetOrdersHistoryAsync(OrderQueryDto query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderDto>> GetPaidAndCancelledOrdersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderGameDto>> GetOrderGamesAsync(string orderId, CancellationToken cancellationToken = default);

    Task<OrderDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task ShipOrderAsync(string orderId, CancellationToken cancellationToken = default);

    Task AddOrderGameAsync(string orderId, string gameKey, CancellationToken cancellationToken = default);

    Task UpdateOrderGameQuantityAsync(Guid orderGameId, UpdateQuantityRequest request, CancellationToken cancellationToken = default);

    Task DeleteOrderGameAsync(Guid orderGameId, CancellationToken cancellationToken = default);
}