using FluentValidation;
using Gamestore.BLL.DTOs.Games.Genres;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Games;

namespace Gamestore.BLL.Validators.Games.Genres;

public class GenreCreateDtoValidator : AbstractValidator<GenreCreateDto>
{
    private readonly IRepository<Genre> _genreRepository;

    public GenreCreateDtoValidator(
        IRepository<Genre> genreRepository,
        IValidator<GenreCreateUpdateDto> baseValidator)
    {
        _genreRepository = genreRepository;

        Include(baseValidator);

        RuleFor(x => x.Name)
            .MustAsync(BeUniqueNameAsync)
            .WithMessage("Genre with this name already exists.");
    }

    private Task<bool> BeUniqueNameAsync(string name, CancellationToken cancellationToken)
    {
        return _genreRepository.NotExistsAsync(
            g => g.Name == name,
            cancellationToken);
    }
}