using System.Diagnostics.CodeAnalysis;
using Gamestore.BLL.DTOs.Logging;
using Gamestore.BLL.DTOs.Orders;
using Gamestore.BLL.FilterPipelines.Implementations;
using Gamestore.BLL.FilterPipelines.Implementations.OrderFilters;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Interfaces.Orders;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Games;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Entities.Orders;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Shared;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Orders;

public class OrderService : IOrderService
{
    private static readonly TimeSpan DefaultOrderHistoryStartPeriod = TimeSpan.FromDays(30);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderGame> _orderGameRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;
    private readonly IEntityChangeLogService _entityChangeLogService;
    private readonly IUserContext _userContext;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IServiceContext context,
        IEntityChangeLogService entityChangeLogService,
        ILogger<OrderService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _orderRepository = _unitOfWork.Repositories.GetGeneric<Order>();
        _orderGameRepository = _unitOfWork.Repositories.GetGeneric<OrderGame>();
        _gameRepository = _unitOfWork.Repositories.Get<IGameRepository>();
        _mapper = context.Mapper;
        _validationService = context.Validation;
        _entityChangeLogService = entityChangeLogService;
        _userContext = context.UserContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersHistoryAsync(OrderQueryDto query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve orders history with query: {@Query}", query);

        var queryPipeline = new QueryPipelineBuilder<Order>()
            .AddFilterStep(new FilterByDateRange(query.Start ?? DateTime.UtcNow.Subtract(DefaultOrderHistoryStartPeriod), query.End))
            .AddSortStep(new OrderSortByDateStep())
            .Build();

        var orders = await _orderRepository.GetAllAsync<OrderDto>(queryPipeline, cancellationToken);

        _logger.LogInformation("Retrieved {Count} orders from database", orders.Count);

        return orders;
    }

    public async Task<IReadOnlyList<OrderDto>> GetPaidAndCancelledOrdersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all paid and cancelled orders");

        var queryPipeline = new QueryPipelineBuilder<Order>()
            .AddFilterStep(new FilterStep<Order>(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Cancelled))
            .AddSortStep(new OrderSortByDateStep())
            .Build();

        var orders = await _orderRepository.GetAllAsync<OrderDto>(queryPipeline, cancellationToken);

        _logger.LogInformation("Retrieved {Count} paid and cancelled orders from database", orders.Count);

