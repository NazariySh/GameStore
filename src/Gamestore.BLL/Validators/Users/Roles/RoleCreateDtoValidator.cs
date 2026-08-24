using FluentValidation;
using Gamestore.BLL.DTOs.Users.Roles;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Validators.Users.Roles;

public class RoleCreateDtoValidator : AbstractValidator<RoleCreateDto>
{
    private readonly IRepository<Role> _roleRepository;

    public RoleCreateDtoValidator(
        IRepository<Role> roleRepository,
        IValidator<RoleCreateUpdateDto> baseValidator)
    {
        _roleRepository = roleRepository;

        Include(baseValidator);

        RuleFor(x => x.Name)
            .MustAsync(BeUniqueNameAsync)
            .WithMessage("Role with this name already exists.");
    }

    private Task<bool> BeUniqueNameAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = name.ToUpperInvariant();
        return _roleRepository.NotExistsAsync(
            r => r.NormalizedName == normalizedName,
            cancellationToken);
    }
}