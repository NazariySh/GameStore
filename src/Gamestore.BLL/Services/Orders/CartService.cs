using Gamestore.BLL.DTOs.Logging;
using Gamestore.BLL.DTOs.Orders;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Interfaces.Orders;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Games;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Entities.Orders;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Shared;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Orders;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderGame> _orderGameRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IMapper _mapper;
    private readonly IEntityChangeLogService _entityChangeLogService;
    private readonly ILogger<CartService> _logger;

    public CartService(
        IServiceContext context,
        IEntityChangeLogService entityChangeLogService,
        ILogger<CartService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _orderRepository = _unitOfWork.Repositories.GetGeneric<Order>();
        _orderGameRepository = _unitOfWork.Repositories.GetGeneric<OrderGame>();
        _gameRepository = _unitOfWork.Repositories.Get<IGameRepository>();
        _mapper = context.Mapper;
        _entityChangeLogService = entityChangeLogService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OrderGameDto>> GetCartItemsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve cart items for customer with Id '{CustomerId}'", customerId);

        Guard.AgainstEmpty(customerId);

        var cart = await GetCartAsync(customerId, cancellationToken);

        IReadOnlyList<OrderGameDto> cartItems = [];

        if (cart is not null)
        {
            cartItems = await _orderGameRepository.GetAllAsync<OrderGameDto>(
                og => og.OrderId == cart.Id,
                cancellationToken);
        }

        _logger.LogInformation("Retrieved {Count} cart items for customer with Id '{CustomerId}' from database", cartItems.Count, customerId);

        return cartItems;
    }

    public async Task RemoveAsync(string gameKey, Guid customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to remove game with key '{GameKey}' from cart for customer with Id '{CustomerId}'", gameKey, customerId);

        Guard.AgainstNullOrWhiteSpace(gameKey);
        Guard.AgainstEmpty(customerId);

        var cart = await GetCartOrThrowAsync(customerId, cancellationToken);
        var game = await GetGameOrThrowAsync(gameKey, cancellationToken);
        var cartItem = await GetCartItemOrThrowAsync(game, cart.Id, cancellationToken);

        await _orderGameRepository.RemoveAsync(cartItem, cancellationToken);

        game.UnitInStock += cartItem.Quantity;

        await _gameRepository.UpdateUnitInStockAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<OrderGame>(LogAction.Delete, oldVersion: cartItem), cancellationToken);

        _logger.LogInformation("Successfully removed game with key '{GameKey}' from cart for customer with Id '{CustomerId}'", gameKey, customerId);

        if (await IsEmptyCartAsync(cart.Id, cancellationToken))
        {
            await DeleteCartAsync(cart, cancellationToken);
        }
    }

    public async Task AddAsync(string gameKey, Guid customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to add game with key '{GameKey}' to cart for customer with Id '{CustomerId}'", gameKey, customerId);

        Guard.AgainstNullOrWhiteSpace(gameKey);
        Guard.AgainstEmpty(customerId);

        var game = await GetGameOrThrowAsync(gameKey, cancellationToken);

        EnsureGameInStock(game);

        var cart = await GetOrCreateCartAsync(customerId, cancellationToken);
        var existingCartItem = await GetCartItemAsync(game, cart.Id, cancellationToken);

        if (existingCartItem is null)
        {
            await AddProductToCartAsync(game, cart, cancellationToken);
        }
        else
        {
            await IncrementCartProductQuantityAsync(existingCartItem, game, cancellationToken);
        }
    }

    private async Task AddProductToCartAsync(Game game, Order cart, CancellationToken cancellationToken)
    {
        var newItem = new OrderGame
        {
            OrderId = cart.Id,
            ProductId = game.Id,
            MongoProductId = game.ProductId,
            Quantity = 1,
            Price = game.Price,
            Discount = game.Discount,
        };

        await _orderGameRepository.AddAsync(newItem, cancellationToken);

        game.UnitInStock--;

        await _gameRepository.UpdateUnitInStockAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<OrderGame>(LogAction.Add, newVersion: newItem), cancellationToken);

        _logger.LogInformation("Successfully added game with key '{GameKey}' to cart with Id '{CartId}'", game.Key, cart.Id);
    }

    private async Task IncrementCartProductQuantityAsync(OrderGame existingOrderGame, Game game, CancellationToken cancellationToken)
    {
        var oldVersion = _mapper.Map<OrderGame>(existingOrderGame);

        existingOrderGame.Quantity++;

        await _orderGameRepository.UpdateAsync(existingOrderGame, cancellationToken);

        game.UnitInStock--;

        await _gameRepository.UpdateUnitInStockAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<OrderGame>(LogAction.Update, oldVersion, existingOrderGame), cancellationToken);

        _logger.LogInformation("Successfully incremented quantity of cart item with Id '{CartItemId}'", existingOrderGame.Id);
    }

    private async Task<Order> CreateCartAsync(Guid customerId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new cart for customer with Id '{CustomerId}'", customerId);

        var newCart = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Open,
            Date = DateTime.UtcNow,
        };

        await _orderRepository.AddAsync(newCart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Order>(LogAction.Add, newVersion: newCart), cancellationToken);

        _logger.LogInformation("Created new cart with Id '{CartId}' for customer with Id '{CustomerId}'", newCart.Id, customerId);

        return newCart;
    }

    private async Task DeleteCartAsync(Order cart, CancellationToken cancellationToken)
    {
        await _orderRepository.RemoveAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Order>(LogAction.Delete, oldVersion: cart), cancellationToken);

        _logger.LogInformation("Deleted cart with Id '{CartId}' for customer with Id '{CustomerId}'", cart.Id, cart.CustomerId);
    }

    private void EnsureGameInStock(Game game)
    {
        if (game.UnitInStock == 0)
        {
            _logger.LogError("Game with key '{Key}' is out of stock", game.Key);
            throw new GameOutOfStockException(game.Key);
        }
    }

    private async Task<Game> GetGameOrThrowAsync(string key, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetSingleAsync(
            g => g.Key == key,
            cancellationToken: cancellationToken);

        if (game is null)
        {
            _logger.LogError("Game with key '{Key}' not found", key);
            throw new GameNotFoundException(key);
        }

        return game;
    }

    private async Task<OrderGame> GetCartItemOrThrowAsync(Game product, Guid cartId, CancellationToken cancellationToken)
    {
        var productId = product.Id != Guid.Empty ? product.Id.ToString() : product.ProductId.ToString();

        var cartItem = await GetCartItemAsync(product, cartId, cancellationToken);

        if (cartItem is null)
        {
            _logger.LogError("Cart item with ProductId '{ProductId}' not found in cart with Id '{CartId}'", productId, cartId);
            throw new NotFoundException($"Cart item with ProductId '{productId}' not found in cart with Id '{cartId}'.");
        }

        return cartItem;
    }

    private async Task<Order> GetCartOrThrowAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var cart = await GetCartAsync(customerId, cancellationToken);

        if (cart is null)
        {
            _logger.LogError("Cart for customer with Id '{CustomerId}' not found", customerId);
            throw new NotFoundException($"Cart for customer with Id '{customerId}' not found.");
        }

        return cart;
    }

    private async Task<Order> GetOrCreateCartAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var cart = await GetCartAsync(customerId, cancellationToken);

        if (cart is null)
        {
            _logger.LogInformation("No existing cart found for customer with Id '{CustomerId}'", customerId);

            return await CreateCartAsync(customerId, cancellationToken);
        }

        return cart;
    }

    private Task<OrderGame?> GetCartItemAsync(Game product, Guid cartId, CancellationToken cancellationToken)
    {
        return _orderGameRepository.GetAsync(
            og => og.OrderId == cartId &&
                  ((product.Id != Guid.Empty && og.ProductId == product.Id) ||
                   (product.ProductId.HasValue && og.MongoProductId == product.ProductId)),
            cancellationToken: cancellationToken);
    }

    private Task<Order?> GetCartAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return _orderRepository.GetAsync(
            o => o.CustomerId == customerId && o.Status == OrderStatus.Open,
            cancellationToken: cancellationToken);
    }

    private Task<bool> IsEmptyCartAsync(Guid cartId, CancellationToken cancellationToken)
    {
        return _orderGameRepository.NotExistsAsync(og => og.OrderId == cartId, cancellationToken);
    }
}