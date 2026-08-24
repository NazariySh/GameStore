using FluentValidation;

namespace Gamestore.BLL.Validators.Games;

public class GameKeyValidator : AbstractValidator<string>
{
    public GameKeyValidator()
    {
        RuleFor(x => x)
            .MinimumLength(GameValidationRules.MinKeyLength)
            .WithMessage($"Key must be at least {GameValidationRules.MinKeyLength} characters long.")
            .MaximumLength(GameValidationRules.MaxKeyLength)
            .WithMessage($"Key must not exceed {GameValidationRules.MaxKeyLength} characters.")
            .Matches(GameValidationRules.KeyPattern())
            .WithMessage(GameValidationRules.KeyPatternMessage);
    }
}