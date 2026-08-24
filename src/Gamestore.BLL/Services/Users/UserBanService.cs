using Gamestore.BLL.DTOs.Logging;
using Gamestore.BLL.DTOs.Users;
using Gamestore.BLL.Enums;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.BLL.Interfaces.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Shared;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Users;

public class UserBanService : IUserBanService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<UserBan> _userBanRepository;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;
    private readonly IEntityChangeLogService _entityChangeLogService;
    private readonly ILogger<UserBanService> _logger;

    public UserBanService(
        IServiceContext context,
        IEntityChangeLogService entityChangeLogService,
        ILogger<UserBanService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _userBanRepository = _unitOfWork.Repositories.GetGeneric<UserBan>();
        _mapper = context.Mapper;
        _validationService = context.Validation;
        _entityChangeLogService = entityChangeLogService;
        _logger = logger;
    }

    public async Task<bool> IsUserBannedAsync(string userName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking if user '{UserName}' is banned", userName);

        Guard.AgainstNullOrWhiteSpace(userName);

        var normalizedName = userName.ToUpperInvariant();

        var isUserBanned = await _userBanRepository.ExistsAsync(
            u =>
                u.UserName.ToUpper().Equals(normalizedName) &&
                u.BanUntil > DateTime.UtcNow,
            cancellationToken);

        if (isUserBanned)
        {
            _logger.LogInformation("User '{UserName}' is currently banned", userName);
        }
        else
        {
            _logger.LogInformation("User '{UserName}' is not banned", userName);
        }

        return isUserBanned;
    }

    public async Task BanUserAsync(BanUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to ban user '{User}' for duration '{Duration}'", request.User, request.Duration.DisplayName);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var existingBan = await GetUserBanAsync(request.User, cancellationToken);

        if (existingBan is null)
        {
            await CreateUserBanAsync(request, cancellationToken);
        }
        else
        {
            await UpdateUserBanAsync(existingBan, request.Duration, cancellationToken);
        }
    }

    private async Task CreateUserBanAsync(BanUserRequest request, CancellationToken cancellationToken)
    {
        var banUntil = GetBanExpirationDate(request.Duration);

        var newUserBan = new UserBan
        {
            UserName = request.User,
            BanUntil = banUntil,
        };

        await _userBanRepository.AddAsync(newUserBan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<UserBan>(LogAction.Add, newVersion: newUserBan), cancellationToken);

        _logger.LogInformation("User '{User}' has been banned until '{BanUntil}'", request.User, banUntil);
    }

    private async Task UpdateUserBanAsync(UserBan existingBan, BanDuration banDuration, CancellationToken cancellationToken)
    {
        var oldVersion = _mapper.Map<UserBan>(existingBan);
        var banUntil = GetBanExpirationDate(banDuration);

        existingBan.BanUntil = banUntil;

        await _userBanRepository.UpdateAsync(existingBan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<UserBan>(LogAction.Update, oldVersion, existingBan), cancellationToken);

        _logger.LogInformation("User '{User}' already has a ban, updated ban until '{BanUntil}'", existingBan.UserName, banUntil);
    }

    private static DateTime GetBanExpirationDate(BanDuration duration)
    {
        return duration.BanPeriod == TimeSpan.MaxValue
            ? DateTime.MaxValue
            : DateTime.UtcNow.Add(duration.BanPeriod);
    }

    private Task<UserBan?> GetUserBanAsync(string userName, CancellationToken cancellationToken)
    {
        var normalizedName = userName.ToUpperInvariant();
        return _userBanRepository.GetAsync(
            u => u.UserName.ToUpper().Equals(normalizedName),
            cancellationToken: cancellationToken);
    }
}