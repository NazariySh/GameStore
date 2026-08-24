using System.Linq.Expressions;
using Gamestore.BLL.DTOs.Shippers;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Services;
using Gamestore.BLL.Services.Shippers;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData.Shippers;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Entities.Shippers;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Shippers;

public class ShipperServiceTests
{
    private readonly Mock<IRepository<Shipper>> _mockShipperRepository;
    private readonly IMapper _mapper;
    private readonly ShipperService _shipperService;

    public ShipperServiceTests()
    {
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockShipperRepository = new Mock<IRepository<Shipper>>();
        mockUnitOfWork.Setup(x => x.Repositories.GetGeneric<Shipper>())
            .Returns(_mockShipperRepository.Object);

        _mapper = MapperFactory.Create();
        var mockValidationService = new Mock<IValidationService>();
        var mockUserContext = new Mock<IUserContext>();

        _shipperService = new ShipperService(
            new ServiceContext(mockUnitOfWork.Object, _mapper, mockValidationService.Object, mockUserContext.Object),
            Mock.Of<ILogger<ShipperService>>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnShippers_WhenShippersExist()
    {
        var shippers = ShipperTestData.GetShippers();

        var shipperDtos = _mapper.Map<List<ShipperDto>>(shippers);

        SetupMockShipperRepositoryGetAll(shipperDtos);

        var result = await _shipperService.GetAllAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(shipperDtos, result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoShippersExist()
    {
        SetupMockShipperRepositoryGetAll([]);

        var result = await _shipperService.GetAllAsync();

        Assert.Empty(result);
    }

    private void SetupMockShipperRepositoryGetAll(List<ShipperDto> shippers)
    {
        _mockShipperRepository.Setup(r => r.GetAllAsync<ShipperDto>(
                It.IsAny<Expression<Func<Shipper, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shippers);
    }
}