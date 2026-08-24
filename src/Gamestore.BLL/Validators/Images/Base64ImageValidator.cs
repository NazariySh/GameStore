using FluentValidation;

namespace Gamestore.BLL.Validators.Images;

public class Base64ImageValidator : AbstractValidator<string>
{
    public Base64ImageValidator()
    {
        RuleFor(url => url)
            .NotEmpty().WithMessage("Image URL cannot be empty.")
            .Matches(Base64ImageValidationRules.FormatPattern())
            .WithMessage(Base64ImageValidationRules.FormatPatternPatternMessage)
            .Must(HasAllowedImageFormat)
            .WithMessage($"Image format is not allowed. Allowed formats: {string.Join(", ", Base64ImageValidationRules.AllowedImageFormats)}");
    }

    private static bool HasAllowedImageFormat(string url)
    {
        var match = Base64ImageValidationRules.FormatPattern().Match(url);
        if (!match.Success)
        {
            return false;
        }

        var contentType = match.Groups[1].Value;
        return Base64ImageValidationRules.AllowedImageFormats.Contains(contentType);
    }
}