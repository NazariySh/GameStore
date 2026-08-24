using FluentValidation;
using Gamestore.BLL.DTOs.Users;

namespace Gamestore.BLL.Validators.Users;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(
        IValidator<CreateUpdateUserRequest> baseValidator,
        IValidator<UserCreateDto> userValidator)
    {
        Include(baseValidator);

        RuleFor(x => x.User).SetValidator(userValidator);
    }
}