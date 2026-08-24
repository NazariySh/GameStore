using Gamestore.BLL.DTOs.Shippers;
using Gamestore.BLL.Interfaces.Shippers;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Shippers;

public class ShippersController : BaseApiController
{
    private readonly IShipperService _shipperService;

    public ShippersController(IShipperService shipperService)
    {
        _shipperService = shipperService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ShipperDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _shipperService.GetAllAsync(cancellationToken));
    }
}