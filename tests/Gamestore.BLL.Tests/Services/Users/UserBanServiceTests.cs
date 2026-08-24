using FluentValidation;
using Gamestore.BLL.Enums;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Services;
using Gamestore.BLL.Services.Users;
using Gamestore.BLL.Tests.Extensions;
using Gamestore.BLL.Tests.Factories;
using Gamestore.BLL.Tests.TestData;
using Gamestore.BLL.Tests.TestData.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.DAL.Services.Interfaces;
using Gamestore.Domain.Entities.Users;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.BLL.Tests.Services.Users;

public class UserBanServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<UserBan> _userBanRepository;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly UserBanService _userBanService;

    public UserBanServiceTests()
    {
        var mockUserContext = new Mock<IUserContext>();

        _unitOfWork = UnitOfWorkFactory.Create(mockUserContext.Object);
        _userBanRepository = _unitOfWork.Repositories.GetGeneric<UserBan>();
        var mapper = MapperFactory.Create();
        _mockValidationService = new Mock<IValidationService>();

        var mockEntityChangeLogService = new Mock<IEntityChangeLogService>();

        _userBanService = new UserBanService(
            new ServiceContext(_unitOfWork, mapper, _mockValidationService.Object, mockUserContext.Object),
            mockEntityChangeLogService.Object,
            Mock.Of<ILogger<UserBanService>>());
    }

    [Fact]
    public async Task BanUserAsync_ShouldThrowValidationException_WhenRequestIsInvalid()
    {
        var request = UserBanTestData.GetInvalidBanUserRequest();

        _mockValidationService.SetupValidationThrows(request);

        var act = () => _userBanService.BanUserAsync(request);

        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task BanUserAsync_ShouldCreateNewBan_WhenUserHasNoExistingBan()
    {
        var request = UserBanTestData.GetBanUserRequest();

        var beforeBan = DateTime.UtcNow;

        await _userBanService.BanUserAsync(request);

        var userBan = await GetUserBanAsync(request.User);

        Assert.NotNull(userBan);

        var actualBanPeriod = userBan.BanUntil - beforeBan;
        var expectedBanPeriod = request.Duration.BanPeriod;
        var tolerance = TimeSpan.FromSeconds(30);
        Assert.InRange(actualBanPeriod, expectedBanPeriod - tolerance, expectedBanPeriod + tolerance);
    }

    [Fact]
    public async Task BanUserAsync_ShouldBanUserPermanently_WhenDurationIsPermanent()
    {
        var request = UserBanTestData.GetBanUserRequest(duration: BanDuration.Permanent);

        await _userBanService.BanUserAsync(request);

        var userBan = await GetUserBanAsync(request.User);

        Assert.NotNull(userBan);
        Assert.Equal(DateTime.MaxValue, userBan.BanUntil);
    }

    [Fact]
    public async Task BanUserAsync_ShouldUpdateExistingBan_WhenUserHasExistingBan()
    {
        var existingBan = await SeedUserBanAsync();
        var request = UserBanTestData.GetBanUserRequest(existingBan.UserName);

        var beforeBan = DateTime.UtcNow;

        await _userBanService.BanUserAsync(request);

        var updatedUserBan = await GetUserBanAsync(request.User);

        Assert.NotNull(updatedUserBan);

        var actualBanPeriod = updatedUserBan.BanUntil - beforeBan;
        var expectedBanPeriod = request.Duration.BanPeriod;
        var tolerance = TimeSpan.FromSeconds(30);
        Assert.InRange(actualBanPeriod, expectedBanPeriod - tolerance, expectedBanPeriod + tolerance);
    }

    [Theory]
    [ClassData(typeof(InvalidStringTestData))]
    public async Task IsUserBannedAsync_ShouldThrowArgumentException_WhenUserNameIsInvalid(string invalidUserName)
    {
        var act = () => _userBanService.IsUserBannedAsync(invalidUserName);

        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task IsUserBannedAsync_ShouldReturnFalse_WhenUserIsNotBanned()
    {
        const string userName = UserBanTestData.UserName;

        var result = await _userBanService.IsUserBannedAsync(userName);

        Assert.False(result);
    }

    [Fact]
    public async Task IsUserBannedAsync_ShouldReturnTrue_WhenUserIsBanned()
    {
        var userBan = await SeedUserBanAsync();

        var result = await _userBanService.IsUserBannedAsync(userBan.UserName);

        Assert.True(result);
    }

    private Task<UserBan?> GetUserBanAsync(string userName)
    {
        return _userBanRepository.GetAsync(b => b.UserName == userName);
    }

    private async Task<UserBan> SeedUserBanAsync()
    {
        var userBan = UserBanTestData.GetUserBan();
        await _userBanRepository.AddAsync(userBan);
        await _unitOfWork.SaveChangesAsync();
        return userBan;
    }
}