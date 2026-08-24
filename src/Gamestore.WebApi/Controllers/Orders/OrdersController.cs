using Gamestore.BLL.DTOs.Orders;
using Gamestore.BLL.Interfaces.Orders;
using Gamestore.WebApi.Constants;
using Gamestore.WebApi.Models.Orders;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Gamestore.WebApi.Controllers.Orders;

[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly IMapper _mapper;

    public OrdersController(
        IOrderService orderService,
        IMapper mapper)
    {
        _orderService = orderService;
        _mapper = mapper;
    }

    [Authorize(Policy = CustomPolicyNames.CanViewOrdersHistory)]
    [HttpGet("history")]
    [OutputCache(NoStore = true)]
    [ProducesResponseType(typeof(IReadOnlyList<OrderModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersHistory([FromQuery] OrderQueryModel queryModel, CancellationToken cancellationToken)
    {
        var query = _mapper.Map<OrderQueryDto>(queryModel);
        var orders = await _orderService.GetOrdersHistoryAsync(query, cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<OrderModel>>(orders));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaidAndCancelledOrders(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetPaidAndCancelledOrdersAsync(cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<OrderModel>>(orders));
    }

    [HttpGet("{id}")]
    [OutputCache(NoStore = true)]
    [ProducesResponseType(typeof(OrderModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        return Ok(_mapper.Map<OrderModel>(order));
    }

    [HttpGet("{id}/details")]
    [OutputCache(NoStore = true)]
    [ProducesResponseType(typeof(IReadOnlyList<OrderGameModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderGames(string id, CancellationToken cancellationToken)
    {
        var orderGames = await _orderService.GetOrderGamesAsync(id, cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<OrderGameModel>>(orderGames));
    }

    [Authorize(Policy = CustomPolicyNames.CanChangeOrderStatusToShipped)]
    [HttpPost("{id}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ShipOrder(string id, CancellationToken cancellationToken)
    {
        await _orderService.ShipOrderAsync(id, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = CustomPolicyNames.CanEditOrders)]
    [HttpPost("{id}/details/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddOrderGame(string id, string key, CancellationToken cancellationToken)
    {
        await _orderService.AddOrderGameAsync(id, key, cancellationToken);
        return Ok();
    }

    [Authorize(Policy = CustomPolicyNames.CanEditOrders)]
    [HttpPatch("details/{id:guid}/quantity")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateOrderGameQuantity(Guid id, [FromBody] UpdateQuantityRequest request, CancellationToken cancellationToken)
    {
        await _orderService.UpdateOrderGameQuantityAsync(id, request, cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = CustomPolicyNames.CanEditOrders)]
    [HttpDelete("details/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrderGame(Guid id, CancellationToken cancellationToken)
    {
        await _orderService.DeleteOrderGameAsync(id, cancellationToken);
        return NoContent();
    }
}