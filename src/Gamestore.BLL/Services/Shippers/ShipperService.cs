using Gamestore.BLL.DTOs.Shippers;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Shippers;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Shippers;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Shippers;

public class ShipperService : IShipperService
{
    private readonly IRepository<Shipper> _shipperRepository;
    private readonly ILogger<ShipperService> _logger;

    public ShipperService(
        IServiceContext context,
        ILogger<ShipperService> logger)
    {
        var unitOfWork = context.UnitOfWork;
        _shipperRepository = unitOfWork.Repositories.GetGeneric<Shipper>();
        _logger = logger;
    }

    public async Task<IReadOnlyList<ShipperDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all shippers");

        var shippers = await _shipperRepository.GetAllAsync<ShipperDto>(cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} shippers from database", shippers.Count);

        return shippers;
    }
}