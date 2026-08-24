using FluentValidation;
using Gamestore.BLL.DTOs.Games.Platforms;

namespace Gamestore.BLL.Validators.Games.Platforms;

public class BasePlatformDtoValidator : AbstractValidator<PlatformCreateUpdateDto>
{
    public BasePlatformDtoValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .MinimumLength(PlatformValidationRules.MinTypeLength)
            .WithMessage($"Type must be at least {PlatformValidationRules.MinTypeLength} characters long.")
            .MaximumLength(PlatformValidationRules.MaxTypeLength)
            .WithMessage($"Type must not exceed {PlatformValidationRules.MaxTypeLength} characters.")
            .Matches(PlatformValidationRules.TypePattern())
            .WithMessage(PlatformValidationRules.TypePatternMessage);
    }
}