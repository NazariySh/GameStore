using FluentValidation;
using Gamestore.BLL.DTOs.Games.Publishers;

namespace Gamestore.BLL.Validators.Games.Publishers;

public class CreatePublisherRequestValidator : AbstractValidator<CreatePublisherRequest>
{
    public CreatePublisherRequestValidator(IValidator<PublisherCreateDto> publisherValidator)
    {
        RuleFor(x => x.Publisher).SetValidator(publisherValidator);
    }
}