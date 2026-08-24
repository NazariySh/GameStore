using FluentValidation;
using Gamestore.BLL.DTOs.Users.Roles;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Constants;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Validators.Users.Roles;

public class BaseRoleRequestValidator : AbstractValidator<CreateUpdateRoleRequest>
{
    private readonly IRepository<Role> _roleRepository;

    public BaseRoleRequestValidator(
        IRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;

        RuleFor(x => x.Permissions)
            .NotEmpty().WithMessage("At least one permission is required.")
            .ForEach(p => p
                .NotEmpty().WithMessage("Permission cannot be empty.")
                .MustAsync(BeValidPermissionAsync)
                .WithMessage((_, permission) => $"Permission '{permission}' is not recognized."));
    }

    private Task<bool> BeValidPermissionAsync(string permission, CancellationToken cancellationToken)
    {
        return _roleRepository.ExistsAsync(
            r => r.RoleClaims.Any(rc => rc.ClaimType == CustomClaimTypes.Permission && rc.ClaimValue == permission),
            cancellationToken);
    }
}