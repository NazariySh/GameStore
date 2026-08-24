using System.Diagnostics.CodeAnalysis;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Genres;
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

public class GenreService : IGenreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Genre> _genreRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;
    private readonly IEntityChangeLogService _entityChangeLogService;
    private readonly ILogger<GenreService> _logger;

    public GenreService(
        IServiceContext context,
        IEntityChangeLogService entityChangeLogService,
        ILogger<GenreService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _genreRepository = _unitOfWork.Repositories.GetGeneric<Genre>();
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();
        _mapper = context.Mapper;
        _validationService = context.Validation;
        _entityChangeLogService = entityChangeLogService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all genres");

        var genres = await _genreRepository.GetAllAsync<GenreDto>(cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} genres from database", genres.Count);

        return genres;
    }

    public async Task<IReadOnlyList<GenreDto>> GetSubGenresAsync(string parentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve sub-genres for parent genre with Id '{ParentId}'", parentId);

        var parentGenreId = EntityId.Parse(parentId);

        Guard.AgainstEmpty(parentGenreId);

        IReadOnlyList<GenreDto> genres = [];

        if (parentGenreId.IsPrimary)
        {
            genres = await _genreRepository.GetAllAsync<GenreDto>(
                g => g.ParentGenreId == parentGenreId.PrimaryId,
                cancellationToken);
        }

        _logger.LogInformation("Retrieved {Count} sub-genres for parent genre with Id '{ParentId}' from database", genres.Count, parentId);

        return genres;
    }

    public async Task<IReadOnlyList<GameDto>> GetGenreGamesAsync(string genreId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve games with genre '{GenreId}'", genreId);

        var id = EntityId.Parse(genreId);

        Guard.AgainstEmpty(id);

        IReadOnlyList<GameDto> games;

        if (id.IsPrimary)
        {
            games = await _gameRepository.GetAllAsync<GameDto>(
                game => game.GameGenres.Any(gg => gg.GenreId == id.PrimaryId),
                cancellationToken);
        }
        else
        {
            games = await _gameRepository.GetAllAsync<GameDto>(
                game => game.CategoryId == id.SecondaryId,
                cancellationToken);
        }

        _logger.LogInformation("Retrieved {Count} games with genre '{GenreId}' from database", games.Count, genreId);

        return games;
    }

    public async Task<GenreDetailedDto> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve genre with Id '{Id}'", id);

        var genreId = EntityId.Parse(id);

        Guard.AgainstEmpty(genreId);

        var genre = genreId.IsPrimary
            ? await _genreRepository.GetByIdAsync<GenreDetailedDto>(genreId.PrimaryId.Value, cancellationToken)
            : await _genreRepository.GetAsync<GenreDetailedDto>(g => g.CategoryId == genreId.SecondaryId, cancellationToken);

        EnsureGenreExists(genre, genreId);

        _logger.LogInformation("Retrieved genre with Id '{Id}' from database", id);

        return genre;
    }

    public async Task<CreateGenreResponse> CreateAsync(CreateGenreRequest request, CancellationToken cancellationToken = default)
    {
        var genre = request.Genre;

        _logger.LogInformation("Attempting to create a new genre with name '{Name}'", genre.Name);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var newGenre = _mapper.Map<Genre>(genre);

        await _genreRepository.AddAsync(newGenre, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Genre>(LogAction.Add, newVersion: newGenre), cancellationToken);

        _logger.LogInformation("Genre with Id '{Id}' created successfully", newGenre.Id);

        return new CreateGenreResponse
        {
            Id = newGenre.Id,
        };
    }

    public async Task UpdateAsync(UpdateGenreRequest request, CancellationToken cancellationToken = default)
    {
        var genre = request.Genre;

        _logger.LogInformation("Attempting to update genre with Id '{Id}'", genre.Id);

        Guard.AgainstEmpty(genre.Id);

        await EnsureGenreExistsAsync(genre.Id, cancellationToken);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var existingGenre = await GetGenreOrThrowAsync(genre.Id, cancellationToken);
        var oldVersion = _mapper.Map<Genre>(existingGenre);

        _mapper.Map(genre, existingGenre);

        await _genreRepository.UpdateAsync(existingGenre, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Genre>(LogAction.Update, oldVersion, existingGenre), cancellationToken);

        _logger.LogInformation("Genre with Id '{Id}' updated successfully", genre.Id);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete genre with Id '{Id}'", id);

        var genreId = EntityId.Parse(id);

        Guard.AgainstEmpty(genreId);

        var genre = await GetGenreOrThrowAsync(genreId, cancellationToken);

        await UnlinkSubGenresAsync(genre, cancellationToken);

        await _genreRepository.RemoveAsync(genre, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Genre>(LogAction.Delete, oldVersion: genre), cancellationToken);

        _logger.LogInformation("Genre with Id '{Id}' deleted successfully", id);
    }

    private async Task UnlinkSubGenresAsync(Genre genre, CancellationToken cancellationToken)
    {
        var subGenres = await _genreRepository.GetAllAsync(
            g => g.ParentGenreId == genre.Id,
            cancellationToken);

        foreach (var subGenre in subGenres)
        {
            subGenre.ParentGenreId = null;
            await _genreRepository.UpdateAsync(subGenre, cancellationToken);
        }
    }

    private async Task<Genre> GetGenreOrThrowAsync(EntityId id, CancellationToken cancellationToken)
    {
        var genre = id.IsPrimary
            ? await _genreRepository.GetByIdAsync(id.PrimaryId.Value, cancellationToken: cancellationToken)
            : await _genreRepository.GetAsync(g => g.CategoryId == id.SecondaryId, cancellationToken: cancellationToken);

        EnsureGenreExists(genre, id);

        return genre;
    }

    private void EnsureGenreExists<T>([NotNull] T? genre, EntityId id)
    {
        if (genre is null)
        {
            ThrowGenreNotFound(id);
        }
    }

    private async Task EnsureGenreExistsAsync(EntityId id, CancellationToken cancellationToken)
    {
        if (!await GenreExistsAsync(id, cancellationToken))
        {
            ThrowGenreNotFound(id);
        }
    }

    private Task<bool> GenreExistsAsync(EntityId id, CancellationToken cancellationToken)
    {
        return id.IsPrimary
            ? _genreRepository.ExistsAsync(g => g.Id == id.PrimaryId, cancellationToken)
            : _genreRepository.ExistsAsync(g => g.CategoryId == id.SecondaryId, cancellationToken);
    }

    [DoesNotReturn]
    private void ThrowGenreNotFound(EntityId id)
    {
        _logger.LogError("Genre with Id '{Id}' not found", id.ToString());
        throw new NotFoundException(nameof(Genre), id);
    }
}