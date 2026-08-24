using FluentValidation;
using Gamestore.BLL.DTOs.Games;

namespace Gamestore.BLL.Validators.Games;

public class UpdateGameRequestValidator : AbstractValidator<UpdateGameRequest>
{
    public UpdateGameRequestValidator(
        IValidator<CreateUpdateGameRequest> baseValidator,
        IValidator<GameUpdateDto> gameValidator)
    {
        Include(baseValidator);

        RuleFor(x => x.Game).SetValidator(gameValidator);
    }
}