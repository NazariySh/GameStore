using FluentValidation;
using Gamestore.BLL.DTOs.Users;

namespace Gamestore.BLL.Validators.Users;

public class BaseUserDtoValidator : AbstractValidator<UserCreateUpdateDto>
{
    public BaseUserDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(UserValidationRules.MinUserNameLength)
            .WithMessage($"Username must be at least {UserValidationRules.MinUserNameLength} characters long.")
            .MaximumLength(UserValidationRules.MaxUserNameLength)
            .WithMessage($"Username must not exceed {UserValidationRules.MaxUserNameLength} characters.")
            .Matches(UserValidationRules.UserNamePattern())
            .WithMessage(UserValidationRules.UserNamePatternMessage);
    }
}