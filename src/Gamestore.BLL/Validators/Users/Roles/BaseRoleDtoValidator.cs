using FluentValidation;
using Gamestore.BLL.DTOs.Users.Roles;

namespace Gamestore.BLL.Validators.Users.Roles;

public class BaseRoleDtoValidator : AbstractValidator<RoleCreateUpdateDto>
{
    public BaseRoleDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(RoleValidationRules.MinNameLength)
            .WithMessage($"Name must be at least {RoleValidationRules.MinNameLength} characters long.")
            .MaximumLength(RoleValidationRules.MaxNameLength)
            .WithMessage($"Name must not exceed {RoleValidationRules.MaxNameLength} characters.")
            .Matches(RoleValidationRules.NamePattern())
            .WithMessage(RoleValidationRules.NamePatternMessage);
    }
}