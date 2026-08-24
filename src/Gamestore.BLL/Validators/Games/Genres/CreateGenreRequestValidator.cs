using FluentValidation;
using Gamestore.BLL.DTOs.Games.Genres;

namespace Gamestore.BLL.Validators.Games.Genres;

public class CreateGenreRequestValidator : AbstractValidator<CreateGenreRequest>
{
    public CreateGenreRequestValidator(IValidator<GenreCreateDto> genreValidator)
    {
        RuleFor(x => x.Genre).SetValidator(genreValidator);
    }
}