using FluentValidation;
using Gamestore.BLL.DTOs.Games.Publishers;

namespace Gamestore.BLL.Validators.Games.Publishers;

public class BasePublisherDtoValidator : AbstractValidator<PublisherCreateUpdateDto>
{
    public BasePublisherDtoValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company Name is required.")
            .MinimumLength(PublisherValidationRules.MinCompanyNameLength)
            .WithMessage($"Company Name must be at least {PublisherValidationRules.MinCompanyNameLength} characters long.")
            .MaximumLength(PublisherValidationRules.MaxCompanyNameLength)
            .WithMessage($"Company Name must not exceed {PublisherValidationRules.MaxCompanyNameLength} characters.")
            .Matches(PublisherValidationRules.CompanyNamePattern())
            .WithMessage(PublisherValidationRules.CompanyNamePatternMessage);

        RuleFor(x => x.HomePage)
            .MaximumLength(PublisherValidationRules.MaxHomePageLength)
            .WithMessage($"Home Page must not exceed {PublisherValidationRules.MaxHomePageLength} characters.")
            .Must(BeValidUrl)
            .WithMessage("Home Page must be a valid URL.")
            .When(NotEmptyHomePage);

        RuleFor(x => x.Description)
            .MaximumLength(PublisherValidationRules.MaxDescriptionLength)
            .WithMessage($"Description must not exceed {PublisherValidationRules.MaxDescriptionLength} characters.")
            .When(NotEmptyDescription);
    }

    private static bool NotEmptyHomePage(PublisherCreateUpdateDto publisher)
    {
        return !string.IsNullOrEmpty(publisher.HomePage);
    }

    private static bool NotEmptyDescription(PublisherCreateUpdateDto publisher)
    {
        return !string.IsNullOrEmpty(publisher.Description);
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}