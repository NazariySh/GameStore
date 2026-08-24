using FluentValidation;
using Gamestore.BLL.DTOs.Games.Genres;

namespace Gamestore.BLL.Validators.Games.Genres;

public class UpdateGenreRequestValidator : AbstractValidator<UpdateGenreRequest>
{
    public UpdateGenreRequestValidator(IValidator<GenreUpdateDto> genreValidator)
    {
        RuleFor(x => x.Genre).SetValidator(genreValidator);
    }
}