using FluentValidation;
using Gamestore.BLL.DTOs.Users.Roles;

namespace Gamestore.BLL.Validators.Users.Roles;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator(
        IValidator<CreateUpdateRoleRequest> baseValidator,
        IValidator<RoleUpdateDto> roleValidator)
    {
        Include(baseValidator);

        RuleFor(x => x.Role).SetValidator(roleValidator);
    }
}