using FluentValidation;
using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Validators.Games.Genres;

public class BaseGenreDtoValidator : AbstractValidator<GenreCreateUpdateDto>
{
    private readonly IRepository<Genre> _genreRepository;

    public BaseGenreDtoValidator(IRepository<Genre> genreRepository)
    {
        _genreRepository = genreRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(GenreValidationRules.MinNameLength)
            .WithMessage($"Name must be at least {GenreValidationRules.MinNameLength} characters long.")
            .MaximumLength(GenreValidationRules.MaxNameLength)
            .WithMessage($"Name must not exceed {GenreValidationRules.MaxNameLength} characters.")
            .Matches(GenreValidationRules.NamePattern())
            .WithMessage(GenreValidationRules.NamePatternMessage);

        RuleFor(x => x)
            .MustAsync(BeValidParentGenreIdAsync)
            .WithMessage(x => $"Parent genre with Id {x.ParentGenreId} does not exist.")
            .OverridePropertyName(x => x.ParentGenreId)
            .When(NotEmptyParentGenreId);
    }

    private static bool NotEmptyParentGenreId(GenreCreateUpdateDto genre)
    {
        return genre.ParentGenreId.HasValue;
    }

    private async Task<bool> BeValidParentGenreIdAsync(GenreCreateUpdateDto genre, CancellationToken cancellationToken)
    {
        if (!genre.ParentGenreId.HasValue)
        {
            return true;
        }

        var parentGenreId = genre.ParentGenreId.Value;
        return parentGenreId.IsPrimary
            ? await _genreRepository.ExistsAsync(g => g.Id == parentGenreId.PrimaryId, cancellationToken)
            : await _genreRepository.ExistsAsync(g => g.CategoryId == parentGenreId.SecondaryId, cancellationToken);
    }
}