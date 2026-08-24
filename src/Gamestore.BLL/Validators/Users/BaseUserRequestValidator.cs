using FluentValidation;
using Gamestore.BLL.DTOs.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Validators.Users;

public class BaseUserRequestValidator : AbstractValidator<CreateUpdateUserRequest>
{
    private readonly IRepository<Role> _roleRepository;

    public BaseUserRequestValidator(
        IRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;

        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("At least one role is required.")
            .ForEach(role => role
                .NotEmpty().WithMessage("Role Id is required.")
                .MustAsync(BeValidRoleIdAsync)
                .WithMessage((_, roleId) => $"Role with Id {roleId} does not exist."));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(UserValidationRules.MinPasswordLength)
            .WithMessage($"Password must be at least {UserValidationRules.MinPasswordLength} characters long.")
            .MaximumLength(UserValidationRules.MaxPasswordLength)
            .WithMessage($"Password must not exceed {UserValidationRules.MaxPasswordLength} characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }

    private Task<bool> BeValidRoleIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _roleRepository.ExistsAsync(r => r.Id == id, cancellationToken);
    }
}