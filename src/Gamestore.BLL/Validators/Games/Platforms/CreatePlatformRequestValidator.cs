using FluentValidation;
using Gamestore.BLL.DTOs.Games.Platforms;

namespace Gamestore.BLL.Validators.Games.Platforms;

public class CreatePlatformRequestValidator : AbstractValidator<CreatePlatformRequest>
{
    public CreatePlatformRequestValidator(IValidator<PlatformCreateDto> platformValidator)
    {
        RuleFor(x => x.Platform).SetValidator(platformValidator);
    }
}