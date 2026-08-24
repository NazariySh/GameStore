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

public class CartServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IMapper _mapper;
    private readonly CartService _cartService;

    public CartServiceTests()
    {
        var mockUserContext = new Mock<IUserContext>();
        mockUserContext.SetupHasPermission(Permissions.ViewDeletedGames, false);

        _unitOfWork = UnitOfWorkFactory.Create(mockUserContext.Object);
        _orderRepository = _unitOfWork.Repositories.GetGeneric<Order>();
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();

        _mapper = MapperFactory.Create();
        var mockEntityChangeLogService = new Mock<IEntityChangeLogService>();
        var mockValidationService = new Mock<IValidationService>();

        _cartService = new CartService(
            new ServiceContext(_unitOfWork, _mapper, mockValidationService.Object, mockUserContext.Object),
            mockEntityChangeLogService.Object,
            Mock.Of<ILogger<CartService>>());
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task GetCartItemsAsync_ShouldThrowArgumentException_WhenCustomerIdInvalid(Guid invalidCustomerId)
    {
        var act = () => _cartService.GetCartItemsAsync(invalidCustomerId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task GetCartItemsAsync_ShouldReturnOrderGames_WhenCartItemsExistForCustomer()
    {
        var customerId = OrderTestData.UserId;
        var cart = await SeedCartWithItemsAsync(customerId);

        var orderGameDtos = _mapper.Map<List<OrderGameDto>>(cart.OrderGames);

        var result = await _cartService.GetCartItemsAsync(cart.CustomerId);

        Assert.NotEmpty(result);
        Assert.Equivalent(orderGameDtos, result);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task AddAsync_ShouldThrowArgumentException_WhenGameKeyIsInvalid(string invalidGameKey)
    {
        var customerId = OrderTestData.UserId;

        var act = () => _cartService.AddAsync(invalidGameKey, customerId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task AddAsync_ShouldThrowArgumentException_WhenCustomerIdInvalid(Guid invalidCustomerId)
    {
        const string gameKey = "TestGameKey";

        var act = () => _cartService.AddAsync(gameKey, invalidCustomerId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        var customerId = OrderTestData.UserId;
        const string gameKey = "NonExistentGameKey";

        var act = () => _cartService.AddAsync(gameKey, customerId);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndCannotViewDeletedGames()
    {
        var customerId = OrderTestData.UserId;
        var game = await SeedDeletedGameAsync();

        var act = () => _cartService.AddAsync(game.Key, customerId);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowGameOutOfStockException_WhenGameIsOutOfStock()
    {
        var customerId = OrderTestData.UserId;
        var game = await SeedGameAsync(stock: 0);

        var act = () => _cartService.AddAsync(game.Key, customerId);

        await Assert.ThrowsAsync<GameOutOfStockException>(act);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateNewCart_WhenCartDoesNotExist()
    {
        var customerId = OrderTestData.UserId;
        var game = await SeedGameAsync();

        await _cartService.AddAsync(game.Key, customerId);

        Assert.True(await CartExistsAsync(customerId));
    }

    [Fact]
    public async Task AddAsync_ShouldAddGameToCart_WhenGameIsNotAlreadyInCart()
    {
        var customerId = OrderTestData.UserId;

        var game = await SeedGameAsync();
        var gameInStock = game.UnitInStock;

        await _cartService.AddAsync(game.Key, customerId);

        var cart = await GetCartAsync(customerId);

        Assert.NotNull(cart);
        Assert.Single(cart.OrderGames);
        Assert.True(await CheckProductStockAsync(game.Id, gameInStock - 1));
    }

    [Fact]
    public async Task AddAsync_ShouldIncrementItemQuantity_WhenGameIsAlreadyInCart()
    {
        var customerId = OrderTestData.UserId;

        var game = await SeedGameAsync();
        var gameInStock = game.UnitInStock;

        var (_, orderItem) = await SeedCartWithItemAsync(customerId, game);
        var orderItemQuantity = orderItem.Quantity;

        await _cartService.AddAsync(game.Key, customerId);

        var updatedCart = await GetCartAsync(customerId);

        Assert.NotNull(updatedCart);
        Assert.Single(updatedCart.OrderGames);

        var updatedOrderGame = updatedCart.OrderGames.First(og => og.Id == orderItem.Id);
        Assert.Equal(orderItemQuantity + 1, updatedOrderGame.Quantity);

        Assert.True(await CheckProductStockAsync(game.Id, gameInStock - 1));
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task RemoveAsync_ShouldThrowArgumentException_WhenGameKeyIsInvalid(string invalidGameKey)
    {
        var customerId = OrderTestData.UserId;

        var act = () => _cartService.RemoveAsync(invalidGameKey, customerId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task RemoveAsync_ShouldThrowArgumentException_WhenCustomerIdInvalid(Guid invalidCustomerId)
    {
        const string gameKey = "TestGameKey";

        var act = () => _cartService.RemoveAsync(gameKey, invalidCustomerId);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowNotFoundException_WhenCartDoesNotExist()
    {
        var customerId = OrderTestData.UserId;
        const string gameKey = "TestGameKey";

        var act = () => _cartService.RemoveAsync(gameKey, customerId);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowGameNotFoundException_WhenGameDoesNotExist()
    {
        var customerId = OrderTestData.UserId;

        await SeedCartWithItemsAsync(customerId);

        const string gameKey = "NonExistentGameKey";

        var act = () => _cartService.RemoveAsync(gameKey, customerId);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowGameNotFoundException_WhenGameIsDeletedAndCannotViewDeletedGames()
    {
        var customerId = OrderTestData.UserId;

        var game = await SeedDeletedGameAsync();
        await SeedCartWithItemAsync(customerId, game);

        var act = () => _cartService.RemoveAsync(game.Key, customerId);

        await Assert.ThrowsAsync<GameNotFoundException>(act);
    }

    [Fact]

    public async Task RemoveAsync_ShouldThrowNotFoundException_WhenGameDoesNotExistInCart()
    {
        var customerId = OrderTestData.UserId;

        await SeedCartWithItemsAsync(customerId);
        var game = await SeedGameAsync();

        var act = () => _cartService.RemoveAsync(game.Key, customerId);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveGameFromCart_WhenGameExistsInCart()
    {
        var customerId = OrderTestData.UserId;

        var cart = await SeedCartWithItemsAsync(customerId);
        var orderItem = cart.OrderGames.First();
        var orderItemQuantity = orderItem.Quantity;

        var game = GameTestData.GetGame(orderItem.ProductId);
        var gameInStock = game.UnitInStock;

        await _cartService.RemoveAsync(game.Key, customerId);

        var updatedCart = await GetCartAsync(customerId);

        Assert.NotNull(updatedCart);
        Assert.DoesNotContain(updatedCart.OrderGames, og => og.ProductId == game.Id);
        Assert.True(await CheckProductStockAsync(game.Id, gameInStock + orderItemQuantity));
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteCart_WhenLastGameIsRemoved()
    {
        var customerId = OrderTestData.UserId;

        var game = await SeedGameAsync();
        var gameInStock = game.UnitInStock;

        var (_, orderItem) = await SeedCartWithItemAsync(customerId, game);
        var orderItemQuantity = orderItem.Quantity;

        await _cartService.RemoveAsync(game.Key, customerId);

        Assert.False(await CartExistsAsync(customerId));
        Assert.True(await CheckProductStockAsync(game.Id, gameInStock + orderItemQuantity));
    }

    private Task<Order?> GetCartAsync(Guid customerId)
    {
        return _orderRepository.GetAsync(
            o => o.CustomerId == customerId && o.Status == OrderStatus.Open,
            include: x => x.Include(o => o.OrderGames));
    }

    private Task<bool> CheckProductStockAsync(Guid productId, int expectedStock)
    {
        return _gameRepository.ExistsAsync(g => g.Id == productId && g.UnitInStock == expectedStock);
    }

    private Task<bool> CartExistsAsync(Guid customerId)
    {
        return _orderRepository.ExistsAsync(o => o.CustomerId == customerId && o.Status == OrderStatus.Open);
    }

    private async Task<Order> SeedCartWithItemsAsync(Guid customerId)
    {
        var cart = OrderTestData.GetCart(customerId);
        var gameIds = cart.OrderGames.Select(og => og.ProductId).ToList();
        var games = GameTestData.GetGames(gameIds);

        await _gameRepository.AddRangeAsync(games);
        await _orderRepository.AddAsync(cart);
        await _unitOfWork.SaveChangesAsync();

        return cart;
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

    private async Task<(Order Cart, OrderGame OrderItem)> SeedCartWithItemAsync(Guid customerId, Game game)
    {
        var order = OrderTestData.GetEmptyCart(customerId);
        var orderGame = OrderTestData.GetOrderGame(game);
        orderGame.OrderId = order.Id;
        order.OrderGames.Add(orderGame);

        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return (order, orderGame);
    }
}