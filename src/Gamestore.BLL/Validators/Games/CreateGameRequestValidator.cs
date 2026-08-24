using FluentValidation;
using Gamestore.BLL.DTOs.Games;

namespace Gamestore.BLL.Validators.Games;

public class CreateGameRequestValidator : AbstractValidator<CreateGameRequest>
{
    public CreateGameRequestValidator(
        IValidator<CreateUpdateGameRequest> baseValidator,
        IValidator<GameCreateDto> gameValidator)
    {
        Include(baseValidator);

        RuleFor(x => x.Game).SetValidator(gameValidator);
    }
}