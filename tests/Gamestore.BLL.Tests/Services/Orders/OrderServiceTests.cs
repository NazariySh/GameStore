using System.Globalization;
using FluentValidation;
using Gamestore.BLL.DTOs.Orders;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Services;
using Gamestore.BLL.Services.Orders;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.BLL.Tests.TestData.Orders;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Entities.Orders;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Exceptions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Orders;

public class OrderServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderGame> _orderGameRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        var mockUserContext = new Mock<IUserContext>();
        mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, false);
        mockUserContext.SetupHasPermission(Permissions.EditOrdersFromHistory, false);

        _unitOfWork = UnitOfWorkFactory.Create(mockUserContext.Object);
        _orderRepository = _unitOfWork.Repositories.GetGeneric<Order>();
        _orderGameRepository = _unitOfWork.Repositories.GetGeneric<OrderGame>();
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();

        _mapper = MapperFactory.Create();
        _mockValidationService = new Mock<IValidationService>();

        var mockEntityChangeLogService = new Mock<IEntityChangeLogService>();

        _orderService = new OrderService(
            new ServiceContext(_unitOfWork, _mapper, _mockValidationService.Object, mockUserContext.Object),
            mockEntityChangeLogService.Object,
            Mock.Of<ILogger<OrderService>>());
    }

    [Fact]
    public async Task GetOrdersHistoryAsync_ShouldReturnEmptyList_WhenNoOrdersMatchFilter()
    {
        await SeedOrdersAsync();

        var query = new OrderQueryDto
        {
            Start = DateTime.UtcNow.AddDays(3),
            End = DateTime.UtcNow.AddDays(30),
        };

        var result = await _orderService.GetOrdersHistoryAsync(query);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOrdersHistoryAsync_ShouldApplyDateFilter_WhenFilterIsValid()
    {
        var orders = await SeedOrdersAsync();

        var query = new OrderQueryDto
        {
            Start = DateTime.Parse("2023-01-01T00:00:00Z", CultureInfo.InvariantCulture),
            End = DateTime.Parse("2024-12-31T23:59:59Z", CultureInfo.InvariantCulture),
        };

        var filteredOrders = orders
            .Where(o => o.Date >= query.Start && o.Date <= query.End)
            .ToList();

        var orderDtos = _mapper.Map<List<OrderDto>>(filteredOrders);

        var result = await _orderService.GetOrdersHistoryAsync(query);

        Assert.NotEmpty(result);
        Assert.Equivalent(orderDtos, result);
    }

    [Fact]
    public async Task GetPaidAndCancelledOrdersAsync_ShouldReturnOrders_WhenOrdersExist()
    {
        var orders = await SeedPaidAndCancelledOrdersAsync();

        var orderDtos = _mapper.Map<List<OrderDto>>(orders);

        var result = await _orderService.GetPaidAndCancelledOrdersAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(orderDtos, result);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdOrEmptyIdTestData))]
    public async Task GetOrderGamesAsync_ShouldThrowArgumentException_WhenOrderIdIsEmpty(string invalidOrderId)
    {
        var act = () => _orderService.GetOrderGamesAsync(invalidOrderId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetOrderGamesAsync_ShouldReturnOrderGames_WhenOrderGamesExistForOrder()
    {
        var order = await SeedOrderAsync();

        var orderGameDtos = _mapper.Map<List<OrderGameDto>>(order.OrderGames);

        var result = await _orderService.GetOrderGamesAsync(order.Id.ToString());

        Assert.NotEmpty(result);
        Assert.Equivalent(orderGameDtos, result);
    }

    [Fact]
    public async Task GetOrderGamesAsync_ShouldReturnOrderGames_WhenOrderGamesExistForOrderWithIntId()
    {
        var order = await SeedOrderAsync();

        var orderGameDtos = _mapper.Map<List<OrderGameDto>>(order.OrderGames);

        var result = await _orderService.GetOrderGamesAsync(order.MongoOrderId.ToString()!);

        Assert.NotEmpty(result);
        Assert.Equivalent(orderGameDtos, result);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdOrEmptyIdTestData))]
    public async Task GetByIdAsync_ShouldThrowArgumentException_WhenIdIsEmpty(string invalidId)
    {
        var act = () => _orderService.GetByIdAsync(invalidId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();

        var act = () => _orderService.GetByIdAsync(orderId.ToString());

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenIdIsValidGuid()
    {
        var order = await SeedOrderAsync();

        var orderDto = _mapper.Map<OrderDto>(order);

        var result = await _orderService.GetByIdAsync(order.Id.ToString());

        Assert.NotNull(result);
        Assert.Equivalent(orderDto, result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenIdIsValidIntId()
    {
        var order = await SeedOrderAsync();

        var orderDto = _mapper.Map<OrderDto>(order);

        var result = await _orderService.GetByIdAsync(order.MongoOrderId.ToString()!);

        Assert.NotNull(result);
        Assert.Equivalent(orderDto, result);
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdOrEmptyIdTestData))]
    public async Task ShipOrderAsync_ShouldThrowArgumentException_WhenOrderIdInvalid(string invalidOrderId)
    {
        var act = () => _orderService.ShipOrderAsync(invalidOrderId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task ShipOrderAsync_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();

        var act = () => _orderService.ShipOrderAsync(orderId.ToString());

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task ShipOrderAsync_ShouldThrowConflictException_WhenOrderIsReadonly()
    {
        var order = await SeedOrderWithStatusAsync(OrderStatus.Paid);

        var act = () => _orderService.ShipOrderAsync(order.MongoOrderId.ToString()!);

        var exception = await Assert.ThrowsAsync<ConflictException>(act);
        Assert.Contains($"Order with id '{order.MongoOrderId}' read-only and cannot be modified.", exception.Message);
    }

    [Theory]
    [InlineData(OrderStatus.Open)]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Shipped)]
    public async Task ShipOrderAsync_ShouldThrowConflictException_WhenOrderStatusIsNotPaid(OrderStatus status)
    {
        var order = await SeedOrderWithStatusAsync(status);

        var act = () => _orderService.ShipOrderAsync(order.Id.ToString());

        var exception = await Assert.ThrowsAsync<ConflictException>(act);
        Assert.Contains($"Only orders with status '{OrderStatus.Paid}' can be shipped", exception.Message);
        Assert.Contains($"Current status: '{status}'", exception.Message);
    }

    [Fact]
    public async Task ShipOrderAsync_ShouldUpdateOrderStatusToShipped_WhenOrderStatusIsPaid()
    {
        var order = await SeedOrderWithStatusAsync(OrderStatus.Paid);

        await _orderService.ShipOrderAsync(order.Id.ToString());

        var updatedOrder = await _orderRepository.GetByIdAsync(order.Id);

        Assert.NotNull(updatedOrder);
        Assert.Equal(OrderStatus.Shipped, updatedOrder.Status);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task UpdateOrderGameQuantityAsync_ShouldThrowArgumentException_WhenOrderGameIdIsInvalid(Guid invalidOrderGameId)
    {
        var request = new UpdateQuantityRequest { Count = 1 };

        var act = () => _orderService.UpdateOrderGameQuantityAsync(invalidOrderGameId, request);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task UpdateOrderGameQuantityAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var orderGameId = Guid.NewGuid();
        var request = new UpdateQuantityRequest { Count = -10 };

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _orderService.UpdateOrderGameQuantityAsync(orderGameId, request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task UpdateOrderGameQuantityAsync_ShouldThrowNotFoundException_WhenOrderGameDoesNotExist()
    {
        var orderId = Guid.NewGuid();
        var request = new UpdateQuantityRequest { Count = 1 };

        var act = () => _orderService.UpdateOrderGameQuantityAsync(orderId, request);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task UpdateOrderGameQuantityAsync_ShouldThrowForbiddenException_WhenUserCannotEditOrdersFromHistory()
    {
        var (_, orderGame) = await SeedShippedOrderWithOrderGameAsync();
        var request = new UpdateQuantityRequest { Count = 1 };

        var act = () => _orderService.UpdateOrderGameQuantityAsync(orderGame.Id, request);

        var exception = await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Contains("You do not have permission to edit orders from history.", exception.Message);
    }

    [Fact]
    public async Task UpdateOrderGameQuantityAsync_ShouldThrowNotFoundException_WhenGameDoesNotExist()
    {
        var (_, orderGame) = await SeedOrderWithOrderGameAsync();
        var request = new UpdateQuantityRequest { Count = 1 };

        var act = () => _orderService.UpdateOrderGameQuantityAsync(orderGame.Id, request);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task UpdateOrderGameQuantityAsync_ShouldThrowNotFoundException_WhenUserCannotViewDeletedGames()
    {
        var game = await SeedDeletedGameAsync();
        var (_, orderGame) = await SeedOrderWithOrderGameAsync(game);
        var request = new UpdateQuantityRequest { Count = 1 };

        var act = () => _orderService.UpdateOrderGameQuantityAsync(orderGame.Id, request);
        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task UpdateOrderGameQuantityAsync_ShouldThrowGameOutOfStockException_WhenGameDoesNotHaveEnoughStock()
    {
        var game = await SeedGameAsync(5);
        var (_, orderGame) = await SeedOrderWithOrderGameAsync(game);
        var request = new UpdateQuantityRequest { Count = 10 };

        var act = () => _orderService.UpdateOrderGameQuantityAsync(orderGame.Id, request);

        await Assert.ThrowsAsync<GameOutOfStockException>(act);
    }

    [Fact]
    public async Task UpdateOrderGameQuantityAsync_ShouldUpdateQuantity_WhenRequestIsValid()
    {
        var game = await SeedGameAsync(10);
        var gameInStock = game.UnitInStock;

        var (_, orderGame) = await SeedOrderWithOrderGameAsync(game);

        var request = new UpdateQuantityRequest { Count = 5 };

        var difference = request.Count - orderGame.Quantity;

        await _orderService.UpdateOrderGameQuantityAsync(orderGame.Id, request);

        var updatedOrderGame = await GetOrderGameAsync(orderGame.Id);

        Assert.NotNull(updatedOrderGame);
        Assert.Equal(request.Count, updatedOrderGame.Quantity);
        Assert.True(await CheckProductStockAsync(game.Id, gameInStock - difference));
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task DeleteOrderGameAsync_ShouldThrowArgumentException_WhenOrderGameIdInvalid(Guid invalidOrderGameId)
    {
        var act = () => _orderService.DeleteOrderGameAsync(invalidOrderGameId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task DeleteOrderGameAsync_ShouldThrowNotFoundException_WhenOrderGameDoesNotExist()
    {
        var orderGameId = Guid.NewGuid();

        var act = () => _orderService.DeleteOrderGameAsync(orderGameId);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task DeleteOrderGameAsync_ShouldThrowForbiddenException_WhenUserCannotEditOrdersFromHistory()
    {
        var (_, orderGame) = await SeedShippedOrderWithOrderGameAsync();

        var act = () => _orderService.DeleteOrderGameAsync(orderGame.Id);

        var exception = await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Contains("You do not have permission to edit orders from history.", exception.Message);
    }

    [Fact]
    public async Task DeleteOrderGameAsync_ShouldThrowNotFoundException_WhenGameDoesNotExist()
    {
        var (_, orderGame) = await SeedOrderWithOrderGameAsync();

        var act = () => _orderService.DeleteOrderGameAsync(orderGame.Id);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task DeleteOrderGameAsync_ShouldDeleteOrderGame_WhenOrderGameExists()
    {
        var game = await SeedGameAsync();
        var gameInStock = game.UnitInStock;

        var (_, orderGame) = await SeedOrderWithOrderGameAsync(game);
        var orderGameQuantity = orderGame.Quantity;

        await _orderService.DeleteOrderGameAsync(orderGame.Id);

        Assert.False(await OrderGameExistsAsync(orderGame.Id));
        Assert.True(await CheckProductStockAsync(game.Id, gameInStock + orderGameQuantity));
    }

    [Fact]
    public async Task DeleteOrderGameAsync_ShouldDeleteOrder_WhenLastOrderGameIsRemoved()
    {
        var game = await SeedGameAsync();
        var (order, orderGame) = await SeedOrderWithOrderGameAsync(game);

        await _orderService.DeleteOrderGameAsync(orderGame.Id);

        Assert.False(await OrderGameExistsAsync(orderGame.Id));
        Assert.False(await OrderExistsAsync(order.Id));
    }

    [Theory]
    [ClassData(typeof(InvalidEntityIdOrEmptyIdTestData))]
    public async Task AddOrderGameAsync_ShouldThrowArgumentException_WhenOrderIdInvalid(string invalidOrderId)
    {
        var gameKey = "GAME-KEY-1234";

        var act = () => _orderService.AddOrderGameAsync(invalidOrderId, gameKey);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task AddOrderGameAsync_ShouldThrowArgumentException_WhenGameKeyIsInvalid(string invalidGameKey)
    {
        var orderId = Guid.NewGuid();

        var act = () => _orderService.AddOrderGameAsync(orderId.ToString(), invalidGameKey);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task AddOrderGameAsync_ShouldThrowConflictException_WhenOrderIsReadonly()
    {
        var game = await SeedGameAsync(10);

        var (order, _) = await SeedOrderWithOrderGameAsync(game);

        var act = () => _orderService.AddOrderGameAsync(order.MongoOrderId.ToString()!, game.Key);

        await Assert.ThrowsAsync<ConflictException>(act);
    }

    [Fact]
    public async Task AddOrderGameAsync_ShouldThrowForbiddenException_WhenUserCannotEditOrdersFromHistory()
    {
        var order = await SeedShippedOrderAsync();
        var gameKey = "GAME-KEY-1234";

        var act = () => _orderService.AddOrderGameAsync(order.Id.ToString(), gameKey);

        var exception = await Assert.ThrowsAsync<ForbiddenException>(act);
        Assert.Contains("You do not have permission to edit orders from history.", exception.Message);
    }

    [Fact]
    public async Task AddOrderGameAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        var orderId = Guid.NewGuid();
        var gameKey = "GAME-KEY-1234";

        var act = () => _orderService.AddOrderGameAsync(orderId.ToString(), gameKey);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task AddOrderGameAsync_ShouldThrowGameOutOfStockException_WhenGameDoesNotHaveEnoughStock()
    {
        var game = await SeedGameAsync(0);
        var (_, orderGame) = await SeedOrderWithOrderGameAsync(game);

        var act = () => _orderService.AddOrderGameAsync(orderGame.OrderId.ToString(), game.Key);

        await Assert.ThrowsAsync<GameOutOfStockException>(act);
    }

    [Fact]
    public async Task AddOrderGameAsync_ShouldThrowNotFoundException_WhenOrderDoesNotExist()
    {
        var game = await SeedGameAsync();

        var orderId = Guid.NewGuid();

        var act = () => _orderService.AddOrderGameAsync(orderId.ToString(), game.Key);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task AddOrderGameAsync_ShouldAddGameToOrder_WhenGameIsNotAlreadyInOrder()
    {
        var game = await SeedGameAsync();
        var gameInStock = game.UnitInStock;

        var order = await SeedEmptyOrderAsync();

        await _orderService.AddOrderGameAsync(order.Id.ToString(), game.Key);

        var updatedOrder = await GetOrderWithDetailsAsync(order.Id);

        Assert.NotNull(updatedOrder);
        Assert.Single(updatedOrder.OrderGames);
        Assert.True(await CheckProductStockAsync(game.Id, gameInStock - 1));
    }

    [Fact]
    public async Task AddOrderGameAsync_ShouldIncrementItemQuantity_WhenGameIsAlreadyInOrder()
    {
        var game = await SeedGameAsync();
        var gameInStock = game.UnitInStock;

        var (order, orderGame) = await SeedOrderWithOrderGameAsync(game);
        var orderGameQuantity = orderGame.Quantity;

        await _orderService.AddOrderGameAsync(order.Id.ToString(), game.Key);

        var updatedOrder = await GetOrderWithDetailsAsync(order.Id);

        Assert.NotNull(updatedOrder);
        Assert.Single(updatedOrder.OrderGames);

        var updatedOrderGame = updatedOrder.OrderGames.First(og => og.Id == orderGame.Id);
        Assert.Equal(orderGameQuantity + 1, updatedOrderGame.Quantity);

        Assert.True(await CheckProductStockAsync(game.Id, gameInStock - 1));
    }

    private Task<Order?> GetOrderWithDetailsAsync(Guid orderId)
    {
        return _orderRepository.GetByIdAsync(
            orderId,
            o => o.Include(og => og.OrderGames));
    }

    private Task<OrderGame?> GetOrderGameAsync(Guid orderGameId)
    {
        return _orderGameRepository.GetByIdAsync(orderGameId);
    }

    private Task<bool> OrderExistsAsync(Guid orderId)
    {
        return _orderRepository.ExistsAsync(o => o.Id == orderId);
    }

    private Task<bool> OrderGameExistsAsync(Guid orderGameId)
    {
        return _orderGameRepository.ExistsAsync(og => og.Id == orderGameId);
    }

    private Task<bool> CheckProductStockAsync(Guid productId, int expectedStock)
    {
        return _gameRepository.ExistsAsync(g => g.Id == productId && g.UnitInStock == expectedStock);
    }

    private async Task<Game> SeedDeletedGameAsync()
    {
        var game = GameTestData.GetGame();
        game.IsDeleted = true;
        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();
        return game;
    }

    private async Task<Game> SeedGameAsync(int? stock = null)
    {
        var game = GameTestData.GetGame();
        if (stock.HasValue)
        {
            game.UnitInStock = stock.Value;
        }

        await _gameRepository.AddAsync(game);
        await _unitOfWork.SaveChangesAsync();
        return game;
    }

    private async Task<Order> SeedOrderAsync()
    {
        var order = OrderTestData.GetOrder();
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        return order;
    }

    private async Task<Order> SeedEmptyOrderAsync()
    {
        var order = OrderTestData.GetEmptyOrder();
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        return order;
    }

    private async Task<Order> SeedShippedOrderAsync()
    {
        var order = OrderTestData.GetOrder(OrderStatus.Shipped);
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        return order;
    }

    private async Task<(Order Order, OrderGame OrderGame)> SeedShippedOrderWithOrderGameAsync()
    {
        var order = OrderTestData.GetEmptyOrder();
        order.Status = OrderStatus.Shipped;
        var orderGame = OrderTestData.GetOrderGame(order.Id);
        order.OrderGames.Add(orderGame);

        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return (order, orderGame);
    }

    private async Task<(Order Order, OrderGame OrderGame)> SeedOrderWithOrderGameAsync()
    {
        var order = OrderTestData.GetEmptyOrder();
        var orderGame = OrderTestData.GetOrderGame(order.Id);
        order.OrderGames.Add(orderGame);

        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return (order, orderGame);
    }

    private async Task<(Order Order, OrderGame OrderGame)> SeedOrderWithOrderGameAsync(Game game)
    {
        var order = OrderTestData.GetEmptyOrder();
        var orderGame = OrderTestData.GetOrderGame(game);
        orderGame.OrderId = order.Id;
        order.OrderGames.Add(orderGame);

        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return (order, orderGame);
    }

    private async Task<List<Order>> SeedOrdersAsync()
    {
        var orders = OrderTestData.GetOrders();
        await _orderRepository.AddRangeAsync(orders);
        await _unitOfWork.SaveChangesAsync();
        return orders;
    }

    private async Task<Order> SeedOrderWithStatusAsync(OrderStatus status)
    {
        var order = OrderTestData.GetOrder(status);
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        return order;
    }

    private async Task<List<Order>> SeedPaidAndCancelledOrdersAsync()
    {
        var orders = OrderTestData.GetPaidAndCancelledOrders();
        await _orderRepository.AddRangeAsync(orders);
        await _unitOfWork.SaveChangesAsync();
        return orders;
    }
}