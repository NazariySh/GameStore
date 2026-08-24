using FluentValidation;
using Gamestore.BLL.Data.Interfaces;
using Gamestore.BLL.DTOs;
using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.DTOs.Payments.Bank;
using Gamestore.BLL.DTOs.Payments.PaymentMethods;
using Gamestore.BLL.Factories.Interfaces;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Payments.Processors;
using Gamestore.BLL.Services;
using Gamestore.BLL.Services.Payments;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Games;
using Gamestore.BLL.Tests.TestData.Orders;
using Gamestore.BLL.Tests.TestData.Payments;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Entities.Orders;
using Gamestore.Domain.Entities.Payments;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Exceptions;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Payments;

public class PaymentServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IRepository<PaymentMethod> _paymentMethodRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IPaymentProcessorFactory> _mockPaymentProcessorFactory;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        var mockUserContext = new Mock<IUserContext>();

        _unitOfWork = UnitOfWorkFactory.Create(mockUserContext.Object);
        _orderRepository = _unitOfWork.Repositories.GetGeneric<Order>();
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();
        _paymentMethodRepository = _unitOfWork.Repositories.GetGeneric<PaymentMethod>();
        var mockProductRepositoryFacade = new Mock<IProductRepositoryFacade>();

        _mapper = MapperFactory.Create();
        _mockPaymentProcessorFactory = new Mock<IPaymentProcessorFactory>();
        _mockValidationService = new Mock<IValidationService>();

        _paymentService = new PaymentService(
            new ServiceContext(_unitOfWork, _mapper, _mockValidationService.Object, mockUserContext.Object),
            _mockPaymentProcessorFactory.Object,
            mockProductRepositoryFacade.Object,
            Mock.Of<ILogger<PaymentService>>());
    }

    [Fact]
    public async Task GetPaymentMethodsAsync_ShouldReturnPaymentMethods_WhenPaymentMethodsExist()
    {
        var paymentMethods = await SeedPaymentMethodsAsync();

        var paymentMethodDto = _mapper.Map<List<PaymentMethodDto>>(paymentMethods);

        var result = await _paymentService.GetPaymentMethodsAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(paymentMethodDto, result);
    }

    [Fact]
    public async Task GetPaymentMethodsAsync_ShouldReturnEmptyList_WhenNoPaymentMethodsExist()
    {
        var result = await _paymentService.GetPaymentMethodsAsync();

        Assert.Empty(result);
    }

    [Theory]
    [ClassData(typeof(InvalidGuidTestData))]
    public async Task ProcessPaymentAsync_ShouldThrowArgumentException_WhenCustomerIdIsInvalid(Guid invalidCustomerId)
    {
        var paymentRequest = new PaymentRequest
        {
            Method = PaymentMethodTestData.Bank,
        };

        var act = () => _paymentService.ProcessPaymentAsync(paymentRequest, invalidCustomerId);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowNotFoundException_WhenCartDoesNotExist()
    {
        var customerId = OrderTestData.UserId;
        var paymentRequest = new PaymentRequest
        {
            Method = PaymentMethodTestData.Bank,
        };

        var act = () => _paymentService.ProcessPaymentAsync(paymentRequest, customerId);

        await Assert.ThrowsAsync<NotFoundException>(act);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldThrowValidationException_WhenRequestInvalid()
    {
        var customerId = OrderTestData.UserId;
        var paymentRequest = new PaymentRequest
        {
            Method = PaymentMethodTestData.Visa,
            Model = null,
        };

        await SeedCartWithItemsAsync(customerId);

        _mockValidationService.SetupValidationThrows(paymentRequest);

        var act = () => _paymentService.ProcessPaymentAsync(paymentRequest, customerId);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldCancelOrderAndThrowException_WhenProcessorThrowsPaymentProcessingException()
    {
        var customerId = OrderTestData.UserId;
        var paymentRequest = new PaymentRequest
        {
            Method = PaymentMethodTestData.Bank,
        };

        var cart = await SeedCartWithItemsAsync(customerId);

        var processor = new Mock<IPaymentProcessor>();
        SetupMockPaymentProcessorFactoryCreate(paymentRequest.Method, processor.Object);
        SetupMockPaymentProcessorProcessPaymentThrows(processor);

        var act = () => _paymentService.ProcessPaymentAsync(paymentRequest, customerId);

        await Assert.ThrowsAsync<PaymentProcessingException>(act);

        Assert.True(await CheckOrderStatus(cart.Id, OrderStatus.Cancelled));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldProcessOrderPayment_WhenRequestIsValid()
    {
        var customerId = OrderTestData.UserId;
        var paymentRequest = new PaymentRequest
        {
            Method = PaymentMethodTestData.Bank,
        };

        var cart = await SeedCartWithItemsAsync(customerId);

        var expectedResponse = new BankPaymentResponse
        {
            InvoiceFile = new DownloadFileContentDto
            {
                FileName = "invoice.pdf",
                ContentType = "application/pdf",
                Content = [1, 2, 3, 4, 5],
            },
        };

        var processor = new Mock<IPaymentProcessor>();
        SetupMockPaymentProcessorFactoryCreate(paymentRequest.Method, processor.Object);
        SetupMockPaymentProcessorProcessPayment(processor, expectedResponse);

        var result = await _paymentService.ProcessPaymentAsync(paymentRequest, customerId);

        var response = Assert.IsType<BankPaymentResponse>(result);

        Assert.NotNull(response);
        Assert.Equivalent(expectedResponse, response);

        Assert.True(await CheckOrderStatus(cart.Id, OrderStatus.Paid));
    }

    private void SetupMockPaymentProcessorFactoryCreate(string method, IPaymentProcessor paymentProcessor)
    {
        _mockPaymentProcessorFactory
            .Setup(factory => factory.Create(method))
            .Returns(paymentProcessor);
    }

    private static void SetupMockPaymentProcessorProcessPaymentThrows(Mock<IPaymentProcessor> mockProcessor)
    {
        mockProcessor.Setup(p => p.ProcessPaymentAsync(
                It.IsAny<OrderPaymentRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PaymentProcessingException("Payment processing failed."));
    }

    private static void SetupMockPaymentProcessorProcessPayment(Mock<IPaymentProcessor> mockProcessor, PaymentResponse response)
    {
        mockProcessor.Setup(p => p.ProcessPaymentAsync(
                It.IsAny<OrderPaymentRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    private Task<bool> CheckOrderStatus(Guid orderId, OrderStatus status)
    {
        return _orderRepository.ExistsAsync(o => o.Id == orderId && o.Status == status);
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

    private async Task<IReadOnlyList<PaymentMethod>> SeedPaymentMethodsAsync()
    {
        var paymentMethods = PaymentMethodTestData.GetPaymentMethods();

        await _paymentMethodRepository.AddRangeAsync(paymentMethods);
        await _unitOfWork.SaveChangesAsync();

        return paymentMethods;
    }
}