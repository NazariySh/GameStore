using FluentValidation;
using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Validators.Games.Genres;

public class GenreUpdateDtoValidator : AbstractValidator<GenreUpdateDto>
{
    private readonly IRepository<Genre> _genreRepository;

    public GenreUpdateDtoValidator(
        IRepository<Genre> genreRepository,
        IValidator<GenreCreateUpdateDto> baseValidator)
    {
        _genreRepository = genreRepository;

        Include(baseValidator);

        RuleFor(x => x)
            .MustAsync(BeUniqueNameAsync)
            .WithMessage("Genre with this name already exists.")
            .OverridePropertyName(x => x.Name);

        RuleFor(x => x)
            .MustAsync(NoCyclicReferenceAsync)
            .WithMessage("Cyclic reference detected in genre hierarchy.")
            .OverridePropertyName(x => x.ParentGenreId)
            .When(NotEmptyParentGenreId);
    }

    private static bool NotEmptyParentGenreId(GenreUpdateDto genre)
    {
        return genre.ParentGenreId.HasValue;
    }

    private async Task<bool> BeUniqueNameAsync(GenreUpdateDto genre, CancellationToken cancellationToken)
    {
        bool isUniqueName;

        if (genre.Id.IsPrimary)
        {
            isUniqueName = await _genreRepository.NotExistsAsync(
                g => g.Name == genre.Name && g.Id != genre.Id.PrimaryId,
                cancellationToken);
        }
        else
        {
            isUniqueName = await _genreRepository.NotExistsAsync(
                g => g.Name == genre.Name && g.CategoryId != genre.Id.SecondaryId,
                cancellationToken);
        }

        return isUniqueName;
    }

    private async Task<bool> NoCyclicReferenceAsync(GenreUpdateDto genre, CancellationToken cancellationToken)
    {
        if (!genre.Id.IsPrimary || genre.ParentGenreId is not { IsPrimary: true })
        {
            return true;
        }

        var genreId = genre.Id.PrimaryId.Value;
        var parentId = genre.ParentGenreId.Value.PrimaryId;

        while (parentId.HasValue)
        {
            if (parentId == genreId)
            {
                return false;
            }

            parentId = await _genreRepository.GetFirstSelectedAsync(
                g => g.Id == parentId.Value,
                g => g.ParentGenreId,
                cancellationToken);
        }

        return true;
    }
}