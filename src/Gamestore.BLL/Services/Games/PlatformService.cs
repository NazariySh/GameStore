using System.Diagnostics.CodeAnalysis;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Platforms;
using Gamestore.BLL.DTOs.Logging;
using Gamestore.BLL.Interfaces;
using Gamestore.BLL.Interfaces.Games;
using Gamestore.BLL.Interfaces.Logging;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Enums;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Shared;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services.Games;

public class PlatformService : IPlatformService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Platform> _platformRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;
    private readonly IEntityChangeLogService _entityChangeLogService;
    private readonly ILogger<PlatformService> _logger;

    public PlatformService(
        IServiceContext context,
        IEntityChangeLogService entityChangeLogService,
        ILogger<PlatformService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _platformRepository = _unitOfWork.Repositories.GetGeneric<Platform>();
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();
        _mapper = context.Mapper;
        _validationService = context.Validation;
        _entityChangeLogService = entityChangeLogService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PlatformDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all platforms");

        var platforms = await _platformRepository.GetAllAsync<PlatformDto>(cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} platforms from database", platforms.Count);

        return platforms;
    }

    public async Task<IReadOnlyList<GameDto>> GetPlatformGamesAsync(Guid platformId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve games with platform '{PlatformId}'", platformId);

        Guard.AgainstEmpty(platformId);

        var games = await _gameRepository.GetAllAsync<GameDto>(
            game => game.GamePlatforms.Any(gp => gp.PlatformId == platformId),
            cancellationToken);

        _logger.LogInformation("Retrieved {Count} games with platform '{PlatformId}' from database", games.Count, platformId);

        return games;
    }

    public async Task<PlatformDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve platform with Id '{Id}'", id);

        Guard.AgainstEmpty(id);

        var platform = await _platformRepository.GetByIdAsync<PlatformDto>(id, cancellationToken);

        EnsurePlatformExists(platform, id);

        _logger.LogInformation("Retrieved platform with Id '{Id}' from database", id);

        return platform;
    }

    public async Task<CreatePlatformResponse> CreateAsync(CreatePlatformRequest request, CancellationToken cancellationToken = default)
    {
        var platform = request.Platform;

        _logger.LogInformation("Attempting to create a new platform with type '{Type}'", platform.Type);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var newPlatform = _mapper.Map<Platform>(platform);

        await _platformRepository.AddAsync(newPlatform, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Platform>(LogAction.Add, newVersion: newPlatform), cancellationToken);

        _logger.LogInformation("Platform with Id '{Id}' created successfully", newPlatform.Id);

        return new CreatePlatformResponse
        {
            Id = newPlatform.Id,
        };
    }

    public async Task UpdateAsync(UpdatePlatformRequest request, CancellationToken cancellationToken = default)
    {
        var platform = request.Platform;

        _logger.LogInformation("Attempting to update platform with Id '{Id}'", platform.Id);

        Guard.AgainstEmpty(platform.Id);

        await EnsurePlatformExistsAsync(platform.Id, cancellationToken);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var existingPlatform = await GetPlatformOrThrowAsync(platform.Id, cancellationToken);
        var oldVersion = _mapper.Map<Platform>(existingPlatform);

        _mapper.Map(platform, existingPlatform);

        await _platformRepository.UpdateAsync(existingPlatform, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Platform>(LogAction.Update, oldVersion, existingPlatform), cancellationToken);

        _logger.LogInformation("Platform with Id '{Id}' updated successfully", platform.Id);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete platform with Id '{Id}'", id);

        Guard.AgainstEmpty(id);

        var platform = await GetPlatformOrThrowAsync(id, cancellationToken);

        await _platformRepository.RemoveAsync(platform, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Platform>(LogAction.Delete, oldVersion: platform), cancellationToken);

        _logger.LogInformation("Platform with Id '{Id}' deleted successfully", id);
    }

    private async Task<Platform> GetPlatformOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var platform = await _platformRepository.GetByIdAsync(id, cancellationToken: cancellationToken);

        EnsurePlatformExists(platform, id);

        return platform;
    }

    private void EnsurePlatformExists<T>([NotNull] T? platform, Guid id)
    {
        if (platform is null)
        {
            ThrowPlatformNotFound(id);
        }
    }

    private async Task EnsurePlatformExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await PlatformExistsAsync(id, cancellationToken))
        {
            ThrowPlatformNotFound(id);
        }
    }

    private Task<bool> PlatformExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return _platformRepository.ExistsAsync(p => p.Id == id, cancellationToken);
    }

    [DoesNotReturn]
    private void ThrowPlatformNotFound(Guid id)
    {
        _logger.LogError("Platform with Id '{Id}' not found", id);
        throw new NotFoundException(nameof(Platform), id);
    }
}