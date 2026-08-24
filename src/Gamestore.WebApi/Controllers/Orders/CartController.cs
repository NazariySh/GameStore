using Gamestore.BLL.Interfaces.Orders;
using Gamestore.WebApi.Constants;
using Gamestore.WebApi.Extensions;
using Gamestore.WebApi.Models.Orders;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Gamestore.WebApi.Controllers.Orders;

[Authorize]
[Route("api/orders/[controller]")]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;
    private readonly IMapper _mapper;

    public CartController(
        ICartService cartService,
        IMapper mapper)
    {
        _cartService = cartService;
        _mapper = mapper;
    }

    [HttpGet]
    [OutputCache(NoStore = true)]
    [ProducesResponseType(typeof(IReadOnlyList<OrderGameModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCartItems(CancellationToken cancellationToken)
    {
        var cartItems = await _cartService.GetCartItemsAsync(User.GetId(), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<OrderGameModel>>(cartItems));
    }

    [Authorize(Policy = CustomPolicyNames.CanBuyGames)]
    [HttpPost("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddToCart(string key, CancellationToken cancellationToken)
    {
        await _cartService.AddAsync(key, User.GetId(), cancellationToken);
        return Ok();
    }

    [Authorize(Policy = CustomPolicyNames.CanBuyGames)]
    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromCart(string key, CancellationToken cancellationToken)
    {
        await _cartService.RemoveAsync(key, User.GetId(), cancellationToken);
        return NoContent();
    }
}