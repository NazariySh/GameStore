using Gamestore.BLL.DTOs.Shippers;

namespace Gamestore.BLL.Interfaces.Shippers;

public interface IShipperService
{
    Task<IReadOnlyList<ShipperDto>> GetAllAsync(CancellationToken cancellationToken = default);
}