using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.DTOs.Games.Publishers;
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

public class PublisherService : IPublisherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Publisher> _publisherRepository;
    private readonly IRepository<Game> _gameRepository;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;
    private readonly IEntityChangeLogService _entityChangeLogService;
    private readonly ILogger<PublisherService> _logger;

    public PublisherService(
        IServiceContext context,
        IEntityChangeLogService entityChangeLogService,
        ILogger<PublisherService> logger)
    {
        _unitOfWork = context.UnitOfWork;
        _publisherRepository = _unitOfWork.Repositories.GetGeneric<Publisher>();
        _gameRepository = _unitOfWork.Repositories.GetGeneric<Game>();
        _mapper = context.Mapper;
        _validationService = context.Validation;
        _entityChangeLogService = entityChangeLogService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PublisherDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve all publishers");

        var publishers = await _publisherRepository.GetAllAsync<PublisherDto>(cancellationToken: cancellationToken);

        _logger.LogInformation("Retrieved {Count} publishers from database", publishers.Count);

        return publishers;
    }

    public async Task<IReadOnlyList<GameDto>> GetPublisherGamesAsync(string companyName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve games with publisher '{CompanyName}'", companyName);

        Guard.AgainstNullOrWhiteSpace(companyName);

        var publisher = await GetPublisherOrThrowAsync(
            companyName,
            p => new Publisher { Id = p.Id, SupplierId = p.SupplierId },
            cancellationToken);

        var games = await _gameRepository.GetAllAsync<GameDto>(
            game =>
                (publisher.Id != Guid.Empty && game.PublisherId == publisher.Id) ||
                (publisher.SupplierId.HasValue && game.SupplierId == publisher.SupplierId),
            cancellationToken);

        _logger.LogInformation("Retrieved {Count} games with publisher '{CompanyName}' from database", games.Count, companyName);

        return games;
    }

    public async Task<PublisherDto> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve publisher with Id '{Id}'", id);

        var publisherId = EntityId.Parse(id);

        Guard.AgainstEmpty(publisherId);

        var publisher = publisherId.IsPrimary
            ? await _publisherRepository.GetByIdAsync<PublisherDto>(publisherId.PrimaryId.Value, cancellationToken)
            : await _publisherRepository.GetAsync<PublisherDto>(p => p.SupplierId == publisherId.SecondaryId, cancellationToken);

        EnsurePublisherExists(publisher, publisherId);

        _logger.LogInformation("Retrieved publisher with Id '{Id}' from database", id);

        return publisher;
    }

    public async Task<PublisherDto> GetByCompanyNameAsync(string companyName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to retrieve publisher with company name '{CompanyName}'", companyName);

        Guard.AgainstNullOrWhiteSpace(companyName);

        var publisher = await _publisherRepository.GetSingleAsync<PublisherDto>(
            p => p.CompanyName == companyName,
            cancellationToken);

        EnsurePublisherWithNameExists(publisher, companyName);

        _logger.LogInformation("Retrieved publisher with company name '{CompanyName}' from database", companyName);

        return publisher;
    }

    public async Task<CreatePublisherResponse> CreateAsync(CreatePublisherRequest request, CancellationToken cancellationToken = default)
    {
        var publisher = request.Publisher;

        _logger.LogInformation("Attempting to create a new publisher with company name '{CompanyName}'", publisher.CompanyName);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var newPublisher = _mapper.Map<Publisher>(publisher);

        await _publisherRepository.AddAsync(newPublisher, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Publisher>(LogAction.Add, newVersion: newPublisher), cancellationToken);

        _logger.LogInformation("Publisher with Id '{Id}' created successfully", newPublisher.Id);

        return new CreatePublisherResponse
        {
            Id = newPublisher.Id,
        };
    }

    public async Task UpdateAsync(UpdatePublisherRequest request, CancellationToken cancellationToken = default)
    {
        var publisher = request.Publisher;

        _logger.LogInformation("Attempting to update publisher with Id '{Id}'", publisher.Id);

        Guard.AgainstEmpty(publisher.Id);

        await EnsurePublisherExistsAsync(publisher.Id, cancellationToken);

        await _validationService.ValidateAndThrowAsync(request, cancellationToken);

        var existingPublisher = await GetPublisherOrThrowAsync(publisher.Id, cancellationToken);
        var oldVersion = _mapper.Map<Publisher>(existingPublisher);

        _mapper.Map(publisher, existingPublisher);

        await _publisherRepository.UpdateAsync(existingPublisher, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Publisher>(LogAction.Update, oldVersion, existingPublisher), cancellationToken);

        _logger.LogInformation("Publisher with Id '{Id}' updated successfully", publisher.Id);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete publisher with Id '{Id}'", id);

        var publisherId = EntityId.Parse(id);

        Guard.AgainstEmpty(publisherId);

        var publisher = await GetPublisherOrThrowAsync(publisherId, cancellationToken);

        await EnsureHasNoGamesAsync(publisher, cancellationToken);

        await _publisherRepository.RemoveAsync(publisher, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _entityChangeLogService.LogChangeAsync(new ChangeLogDto<Publisher>(LogAction.Delete, oldVersion: publisher), cancellationToken);

        _logger.LogInformation("Publisher with Id '{Id}' deleted successfully", id);
    }

    private async Task<Publisher> GetPublisherOrThrowAsync(
        string companyName,
        Expression<Func<Publisher, Publisher>> selector,
        CancellationToken cancellationToken)
    {
        var publisher = await _publisherRepository.GetFirstSelectedAsync(
            p => p.CompanyName == companyName,
            selector,
            cancellationToken);

        EnsurePublisherWithNameExists(publisher, companyName);

        return publisher;
    }

    private async Task<Publisher> GetPublisherOrThrowAsync(EntityId id, CancellationToken cancellationToken)
    {
        var publisher = id.IsPrimary
            ? await _publisherRepository.GetByIdAsync(id.PrimaryId.Value, cancellationToken: cancellationToken)
            : await _publisherRepository.GetAsync(p => p.SupplierId == id.SecondaryId, cancellationToken: cancellationToken);

        EnsurePublisherExists(publisher, id);

        return publisher;
    }

    private void EnsurePublisherWithNameExists<T>([NotNull] T? publisher, string companyName)
    {
        if (publisher is null)
        {
            _logger.LogError("Publisher with company name '{CompanyName}' not found", companyName);
            throw new NotFoundException($"Publisher with company name '{companyName}' not found.");
        }
    }

    private void EnsurePublisherExists<T>([NotNull] T? publisher, EntityId id)
    {
        if (publisher is null)
        {
            ThrowPublisherNotFound(id);
        }
    }

    private async Task EnsurePublisherExistsAsync(EntityId id, CancellationToken cancellationToken)
    {
        if (!await PublisherExistsAsync(id, cancellationToken))
        {
            ThrowPublisherNotFound(id);
        }
    }

    private async Task EnsureHasNoGamesAsync(Publisher publisher, CancellationToken cancellationToken)
    {
        if (await PublisherHasGamesAsync(publisher, cancellationToken))
        {
            _logger.LogError("Publisher '{PublisherName}' has associated games and cannot be deleted", publisher.CompanyName);
            throw new ConflictException($"Publisher '{publisher.CompanyName}' cannot be deleted because it has associated games.");
        }
    }

    private Task<bool> PublisherHasGamesAsync(Publisher publisher, CancellationToken cancellationToken)
    {
        return _gameRepository.ExistsAsync(
            g =>
                (publisher.Id != Guid.Empty && g.PublisherId == publisher.Id) ||
                (publisher.SupplierId.HasValue && g.SupplierId == publisher.SupplierId),
            cancellationToken);
    }

    private Task<bool> PublisherExistsAsync(EntityId id, CancellationToken cancellationToken)
    {
        return id.IsPrimary
            ? _publisherRepository.ExistsAsync(p => p.Id == id.PrimaryId, cancellationToken)
            : _publisherRepository.ExistsAsync(p => p.SupplierId == id.SecondaryId, cancellationToken);
    }

    [DoesNotReturn]
    private void ThrowPublisherNotFound(EntityId id)
    {
        _logger.LogError("Publisher with Id '{Id}' not found", id.ToString());
        throw new NotFoundException(nameof(Publisher), id);
    }
}