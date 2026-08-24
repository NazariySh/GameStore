using FluentValidation;
using Gamestore.BLL.DTOs.Games;
using Gamestore.BLL.Validators.Images;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;
using Gamestore.Domain.Shared;

namespace Gamestore.BLL.Validators.Games;

public class BaseGameRequestValidator : AbstractValidator<CreateUpdateGameRequest>
{
    private readonly IRepository<Genre> _genreRepository;
    private readonly IRepository<Platform> _platformRepository;
    private readonly IRepository<Publisher> _publisherRepository;

    public BaseGameRequestValidator(
        Base64ImageValidator imageValidator,
        IRepository<Genre> genreRepository,
        IRepository<Platform> platformRepository,
        IRepository<Publisher> publisherRepository)
    {
        _genreRepository = genreRepository;
        _platformRepository = platformRepository;
        _publisherRepository = publisherRepository;

        RuleFor(x => x.Genres)
            .NotEmpty().WithMessage("At least one genre is required.")
            .ForEach(genre => genre
                .NotEmpty().WithMessage("Genre Id is required.")
                .MustAsync(BeValidGenreIdAsync)
                .WithMessage((_, genreId) => $"Genre with Id {genreId} does not exist."));

        RuleFor(x => x.Platforms)
            .ForEach(platform => platform
                .NotEmpty().WithMessage("Platform Id is required.")
                .MustAsync(BeValidPlatformIdAsync)
                .WithMessage((_, platformId) => $"Platform with Id {platformId} does not exist."));

        RuleFor(x => x.Publisher)
            .NotEmpty().WithMessage("Publisher is required.")
            .MustAsync(BeValidPublisherIdAsync)
            .WithMessage(x => $"Publisher with Id {x.Publisher} does not exist.");

        RuleFor(x => x.Image)
            .SetValidator(imageValidator!)
            .When(NotEmptyImage);
    }

    private static bool NotEmptyImage(CreateUpdateGameRequest game)
    {
        return !string.IsNullOrWhiteSpace(game.Image);
    }

    private Task<bool> BeValidGenreIdAsync(EntityId id, CancellationToken cancellationToken)
    {
        return id.IsPrimary
            ? _genreRepository.ExistsAsync(g => g.Id == id.PrimaryId, cancellationToken)
            : _genreRepository.ExistsAsync(g => g.CategoryId == id.SecondaryId, cancellationToken);
    }

    private Task<bool> BeValidPlatformIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _platformRepository.ExistsAsync(
            p => p.Id == id,
            cancellationToken);
    }

    private Task<bool> BeValidPublisherIdAsync(EntityId id, CancellationToken cancellationToken)
    {
        return id.IsPrimary
            ? _publisherRepository.ExistsAsync(p => p.Id == id.PrimaryId, cancellationToken)
            : _publisherRepository.ExistsAsync(p => p.SupplierId == id.SecondaryId, cancellationToken);
    }
}