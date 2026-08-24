using FluentValidation;
using Gamestore.BLL.DTOs.Games.Publishers;

namespace Gamestore.BLL.Validators.Games.Publishers;

public class UpdatePublisherRequestValidator : AbstractValidator<UpdatePublisherRequest>
{
    public UpdatePublisherRequestValidator(IValidator<PublisherUpdateDto> publisherValidator)
    {
        RuleFor(x => x.Publisher).SetValidator(publisherValidator);
    }
}