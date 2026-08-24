using FluentValidation;
using Gamestore.BLL.DTOs.Users;
using Gamestore.DAL.Repositories.Interfaces;
using Gamestore.Domain.Entities.Users;

namespace Gamestore.BLL.Validators.Users;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    private readonly IRepository<User> _userRepository;

    public UserUpdateDtoValidator(
        IRepository<User> userRepository,
        IValidator<UserCreateUpdateDto> baseValidator)
    {
        _userRepository = userRepository;

        Include(baseValidator);

        RuleFor(x => x)
            .MustAsync(BeUniqueNameAsync)
            .WithMessage("User with this name already exists.")
            .OverridePropertyName(x => x.Name);
    }

    private Task<bool> BeUniqueNameAsync(UserUpdateDto user, CancellationToken cancellationToken)
    {
        var normalizedName = user.Name.ToUpperInvariant();
        return _userRepository.NotExistsAsync(
            u => u.NormalizedUserName == normalizedName && u.Id != user.Id,
            cancellationToken);
    }
}