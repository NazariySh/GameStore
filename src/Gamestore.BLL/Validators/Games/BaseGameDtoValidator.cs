using FluentValidation;
using Gamestore.BLL.DTOs.Games;

namespace Gamestore.BLL.Validators.Games;

public class BaseGameDtoValidator : AbstractValidator<GameCreateUpdateDto>
{
    public BaseGameDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(GameValidationRules.MinNameLength)
            .WithMessage($"Name must be at least {GameValidationRules.MinNameLength} characters long.")
            .MaximumLength(GameValidationRules.MaxNameLength)
            .WithMessage($"Name must not exceed {GameValidationRules.MaxNameLength} characters.")
            .Matches(GameValidationRules.NamePattern())
            .WithMessage(GameValidationRules.NamePatternMessage);

        RuleFor(x => x.Description)
            .MaximumLength(GameValidationRules.MaxDescriptionLength)
            .WithMessage($"Description must not exceed {GameValidationRules.MaxDescriptionLength} characters.")
            .When(NotEmptyDescription);

        RuleFor(x => x.Price)
            .InclusiveBetween(GameValidationRules.MinPrice, GameValidationRules.MaxPrice)
            .WithMessage($"Price must be between {GameValidationRules.MinPrice} and {GameValidationRules.MaxPrice}.");

        RuleFor(x => x.UnitInStock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Unit in stock cannot be negative.");

        RuleFor(x => x.Discount)
            .InclusiveBetween(GameValidationRules.MinDiscount, GameValidationRules.MaxDiscount)
            .WithMessage($"Discount must be between {GameValidationRules.MinDiscount} and {GameValidationRules.MaxDiscount}.");
    }

    private static bool NotEmptyDescription(GameCreateUpdateDto game)
    {
        return !string.IsNullOrEmpty(game.Description);
    }
}