using FluentValidation;
using Gamestore.BLL.DTOs.Users.Roles;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Validators.Users.Roles;

public class RoleUpdateDtoValidator : AbstractValidator<RoleUpdateDto>
{
    private readonly IRepository<Role> _roleRepository;

    public RoleUpdateDtoValidator(
        IRepository<Role> roleRepository,
        IValidator<RoleCreateUpdateDto> baseValidator)
    {
        _roleRepository = roleRepository;

        Include(baseValidator);

        RuleFor(x => x)
            .MustAsync(BeUniqueNameAsync)
            .WithMessage("Role with this name already exists.")
            .OverridePropertyName(x => x.Name);
    }

    private Task<bool> BeUniqueNameAsync(RoleUpdateDto role, CancellationToken cancellationToken)
    {
        var normalizedName = role.Name.ToUpperInvariant();
        return _roleRepository.NotExistsAsync(
            r => r.NormalizedName == normalizedName && r.Id != role.Id,
            cancellationToken);
    }
}