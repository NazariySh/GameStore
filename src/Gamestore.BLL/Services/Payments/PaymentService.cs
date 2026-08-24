using System.Diagnostics.CodeAnalysis;
using Gamestore.BLL.Data.Interfaces;
using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.DTOs.Payments.PaymentMethods;
using Gamestore.BLL.Factories.Interfaces;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Payments;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Repositories.Interfaces.Games;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Entities.Orders;
using Gamestore.Domain.Entities.Payments;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGameRepository _gameRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderGame> _orderGameRepository;
    private readonly IRepository<PaymentMethod> _paymentMethodRepository;
    private readonly IValidationService _validationService;
    private readonly IPaymentProcessorFactory _paymentProcessorFactory;
    private readonly IProductRepositoryFacade _productRepositoryFacade;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IServiceContext context,
        IPaymentProcessorFactory paymentProcessorFactory,
        IProductRepositoryFacade productRepositoryFacade,
        ILogger<PaymentService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _gameRepository = _unitOfWork.Repositories.Get<IGameRepository>();
        _orderRepository = _unitOfWork.Repositories.GetGeneric<Order>();
        _orderGameRepository = _unitOfWork.Repositories.GetGeneric<OrderGame>();
        _paymentMethodRepository = _unitOfWork.Repositories.GetGeneric<PaymentMethod>();
        _validationService = context.Validation;
        _paymentProcessorFactory = paymentProcessorFactory;
        _productRepositoryFacade = productRepositoryFacade;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PaymentMethodDto>> GetPaymentMethodsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve available payment methods");

        var paymentMethods = await _paymentMethodRepository.GetAllAsync<PaymentMethodDto>(cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} payment methods from database", paymentMethods.Count);

        return paymentMethods;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, Guid customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to process payment '{PaymentMethod}' for account '{CustomerId}'", request.Method, customerId);

        Guard.AgainstEmpty(customerId);

        await EnsureCartExistsAsync(customerId, cancellationToken);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var cart = await GetCartOrThrowAsync(customerId, cancellationToken);
        var products = await GetCartProductsAsync(cart, cancellationToken);

        await ApplyCurrentProductPricesToCartAsync(cart, products, cancellationToken);

        var totalAmount = CalculateTotalAmount(cart);

        _logger.LogInformation("Total amount for order '{OrderId}' is {TotalAmount}", cart.Id, totalAmount);

        var paymentRequest = new OrderPaymentRequest
        {
            CustomerId = cart.CustomerId,
            OrderId = cart.Id,
            TotalAmount = totalAmount,
            CardDetails = request.Model,
        };

        return await HandlePaymentMethodAsync(request.Method, paymentRequest, cart, products, cancellationToken);
    }

    private async Task<PaymentResponse> HandlePaymentMethodAsync(
        string paymentMethod,
        OrderPaymentRequest paymentRequest,
        Order cart,
        ICollection<Game> products,
        CancellationToken cancellationToken)
    {
        var paymentProcessor = _paymentProcessorFactory.Create(paymentMethod);

        await UpdateOrderStatusAsync(cart, OrderStatus.Checkout, cancellationToken);

        try
        {
            var response = await paymentProcessor.ProcessPaymentAsync(paymentRequest, cancellationToken);

            var productKeys = products.Select(p => p.Key).ToList();
            await _productRepositoryFacade.RecalculateUnitInStockCountAsync(productKeys, cancellationToken);

            await UpdateOrderStatusAsync(cart, OrderStatus.Paid, cancellationToken);

            _logger.LogInformation("Payment processed successfully for order '{OrderId}'", cart.Id);

            return response;
        }
        catch (PaymentProcessingException ex)
        {
            _logger.LogError(ex, "Error processing payment for customer with Id '{CustomerId}'", cart.CustomerId);

            await RestoreProductsStockFromOrderAsync(cart, products, cancellationToken);

            await UpdateOrderStatusAsync(cart, OrderStatus.Cancelled, cancellationToken);

            _logger.LogInformation("Order '{OrderId}' has been cancelled", cart.Id);

            throw new PaymentProcessingException(
                ex.StatusCode,
                $"Order '{cart.Id}' cancelled. {ex.Message}",
                ex.Title,
                ex.Detail,
                ex);
        }
        catch
        {
            await UpdateOrderStatusAsync(cart, OrderStatus.Open, cancellationToken);

            throw;
        }
    }

    private async Task UpdateOrderStatusAsync(Order cart, OrderStatus status, CancellationToken cancellationToken)
    {
        cart.Status = status;

        await _orderRepository.UpdateAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order '{OrderId}' status updated to {Status}", cart.Id, cart.Status);
    }

    private static decimal CalculateTotalAmount(Order cart)
    {
        return cart.OrderGames.Sum(g =>
        {
            var discount = g.Discount ?? 0;
            return g.Price * g.Quantity * (1 - (discount / 100m));
        });
    }

    private async Task ApplyCurrentProductPricesToCartAsync(Order cart, ICollection<Game> products, CancellationToken cancellationToken)
    {
        var productPrices = products.ToDictionary(p => (p.Id, p.ProductId), p => p.Price);

        foreach (var cartItem in cart.OrderGames)
        {
            if (!productPrices.TryGetValue((cartItem.ProductId, cartItem.MongoProductId), out var price))
            {
                continue;
            }

            cartItem.Price = price;

            await _orderGameRepository.UpdateAsync(cartItem, cancellationToken);
        }
    }

    private async Task RestoreProductsStockFromOrderAsync(Order order, ICollection<Game> products, CancellationToken cancellationToken)
    {
        var orderProductQuantities = order.OrderGames.ToDictionary(og => (og.ProductId, og.MongoProductId), og => og.Quantity);

        foreach (var product in products)
        {
            if (!orderProductQuantities.TryGetValue((product.Id, product.ProductId), out var quantity))
            {
                continue;
            }

            product.UnitInStock += quantity;

            await _gameRepository.UpdateUnitInStockAsync(product, cancellationToken);
        }
    }

    private Task<ICollection<Game>> GetCartProductsAsync(Order cart, CancellationToken cancellationToken)
    {
        var gameIds = cart.OrderGames
            .Where(g => g.ProductId != Guid.Empty)
            .Select(g => g.ProductId)
            .ToList();

        var productIds = cart.OrderGames
            .Where(g => g.MongoProductId.HasValue)
            .Select(g => g.MongoProductId!.Value)
            .ToList();

        return _gameRepository.GetAllAsync(
            g => gameIds.Contains(g.Id) || (g.ProductId.HasValue && productIds.Contains(g.ProductId.Value)),
            cancellationToken);
    }

    private async Task<Order> GetCartOrThrowAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var cart = await _orderRepository.GetAsync(
            o => o.CustomerId == customerId && o.Status == OrderStatus.Open,
            include: x => x.Include(o => o.OrderGames),
            cancellationToken);

        if (cart is null)
        {
            ThrowCartNotFound(customerId);
        }

        return cart;
    }

    private async Task EnsureCartExistsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        if (!await CartExistsAsync(customerId, cancellationToken))
        {
            ThrowCartNotFound(customerId);
        }
    }

    private Task<bool> CartExistsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return _orderRepository.ExistsAsync(
            o => o.CustomerId == customerId && o.Status == OrderStatus.Open,
            cancellationToken);
    }

    [DoesNotReturn]
    private void ThrowCartNotFound(Guid customerId)
    {
        _logger.LogError("Cart for customer with Id '{CustomerId}' not found", customerId);
        throw new NotFoundException($"Cart for customer with Id '{customerId}' not found.");
    }
}