        return orders;
    }

    public async Task<IReadOnlyList<OrderGameDto>> GetOrderGamesAsync(string orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve order games for order with Id '{OrderId}'", orderId);

        var id = EntityId.Parse(orderId);

        Guard.AgainstEmpty(id);

        IReadOnlyList<OrderGameDto> orderGames;

        if (id.IsPrimary)
        {
            orderGames = await _orderGameRepository.GetAllAsync<OrderGameDto>(
                og => og.OrderId == id.PrimaryId,
                cancellationToken);
        }
        else
        {
            orderGames = await _orderGameRepository.GetAllAsync<OrderGameDto>(
                og => og.MongoOrderId == id.SecondaryId,
                cancellationToken);
        }

        _logger.LogInformation("Retrieved {Count} order games for order with Id '{OrderId}' from database", orderGames.Count, orderId);

        return orderGames;
    }

    public async Task<OrderDto> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve order with Id '{OrderId}'", id);

        var orderId = EntityId.Parse(id);

        Guard.AgainstEmpty(orderId);

        var order = orderId.IsPrimary
            ? await _orderRepository.GetByIdAsync<OrderDto>(orderId.PrimaryId.Value, cancellationToken)
            : await _orderRepository.GetAsync<OrderDto>(o => o.MongoOrderId == orderId.SecondaryId, cancellationToken);

        EnsureOrderExists(order, id);

        _logger.LogInformation("Retrieved order with Id '{OrderId}' from database", id);

        return order;
    }

    public async Task ShipOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to ship order with Id '{OrderId}'", orderId);

        var id = EntityId.Parse(orderId);

        Guard.AgainstEmpty(id);

        EnsureNotReadonlyOrder(id);

        var order = await GetOrderOrThrowAsync(id.PrimaryId!.Value, cancellationToken);

        EnsureCanShipOrder(order);

        var oldVersion = _mapper.Map<Order>(order);

        order.Status = OrderStatus.Shipped;
        order.ShippedDate = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Order>(LogAction.Update, oldVersion, order), cancellationToken);

        _logger.LogInformation("Shipped order with Id '{OrderId}'", orderId);
    }

    public async Task UpdateOrderGameQuantityAsync(Guid orderGameId, UpdateQuantityRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to update quantity for order game with Id '{OrderGameId}'", orderGameId);

        Guard.AgainstEmpty(orderGameId);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var orderGame = await GetOrderGameOrThrowAsync(orderGameId, cancellationToken);

        await EnsureCanEditOrderIfFromHistory(orderGame.OrderId, cancellationToken);

        var game = await GetGameOrThrowAsync(orderGame, cancellationToken);

        var oldVersion = _mapper.Map<OrderGame>(orderGame);

        UpdateStockReservation(game, orderGame.Quantity, request.Count);

        orderGame.Quantity = request.Count;

        await _orderGameRepository.UpdateAsync(orderGame, cancellationToken);

        await _gameRepository.UpdateUnitInStockAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<OrderGame>(LogAction.Update, oldVersion, orderGame), cancellationToken);

        _logger.LogInformation("Updated quantity for order game with Id '{OrderGameId}' to {Quantity}", orderGameId, request.Count);
    }

    public async Task DeleteOrderGameAsync(Guid orderGameId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete order game with Id '{OrderGameId}'", orderGameId);

        Guard.AgainstEmpty(orderGameId);

        var orderGame = await GetOrderGameOrThrowAsync(orderGameId, cancellationToken);

        await EnsureCanEditOrderIfFromHistory(orderGame.OrderId, cancellationToken);

        var game = await GetGameOrThrowAsync(orderGame, cancellationToken);

        await _orderGameRepository.RemoveAsync(orderGame, cancellationToken);

        game.UnitInStock += orderGame.Quantity;

        await _gameRepository.UpdateUnitInStockAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<OrderGame>(LogAction.Delete, oldVersion: orderGame), cancellationToken);

        _logger.LogInformation("Deleted order game with Id '{OrderGameId}'", orderGameId);

        if (!await OrderHasOrderGamesAsync(orderGame.OrderId, cancellationToken))
        {
            await DeleteOrderAsync(orderGame.OrderId, cancellationToken);
        }
    }

    public async Task AddOrderGameAsync(string orderId, string gameKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to add game with key '{GameKey}' to order with Id '{OrderId}'", gameKey, orderId);

        Guard.AgainstNullOrWhiteSpace(gameKey);

        var id = EntityId.Parse(orderId);

        Guard.AgainstEmpty(id);

        EnsureNotReadonlyOrder(id);

        await EnsureCanEditOrderIfFromHistory(id.PrimaryId!.Value, cancellationToken);

        var game = await GetGameOrThrowAsync(gameKey, cancellationToken);

        EnsureGameInStock(game);

        var order = await GetOrderOrThrowAsync(id.PrimaryId!.Value, cancellationToken);
        var existingOrderGame = await GetOrderGameAsync(order.Id, game, cancellationToken);

        if (existingOrderGame is null)
        {
            await AddOrderGameToOrderAsync(order, game, cancellationToken);
        }
        else
        {
            await IncrementOrderGameQuantityAsync(existingOrderGame, game, cancellationToken);
        }
    }

    private async Task AddOrderGameToOrderAsync(Order order, Game game, CancellationToken cancellationToken)
    {
        var newOrderGame = new OrderGame
        {
            OrderId = order.Id,
            ProductId = game.Id,
            MongoProductId = game.ProductId,
            Quantity = 1,
            Price = game.Price,
            Discount = game.Discount,
        };

        await _orderGameRepository.AddAsync(newOrderGame, cancellationToken);

        game.UnitInStock--;

        await _gameRepository.UpdateUnitInStockAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<OrderGame>(LogAction.Add, newVersion: newOrderGame), cancellationToken);

        _logger.LogInformation("Added game with key '{GameKey}' to order with Id '{OrderId}'", game.Key, order.Id);
    }

    private async Task IncrementOrderGameQuantityAsync(OrderGame orderGame, Game game, CancellationToken cancellationToken)
    {
        var oldVersion = _mapper.Map<OrderGame>(orderGame);

        orderGame.Quantity++;

        await _orderGameRepository.UpdateAsync(orderGame, cancellationToken);

        game.UnitInStock--;

        await _gameRepository.UpdateUnitInStockAsync(game, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<OrderGame>(LogAction.Update, oldVersion, orderGame), cancellationToken);

        _logger.LogInformation("Increased quantity of game with key '{GameKey}' in order with Id '{OrderId}' to {Quantity}", game.Key, orderGame.OrderId, orderGame.Quantity);
    }

    private async Task DeleteOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetOrderOrThrowAsync(id, cancellationToken);

        await _orderRepository.RemoveAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Order>(LogAction.Delete, oldVersion: order), cancellationToken);

        _logger.LogInformation("Deleted order with Id '{OrderId}' because it had no more associated order games", id);
    }

    private Task<bool> OrderHasOrderGamesAsync(Guid id, CancellationToken cancellationToken)
    {
        return _orderGameRepository.ExistsAsync(og => og.OrderId == id, cancellationToken);
    }

    private async Task<Order> GetOrderOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken: cancellationToken);

        EnsureOrderExists(order, id.ToString());

        return order;
    }

    private async Task<OrderGame> GetOrderGameOrThrowAsync(Guid orderGameId, CancellationToken cancellationToken)
    {
        var orderGame = await _orderGameRepository.GetByIdAsync(orderGameId, cancellationToken: cancellationToken);

        EnsureOrderGameExists(orderGame, orderGameId);

        return orderGame;
    }

    private Task<OrderGame?> GetOrderGameAsync(Guid orderId, Game game, CancellationToken cancellationToken)
    {
        return _orderGameRepository.GetAsync(
            og => og.OrderId == orderId &&
                  ((game.Id != Guid.Empty && og.ProductId == game.Id) ||
                   (game.ProductId.HasValue && og.MongoProductId == game.ProductId)),
            cancellationToken: cancellationToken);
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

    private async Task<Game> GetGameOrThrowAsync(OrderGame orderGame, CancellationToken cancellationToken)
    {
        var productId = orderGame.ProductId != Guid.Empty ? orderGame.ProductId.ToString() : orderGame.MongoProductId.ToString();

        var game = await _gameRepository.GetSingleAsync(
            g =>
                (orderGame.ProductId != Guid.Empty && g.Id == orderGame.ProductId) ||
                (orderGame.MongoProductId.HasValue && g.ProductId == orderGame.MongoProductId),
            cancellationToken: cancellationToken);

        if (game is null)
        {
            _logger.LogError("Game with Id '{Id}' not found", productId);
            throw new NotFoundException($"Game with Id '{productId}' not found.");
        }

        return game;
    }

    private void UpdateStockReservation(Game game, int previousQuantity, int newQuantity)
    {
        var difference = newQuantity - previousQuantity;

        if (difference > 0)
        {
            EnsureGameHasEnoughStock(game, difference);

            game.UnitInStock -= difference;
        }
        else if (difference < 0)
        {
            game.UnitInStock += Math.Abs(difference);
        }
    }

    private async Task EnsureCanEditOrderIfFromHistory(Guid orderId, CancellationToken cancellationToken)
    {
        if (_userContext.HasPermission(Permissions.EditOrdersFromHistory))
        {
            return;
        }

        if (await IsOrderFromHistoryAsync(orderId, cancellationToken))
        {
            _logger.LogError("Editing orders from history is not allowed. Order Id: '{OrderId}'", orderId);
            throw new ForbiddenException("You do not have permission to edit orders from history.");
        }
    }

    private void EnsureCanShipOrder(Order order)
    {
        if (order.Status is not OrderStatus.Paid)
        {
            _logger.LogError("Cannot ship order with Id '{OrderId}' because its status is '{Status}'", order.Id, order.Status);
            throw new ConflictException($"Only orders with status '{OrderStatus.Paid}' can be shipped. Current status: '{order.Status}'.");
        }
    }

    private void EnsureNotReadonlyOrder(EntityId id)
    {
        if (id.IsSecondary)
        {
            _logger.LogError("Order with id '{OrderId}' is read-only and cannot be modified", id.SecondaryId);
            throw new ConflictException($"Order with id '{id.SecondaryId}' read-only and cannot be modified.");
        }
    }

    private Task<bool> IsOrderFromHistoryAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return _orderRepository.ExistsAsync(
            o => o.Id == orderId && (o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Cancelled),
            cancellationToken);
    }

    private void EnsureGameInStock(Game game)
    {
        if (game.UnitInStock == 0)
        {
            _logger.LogError("Game with key '{Key}' is out of stock", game.Key);
            throw new GameOutOfStockException(game.Key);
        }
    }

    private void EnsureGameHasEnoughStock(Game game, int requiredQuantity)
    {
        if (game.UnitInStock < requiredQuantity)
        {
            _logger.LogError("Not enough stock for game with Id '{GameId}'. Available: {Available}, Requested: {Requested}", game.Id, game.UnitInStock, requiredQuantity);
            throw new GameOutOfStockException(game.Key);
        }
    }

    private void EnsureOrderExists<T>([NotNull] T? order, string id)
    {
        if (order is null)
        {
            _logger.LogError("Order with Id '{Id}' not found", id);
            throw new NotFoundException($"Order with Id '{id}' not found.");
        }
    }

    private void EnsureOrderGameExists<T>([NotNull] T? orderGame, Guid id)
    {
        if (orderGame is null)
        {
            _logger.LogError("Order game with Id '{Id}' not found", id);
            throw new NotFoundException($"Order game with Id '{id}' not found.");
        }
    }
}