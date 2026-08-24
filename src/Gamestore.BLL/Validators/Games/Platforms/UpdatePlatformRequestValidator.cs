using FluentValidation;
using Gamestore.BLL.DTOs.Games.Platforms;

namespace Gamestore.BLL.Validators.Games.Platforms;

public class UpdatePlatformRequestValidator : AbstractValidator<UpdatePlatformRequest>
{
    public UpdatePlatformRequestValidator(IValidator<PlatformUpdateDto> platformValidator)
    {
        RuleFor(x => x.Platform).SetValidator(platformValidator);
    }
}