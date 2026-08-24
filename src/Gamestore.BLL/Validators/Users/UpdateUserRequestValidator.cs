using FluentValidation;
using Gamestore.BLL.DTOs.Users;

namespace Gamestore.BLL.Validators.Users;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator(
        IValidator<CreateUpdateUserRequest> baseValidator,
        IValidator<UserUpdateDto> userValidator)
    {
        Include(baseValidator);

        RuleFor(x => x.User).SetValidator(userValidator);
    }
}