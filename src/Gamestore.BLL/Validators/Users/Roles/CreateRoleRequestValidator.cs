using FluentValidation;
using Gamestore.BLL.DTOs.Users.Roles;

namespace Gamestore.BLL.Validators.Users.Roles;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator(
        IValidator<CreateUpdateRoleRequest> baseValidator,
        IValidator<RoleCreateDto> roleValidator)
    {
        Include(baseValidator);

        RuleFor(x => x.Role).SetValidator(roleValidator);
    }
}